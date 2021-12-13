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
        public static bool Prefix(Map map, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(map, out var cache))
            {
                cachedResults[map] = new CachedValueTick<float>(0, refreshRateStatic);
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
        public static void Postfix(Map map, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[map].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
