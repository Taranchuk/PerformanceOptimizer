using HarmonyLib;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_WorldGrid_LongLatOf : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Label => "PO.WorldGrid_LongLatOf".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(WorldGrid), "LongLatOf", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<int, Vector2> cachedResults = new();
        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(int tileID, out bool __state, ref Vector2 __result)
        {
            if (!cachedResults.TryGetValue(tileID, out Vector2 cache))
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache;
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(int tileID, bool __state, Vector2 __result)
        {
            if (__state)
            {
                cachedResults[tileID] = __result;
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
