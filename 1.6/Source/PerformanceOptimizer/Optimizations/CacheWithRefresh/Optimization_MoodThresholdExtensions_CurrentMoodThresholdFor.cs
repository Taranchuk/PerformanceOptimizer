using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_MoodThresholdExtensions_CurrentMoodThresholdFor : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedValueTick<MoodThreshold>> cachedResults = new();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.CurrentMoodThresholdFor".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(MoodThresholdExtensions), "CurrentMoodThresholdFor", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn pawn, out CachedValueTick<MoodThreshold> __state, ref MoodThreshold __result)
        {
            if (!cachedResults.TryGetValue(pawn, out __state))
            {
                cachedResults[pawn] = __state = new CachedValueTick<MoodThreshold>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<MoodThreshold> __state, ref MoodThreshold __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
