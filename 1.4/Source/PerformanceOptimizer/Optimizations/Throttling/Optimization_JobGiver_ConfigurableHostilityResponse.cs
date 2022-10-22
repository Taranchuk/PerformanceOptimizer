using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_JobGiver_ConfigurableHostilityResponse : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, int> cachedResults = new();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.Throttle;
        public override string Label => "PO.JobGiver_ConfigurableHostilityResponse".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGiveJob", GetMethod(nameof(Optimization_JobGiver_ConfigurableHostilityResponse.Prefix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn pawn)
        {
            if (!cachedResults.TryGetValue(pawn, out int cache)
                || PerformanceOptimizerMod.tickManager.ticksGameInt > (cache + refreshRateStatic))
            {
                cachedResults[pawn] = PerformanceOptimizerMod.tickManager.ticksGameInt;
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
