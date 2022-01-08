using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_PawnUtility_IsInvisible : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedValueTick<bool>> cachedResults = new Dictionary<Pawn, CachedValueTick<bool>>();
        public override int RefreshRateByDefault => 10;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.IsInvisible".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(PawnUtility), "IsInvisible", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn pawn, out CachedValueTick<bool> __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(pawn, out __state))
            {
                cachedResults[pawn] = __state = new CachedValueTick<bool>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<bool> __state, ref bool __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
