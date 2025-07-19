using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Building_Door_CheckClearReachabilityCacheBecauseOpenedOrClosed : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public static Dictionary<Building_Door, int> cachedResults = new();
        public override int RefreshRateByDefault => 60;
        public override OptimizationType OptimizationType => OptimizationType.Throttle;
        public override string Label => "PO.Building_Door_CheckClearReachabilityCacheBecauseOpenedOrClosed".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Building_Door), "CheckClearReachabilityCacheBecauseOpenedOrClosed", GetMethod(nameof(Prefix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Building_Door __instance)
        {
            if (!cachedResults.TryGetValue(__instance, out int cache)
                || PerformanceOptimizerMod.tickManager.ticksGameInt > (cache + refreshRateStatic))
            {
                cachedResults[__instance] = PerformanceOptimizerMod.tickManager.ticksGameInt;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
