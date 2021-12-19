using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{

    public class Optimization_FreePassage : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override int RefreshRateByDefault => 10;
        public override string Label => "PO.FreePassage".Translate();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Building_Door), "get_FreePassage", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<Building_Door, CachedValueTick<bool>> cachedResults = new Dictionary<Building_Door, CachedValueTick<bool>>();

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Building_Door __instance, out CachedValueTick<bool> __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(__instance, out __state))
            {
                cachedResults[__instance] = __state = new CachedValueTick<bool>();
                return true;
            }
            return __state.TryRefresh(ref __result);
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
