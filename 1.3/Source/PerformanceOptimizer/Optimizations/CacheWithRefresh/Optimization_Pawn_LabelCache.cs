using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Pawn_LabelCache : Optimization_RefreshRate
    {
        public static Dictionary<Pawn, CachedValueTick<string>> cachedResultsLabelNoCountCache = new Dictionary<Pawn, CachedValueTick<string>>();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Name => "PO.PawnLabel".Translate();
        public override int RefreshRateByDefault => 30;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Pawn), "get_LabelNoCount", GetMethod(nameof(LabelNoCountCachePrefix)), GetMethod(nameof(LabelNoCountCachePostfix)));
            Patch(typeof(Pawn), "get_LabelShort", GetMethod(nameof(LabelShortCachePrefix)), GetMethod(nameof(LabelShortCachePostfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool LabelNoCountCachePrefix(Pawn __instance, out bool __state, ref string __result)
        {
            if (!cachedResultsLabelNoCountCache.TryGetValue(__instance, out var cache))
            {
                cachedResultsLabelNoCountCache[__instance] = new CachedValueTick<string>(default, refreshRateStatic);
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
        public static void LabelNoCountCachePostfix(Pawn __instance, bool __state, ref string __result)
        {
            if (__state)
            {
                cachedResultsLabelNoCountCache[__instance].SetValue(__result, refreshRateStatic);
            }
        }

        public static Dictionary<Pawn, CachedValueTick<string>> cachedResultsLabelShortCache = new Dictionary<Pawn, CachedValueTick<string>>();

        [HarmonyPriority(Priority.First)]
        public static bool LabelShortCachePrefix(Pawn __instance, out bool __state, ref string __result)
        {
            if (!cachedResultsLabelShortCache.TryGetValue(__instance, out var cache))
            {
                cachedResultsLabelShortCache[__instance] = new CachedValueTick<string>(default, refreshRateStatic);
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
        public static void LabelShortCachePostfix(Pawn __instance, bool __state, ref string __result)
        {
            if (__state)
            {
                cachedResultsLabelShortCache[__instance].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResultsLabelNoCountCache.Clear();
            cachedResultsLabelShortCache.Clear();
        }
    }
}
