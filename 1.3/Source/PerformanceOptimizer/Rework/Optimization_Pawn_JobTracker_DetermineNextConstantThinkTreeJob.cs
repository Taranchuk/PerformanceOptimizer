using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public class Optimization_Pawn_JobTracker_DetermineNextConstantThinkTreeJob : Optimization_RefreshRate
    {
        public static Dictionary<Pawn, int> cachedResults = new Dictionary<Pawn, int>();
        public override int RefreshRateByDefault => throw new System.NotImplementedException();
        public override OptimizationType OptimizationType => throw new System.NotImplementedException();
        public override string Name => throw new System.NotImplementedException();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Pawn_JobTracker), "DetermineNextConstantThinkTreeJob", GetMethod(nameof(Prefix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn_JobTracker __instance)
        {
            if (__instance.pawn.factionInt != Faction.OfPlayer)
            {
                if (!cachedResults.TryGetValue(__instance.pawn, out var cache)
                    || PerformanceOptimizerMod.tickManager.ticksGameInt > (cache + refreshRateStatic))
                {
                    cachedResults[__instance.pawn] = PerformanceOptimizerMod.tickManager.ticksGameInt;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public override void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}
