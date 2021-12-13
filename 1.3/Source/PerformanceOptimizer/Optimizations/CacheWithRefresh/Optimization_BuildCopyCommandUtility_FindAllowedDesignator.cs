using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_BuildCopyCommandUtility_FindAllowedDesignator : Optimization_RefreshRate
    {
        public static Dictionary<BuildableDef, CachedValueTick<Designator_Build>> cachedResults = new Dictionary<BuildableDef, CachedValueTick<Designator_Build>>();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.FindAllowedDesignator".Translate();
        public override int RefreshRateByDefault => 120;

        public static int refreshRateStatic;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(BuildCopyCommandUtility), "FindAllowedDesignator", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(BuildableDef buildable, out bool __state, ref Designator_Build __result)
        {
            if (!cachedResults.TryGetValue(buildable, out var cache))
            {
                cachedResults[buildable] = new CachedValueTick<Designator_Build>(default, refreshRateStatic);
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
        public static void Postfix(BuildableDef buildable, bool __state, Designator_Build __result)
        {
            if (__state)
            {
                cachedResults[buildable].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
