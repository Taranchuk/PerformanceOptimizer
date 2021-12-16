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
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn ___pawn, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(___pawn, out var cache))
            {
                cachedResults[___pawn] = new CachedValueTick<float>(0, refreshRateStatic);
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
        public static void Postfix(Pawn ___pawn, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[___pawn].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
