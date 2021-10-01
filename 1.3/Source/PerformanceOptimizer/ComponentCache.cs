using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PerformanceOptimizer
{
	[StaticConstructorOnStartup]
	public static class ComponentCache
	{
		private static Stopwatch dictStopwatch = new Stopwatch();

		public static Dictionary<Type, ThingComp> cachedThingComps = new Dictionary<Type, ThingComp>();
		public static T GetThingCompDict<T>(this ThingWithComps thingWithComps) where T : ThingComp
		{
			//dictStopwatch.Restart();
			var type = typeof(T);
			if (!cachedThingComps.TryGetValue(type, out var thingComp) || thingComp is null)
			{
				cachedThingComps[type] = thingComp = thingWithComps.GetComp<T>();
			}
			//dictStopwatch.LogTime("Dict approach: ");
			//Log.Message("Returning thing comp: " + thingComp + ", total count of thing comps is " + (thingWithComps.comps?.Count ?? 0));
			return thingComp as T;
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

		private static Dictionary<Type, MapComponent> cachedMapComps = new Dictionary<Type, MapComponent>();
		public static T GetMapComponent<T>(this Map map) where T : MapComponent
		{
			var type = typeof(T);
			if (!cachedMapComps.TryGetValue(type, out var mapComp))
			{
				cachedMapComps[type] = mapComp = map.GetComponent<T>();
			}
			//Log.Message("Returning map comp: " + mapComp + ", total count of map comps is " + map.components.Count);
			return mapComp as T;
		}

		private static Dictionary<Type, WorldComponent> cachedWorldComps = new Dictionary<Type, WorldComponent>();
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
