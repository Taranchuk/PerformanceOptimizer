using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public class Optimization_Plant_CheckTemperatureMakeLeafless : Optimization_RefreshRate
    {
        public static Dictionary<Plant, int> cachedResults = new Dictionary<Plant, int>();
        public override int RefreshRateByDefault => 3000;
        public override int MaxSliderValue => 6000;
        public override OptimizationType OptimizationType => OptimizationType.Throttle;
        public override string Name => "PO.Plant_CheckTemperatureMakeLeafless".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Plant), "CheckTemperatureMakeLeafless", GetMethod(nameof(Prefix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Plant __instance)
        {
            if (!cachedResults.TryGetValue(__instance, out var cache) || PerformanceOptimizerMod.tickManager.ticksGameInt > (cache + refreshRateStatic))
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
