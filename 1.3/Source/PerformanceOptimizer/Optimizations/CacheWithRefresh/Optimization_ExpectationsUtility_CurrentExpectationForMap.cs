using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_ExpectationsUtility_CurrentExpectationForMap : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Map, CachedValueTick<ExpectationDef>> cachedResults = new Dictionary<Map, CachedValueTick<ExpectationDef>>();
        public override int RefreshRateByDefault => 1000;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.CurrentExpectationForMap".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(AccessTools.Method(typeof(ExpectationsUtility), "CurrentExpectationFor", new Type[] { typeof(Map) }), GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Map m, out bool __state, ref ExpectationDef __result)
        {
            if (!cachedResults.TryGetValue(m, out var cache))
            {
                cachedResults[m] = new CachedValueTick<ExpectationDef>(null, refreshRateStatic);
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
        public static void Postfix(Map m, bool __state, ExpectationDef __result)
        {
            if (__state)
            {
                cachedResults[m].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
