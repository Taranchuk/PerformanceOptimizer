﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_PlantFallColors_GetFallColorFactor : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override int RefreshRateByDefault => 4000;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.PlantFallColors_GetFallColorFactor".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(PlantFallColors), "GetFallColorFactor", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<int, CachedValueTick<float>> cachedResults = new Dictionary<int, CachedValueTick<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(float latitude, int dayOfYear, out CacheData __state, ref float __result)
        {
            var hashcode = 23;
            hashcode = (hashcode * 37) + latitude.GetHashCode();
            hashcode = (hashcode * 37) + dayOfYear;
            if (!cachedResults.TryGetValue(hashcode, out var cache))
            {
                cachedResults[hashcode] = new CachedValueTick<float>(0, refreshRateStatic);
                __state = new CacheData { key = hashcode, state = true };
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = new CacheData { key = hashcode, state = true };
                return true;
            }
            else
            {
                __result = cache.valueInt;
                __state = new CacheData { key = hashcode, state = false };
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(CacheData __state, float __result)
        {
            if (__state.state)
            {
                cachedResults[__state.key].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
