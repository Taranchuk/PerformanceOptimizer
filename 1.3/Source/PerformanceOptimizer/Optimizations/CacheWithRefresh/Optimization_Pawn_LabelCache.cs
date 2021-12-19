using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Pawn_LabelCache : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedObjectTick<string>> cachedResultsLabelNoCountCache = new Dictionary<Pawn, CachedObjectTick<string>>();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.PawnLabel".Translate();
        public override int RefreshRateByDefault => 60;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Pawn), "get_LabelNoCount", GetMethod(nameof(LabelNoCountCachePrefix)), GetMethod(nameof(LabelNoCountCachePostfix)));
            Patch(typeof(Pawn), "get_LabelShort", GetMethod(nameof(LabelShortCachePrefix)), GetMethod(nameof(LabelShortCachePostfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool LabelNoCountCachePrefix(Pawn __instance, out CachedObjectTick<string> __state, ref string __result)
        {
            if (!cachedResultsLabelNoCountCache.TryGetValue(__instance, out __state))
            {
                cachedResultsLabelNoCountCache[__instance] = __state = new CachedObjectTick<string>();
                return true;
            }
            return __state.TryRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void LabelNoCountCachePostfix(CachedObjectTick<string> __state, ref string __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public static Dictionary<Pawn, CachedObjectTick<string>> cachedResultsLabelShortCache = new Dictionary<Pawn, CachedObjectTick<string>>();

        [HarmonyPriority(int.MaxValue)]
        public static bool LabelShortCachePrefix(Pawn __instance, out CachedObjectTick<string> __state, ref string __result)
        {
            if (!cachedResultsLabelShortCache.TryGetValue(__instance, out __state))
            {
                cachedResultsLabelShortCache[__instance] = __state = new CachedObjectTick<string>();
                return true;
            }
            return __state.TryRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void LabelShortCachePostfix(Pawn __instance, CachedObjectTick<string> __state, ref string __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResultsLabelNoCountCache.Clear();
            cachedResultsLabelShortCache.Clear();
        }
    }
}
