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
		public static Dictionary<ThingWithComps, T> thingCompsByThing = new Dictionary<ThingWithComps, T>();
		public static Dictionary<Map, T> mapCompsByMap = new Dictionary<Map, T>();
	}
	[StaticConstructorOnStartup]
	public static class ComponentCache
	{
		private static Stopwatch dictStopwatch = new Stopwatch();

		public static Dictionary<Type, ThingComp>[] thingCompsByThings = new Dictionary<Type, ThingComp>[999999];

		public static Dictionary<int, ThingComp> cachedThingComps = new Dictionary<int, ThingComp>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetThingCompDict<T>(this ThingWithComps thingWithComps) where T : ThingComp
		{
			dictStopwatch.Restart();
			//var thingComps = thingCompsByThings[thingWithComps.thingIDNumber];
			//if (thingComps is null)
            //{
			//	var dict = new Dictionary<Type, ThingComp>();
			//	if (thingWithComps.comps != null)
            //    {
			//		foreach (var comp in thingWithComps.comps)
			//		{
			//			dict[comp.GetType()] = comp;
			//		}
			//	}
			//	thingCompsByThings[thingWithComps.thingIDNumber] = thingComps = dict;
			//}
			//var type = typeof(T);
			//if (!thingComps.TryGetValue(type, out var thingComp))
            //{
			//	thingComps[type] = thingComp = thingWithComps.GetComp<T>();
			//}

			if (!CompsOfType<T>.thingCompsByThing.TryGetValue(thingWithComps, out T thingComp))
            {
				CompsOfType<T>.thingCompsByThing[thingWithComps] = thingComp = thingWithComps.GetComp<T>();
			}
			dictStopwatch.LogTime("Dict approach: ");
			//Log.Message("Returning thing comp: " + thingComp + ", total count of thing comps is " + (thingWithComps.comps?.Count ?? 0));
			return thingComp;
		}

		private static Stopwatch vanillaStopwatch = new Stopwatch();
		public static T GetCompVanilla<T>(this ThingWithComps thingWithComps) where T : ThingComp
		{
			vanillaStopwatch.Restart();
			if (thingWithComps.comps != null)
			{
				int i = 0;
				for (int count = thingWithComps.comps.Count; i < count; i++)
				{
					T val = thingWithComps.comps[i] as T;
					if (val != null)
					{
						vanillaStopwatch.LogTime("Vanilla approach: ");
						return val;
					}
				}
			}
			vanillaStopwatch.LogTime("Vanilla approach: ");
			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T TryGetCompDict<T>(this Thing thing) where T : ThingComp
		{
			ThingWithComps thingWithComps = thing as ThingWithComps;
			if (thingWithComps == null)
			{
				return null;
			}
			return thingWithComps.GetThingCompDict<T>();
		}

		public static T TryGetCompVanilla<T>(this Thing thing) where T : ThingComp
		{
			ThingWithComps thingWithComps = thing as ThingWithComps;
			if (thingWithComps == null)
			{
				return null;
			}
			return thingWithComps.GetCompVanilla<T>();
		}

		private static Dictionary<int, MapComponent> cachedMapComps = new Dictionary<int, MapComponent>();
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetMapComponent<T>(this Map map) where T : MapComponent
		{
			if (!CompsOfType<T>.mapCompsByMap.TryGetValue(map, out T mapComp))
			{
				CompsOfType<T>.mapCompsByMap[map] = mapComp = map.GetComponent<T>();
			}
			//Log.Message("Returning map comp: " + mapComp + ", total count of map comps is " + map.components.Count);
			return mapComp as T;
		}

		private static Dictionary<Type, WorldComponent> cachedWorldComps = new Dictionary<Type, WorldComponent>();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetWorldComponent<T>(this World world) where T : WorldComponent
		{
			var type = typeof(T);
			if (!cachedWorldComps.TryGetValue(type, out var worldComp))
			{
				cachedWorldComps[type] = worldComp = world.GetComponent<T>();
			}
			//Log.Message("Returning world comp: " + worldComp + ", total count of world comps is " + world.components.Count);
			return worldComp as T;
		}

		private static Dictionary<Type, GameComponent> cachedGameComps = new Dictionary<Type, GameComponent>();
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetGameComponent<T>(this Game game) where T : GameComponent
		{
			var type = typeof(T);
			if (!cachedGameComps.TryGetValue(type, out var gameComp))
			{
				cachedGameComps[type] = gameComp = game.GetComponent<T>();
			}
			//Log.Message("Returning game comp: " + gameComp + ", total count of game comps is " + game.components.Count);
			return gameComp as T;
		}
		public static void ResetComps()
		{
			cachedMapComps.Clear();
			cachedWorldComps.Clear();
			cachedGameComps.Clear();
			cachedThingComps.Clear();
		}
	}
}
