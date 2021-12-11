using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_IdeoUtility_GetStyleDominance : Optimization_RefreshRate
    {
        public override int RefreshRateByDefault => throw new NotImplementedException();

        public override OptimizationType OptimizationType => throw new NotImplementedException();

        public override string Name => throw new NotImplementedException();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(IdeoUtility), "GetStyleDominance", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<int, CachedValueTick<float>> cachedResults = new Dictionary<int, CachedValueTick<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Thing t, Ideo ideo, out CacheData __state, ref float __result)
        {
            var hashcode = 23;
            hashcode = (hashcode * 37) + t.thingIDNumber;
            hashcode = (hashcode * 37) + ideo.id;
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
            throw new NotImplementedException();
        }
    }
}
