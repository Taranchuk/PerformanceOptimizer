using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public class Optimization_MentalBreaker_BreakThresholdExtreme : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedValueTick<float>> cachedResults = new Dictionary<Pawn, CachedValueTick<float>>();
        public override int RefreshRateByDefault => 300;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Name => "PO.BreakThresholdExtreme".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(MentalBreaker), "get_BreakThresholdExtreme", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(MentalBreaker __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new CachedValueTick<float>(0, refreshRateStatic);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.valueInt;
                __state = false;
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(MentalBreaker __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
