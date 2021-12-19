using HarmonyLib;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public class Optimization_MentalBreaker_BreakThresholdMinor : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedValueTick<float>> cachedResults = new Dictionary<Pawn, CachedValueTick<float>>();
        public override int RefreshRateByDefault => 600;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.BreakThresholdMinor".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(MentalBreaker), "get_BreakThresholdMinor", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(MentalBreaker __instance, out CachedValueTick<float> __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out __state))
            {
                cachedResults[__instance.pawn] = __state = new CachedValueTick<float>();
                return true;
            }
            return __state.TryRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<float> __state, ref float __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
