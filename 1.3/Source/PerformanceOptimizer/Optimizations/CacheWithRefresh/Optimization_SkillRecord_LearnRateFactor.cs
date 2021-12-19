using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_SkillRecord_LearnRateFactor : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedValueTick<float>> cachedResults = new Dictionary<Pawn, CachedValueTick<float>>();
        public override int RefreshRateByDefault => 300;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.SkillRecord_LearnRateFactor".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(SkillRecord), "LearnRateFactor", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }
        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn ___pawn, out CachedValueTick<float> __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(___pawn, out __state))
            {
                cachedResults[___pawn] = __state = new CachedValueTick<float>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(Pawn ___pawn, CachedValueTick<float> __state, ref float __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
