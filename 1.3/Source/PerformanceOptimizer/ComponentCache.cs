using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PerformanceOptimizer
{
	public static class CompsOfType<T>
	{
		public static Dictionary<Map, T> mapCompsByMap = new Dictionary<Map, T>();
	}
	[StaticConstructorOnStartup]
	public static class ComponentCache
	{
		//private static Stopwatch dictStopwatch = new Stopwatch();

		//public static Dictionary<Type, int> calledStats = new Dictionary<Type, int>();
		//private static void RegisterComp(Type type)
        //{
		//	if (calledStats.ContainsKey(type))
        //    {
		//		calledStats[type]++;
		//	}
		//	else
        //    {
		//		calledStats[type] = 1;
		//	}
        //}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetThingCompFast<T>(this ThingWithComps thingWithComps) where T : ThingComp
		{
			//dictStopwatch.Restart();
			if (thingWithComps.comps == null)
			{
				//dictStopwatch.LogTime("Dict approach: ");
				return null;
			}
			for (int i = 0; i < thingWithComps.comps.Count; i++)
			{
				if (thingWithComps.comps[i].GetType() == typeof(T))
				{
					//RegisterComp(thingWithComps.comps[i].GetType());
					//dictStopwatch.LogTime("Dict approach: ");
					return thingWithComps.comps[i] as T;
				}
			}

			for (int i = 0; i < thingWithComps.comps.Count; i++)
			{
				T val = thingWithComps.comps[i] as T;
				if (val != null)
				{   
					//dictStopwatch.LogTime("Dict approach: ");
					return val;
				}
			}

			//dictStopwatch.LogTime("Dict approach: ");
			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetWorldObjectCompFast<T>(this WorldObject worldObject) where T : WorldObjectComp
		{
			//dictStopwatch.Restart();
			if (worldObject.comps == null)
			{
				//dictStopwatch.LogTime("Dict approach: ");
				return null;
			}
			for (int i = 0; i < worldObject.comps.Count; i++)
			{
				if (worldObject.comps[i].GetType() == typeof(T))
				{
					//RegisterComp(thingWithComps.comps[i].GetType());
					//dictStopwatch.LogTime("Dict approach: ");
					return worldObject.comps[i] as T;
				}
			}

			for (int i = 0; i < worldObject.comps.Count; i++)
			{
				T val = worldObject.comps[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryGetThingCompFast<T>(this Thing thing) where T : ThingComp
		{
			ThingWithComps thingWithComps = thing as ThingWithComps;
			if (thingWithComps == null)
			{
				return null;
			}
			return thingWithComps.GetThingCompFast<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryGetHediffCompFast<T>(this Hediff hd) where T : HediffComp
		{
			HediffWithComps hediffWithComps = hd as HediffWithComps;
			if (hediffWithComps == null)
			{
				return null;
			}
			//dictStopwatch.Restart();
			if (hediffWithComps.comps == null)
			{
				//dictStopwatch.LogTime("Dict approach: ");
				return null;
			}

			for (int i = 0; i < hediffWithComps.comps.Count; i++)
			{
				if (hediffWithComps.comps[i].GetType() == typeof(T))
				{
					//RegisterComp(thingWithComps.comps[i].GetType());
					//dictStopwatch.LogTime("Dict approach: ");
					return hediffWithComps.comps[i] as T;
				}
			}

			for (int i = 0; i < hediffWithComps.comps.Count; i++)
			{
				T val = hediffWithComps.comps[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetMapComponentDict<T>(this Map map) where T : MapComponent
		{
			if (!CompsOfType<T>.mapCompsByMap.TryGetValue(map, out T mapComp) || mapComp is null)
			{
				CompsOfType<T>.mapCompsByMap[map] = mapComp = map.GetComponent<T>();
			}
			//Log.Message("Returning map comp: " + mapComp + ", total count of map comps is " + map.components.Count);
			return mapComp as T;
		}

		public static Dictionary<Type, WorldComponent> cachedWorldComps = new Dictionary<Type, WorldComponent>();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetWorldComponentDict<T>(this World world) where T : WorldComponent
		{
			var type = typeof(T);
			if (!cachedWorldComps.TryGetValue(type, out var worldComp) || worldComp is null)
			{
				cachedWorldComps[type] = worldComp = world.GetComponent<T>();
			}
			//Log.Message("Returning world comp: " + worldComp + ", total count of world comps is " + world.components.Count);
			return worldComp as T;
		}

		public static Dictionary<Type, GameComponent> cachedGameComps = new Dictionary<Type, GameComponent>();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetGameComponentDict<T>(this Game game) where T : GameComponent
		{
			var type = typeof(T);
			if (!cachedGameComps.TryGetValue(type, out var gameComp) || gameComp is null)
			{
				cachedGameComps[type] = gameComp = game.GetComponent<T>();
			}
			//Log.Message("Returning game comp: " + gameComp + ", total count of game comps is " + game.components.Count);
			return gameComp as T;
		}
	}
}
