using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{

    public class Optimization_TicksToOpenNow : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override int RefreshRateByDefault => 10;
        public override string Label => "PO.TicksToOpenNow".Translate();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Building_Door), "get_TicksToOpenNow", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<Building_Door, CachedValueTick<int>> cachedResults = new Dictionary<Building_Door, CachedValueTick<int>>();

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Building_Door __instance, out CachedValueTick<int> __state, ref int __result)
        {
            if (!cachedResults.TryGetValue(__instance, out __state))
            {
                cachedResults[__instance] = __state = new CachedValueTick<int>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<int> __state, ref int __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
