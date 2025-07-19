using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_FormCaravanComp_AnyActiveThreatNow : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<FormCaravanComp, CachedValueTick<bool>> cachedResults = new();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.FormCaravanComp_AnyActiveThreatNow".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(AccessTools.Method(typeof(FormCaravanComp), "get_AnyActiveThreatNow"), GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(FormCaravanComp __instance, out CachedValueTick<bool> __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(__instance, out __state))
            {
                cachedResults[__instance] = __state = new CachedValueTick<bool>();
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
