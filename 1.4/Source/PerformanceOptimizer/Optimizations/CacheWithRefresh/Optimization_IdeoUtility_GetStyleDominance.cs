using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_IdeoUtility_GetStyleDominance : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override int RefreshRateByDefault => 4000;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.GetStyleDominance".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(IdeoUtility), "GetStyleDominance", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<int, CachedValueTick<float>> cachedResults = new Dictionary<int, CachedValueTick<float>>();

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Thing t, Ideo ideo, out CachedValueTick<float> __state, ref float __result)
        {
            var hashcode = 23;
            hashcode = (hashcode * 37) + t.thingIDNumber;
            hashcode = (hashcode * 37) + ideo.id;
            if (!cachedResults.TryGetValue(hashcode, out __state))
            {
                cachedResults[hashcode] = __state = new CachedValueTick<float>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<float> __state, ref float __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
