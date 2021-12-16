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

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Thing __instance, out bool __state, ref float __result)
        {
            if (__instance.def.tickerType != TickerType.Normal)
            {
                __state = false;
                return true;
            }
            if (!cachedResults.TryGetValue(__instance, out var cache))
            {
                cachedResults[__instance] = new CachedValueTick<float>(0, refreshRateStatic);
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
        public static void Postfix(Thing __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
