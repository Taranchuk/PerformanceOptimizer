using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Thing_AmbientTemperature : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.AmbientTemperature".Translate();
        public override int RefreshRateByDefault => 240;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Thing), "get_AmbientTemperature", GetMethod(nameof(Optimization_Thing_AmbientTemperature.Prefix)), GetMethod(nameof(Optimization_Thing_AmbientTemperature.Postfix)));
        }

        public static Dictionary<Thing, CachedValueTick<float>> cachedResults = new Dictionary<Thing, CachedValueTick<float>>();

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Thing __instance, out CachedValueTick<float> __state, ref float __result)
        {
            if (__instance.def.tickerType != TickerType.Normal)
            {
                __state = null;
                return true;
            }
            if (!cachedResults.TryGetValue(__instance, out __state))
            {
                cachedResults[__instance] = __state = new CachedValueTick<float>();
                return true;
            }
            return __state.TryRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<float> __state, ref float __result)
        {
            if (__state != null)
            {
                __state.ProcessResult(ref __result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
