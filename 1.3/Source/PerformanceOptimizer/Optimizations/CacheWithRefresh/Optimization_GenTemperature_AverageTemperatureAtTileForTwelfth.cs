using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_GenTemperature_AverageTemperatureAtTileForTwelfth : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override int RefreshRateByDefault => 2000;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.GenTemperature_AverageTemperatureAtTileForTwelfth".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(GenTemperature), "AverageTemperatureAtTileForTwelfth", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<int, CachedValueTick<float>> cachedResults = new Dictionary<int, CachedValueTick<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(int tile, Twelfth twelfth, out CacheData __state, ref float __result)
        {
            var hashcode = 23;
            hashcode = (hashcode * 37) + ((int)twelfth);
            hashcode = (hashcode * 37) + tile;
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
        public static void Postfix(int tile, Twelfth twelfth, CacheData __state, float __result)
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
