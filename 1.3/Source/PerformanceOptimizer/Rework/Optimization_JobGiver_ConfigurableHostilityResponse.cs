﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_JobGiver_ConfigurableHostilityResponse : Optimization_RefreshRate
    {
        public static Dictionary<Pawn, int> cachedResults = new Dictionary<Pawn, int>();
        public override int RefreshRateByDefault => throw new System.NotImplementedException();
        public override OptimizationType OptimizationType => throw new System.NotImplementedException();
        public override string Name => throw new System.NotImplementedException();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGiveJob", GetMethod(nameof(Optimization_JobGiver_ConfigurableHostilityResponse.Prefix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn)
        {
            if (!cachedResults.TryGetValue(pawn, out var cache)
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
            throw new System.NotImplementedException();
        }
    }
}
