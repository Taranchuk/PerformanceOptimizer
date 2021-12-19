using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_GenCelestial_CurCelestialSunGlow : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Map, CachedValueTick<float>> cachedResults = new Dictionary<Map, CachedValueTick<float>>();
        public override int RefreshRateByDefault => 60;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.GenCelestial_CurCelestialSunGlow".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(AccessTools.Method(typeof(GenCelestial), "CurCelestialSunGlow", new Type[] { typeof(Map) }), GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Map map, out CachedValueTick<float> __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(map, out __state))
            {
                cachedResults[map] = __state = new CachedValueTick<float>(0, refreshRateStatic);
                return true;
            }
            return __state.TryRefresh(ref __result);
        }

        [HarmonyPriority(Priority.Last)]
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
