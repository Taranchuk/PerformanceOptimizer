using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public class Optimization_Need_Beauty_CurrentInstantBeauty : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Need_Beauty, CachedValueTick<float>> cachedResults = new Dictionary<Need_Beauty, CachedValueTick<float>>();
        public override int RefreshRateByDefault => 600;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;

        public override string Label => "PO.CurrentInstantBeauty".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Need_Beauty), "CurrentInstantBeauty", GetMethod(nameof(Optimization_Need_Beauty_CurrentInstantBeauty.Prefix)), GetMethod(nameof(Optimization_Need_Beauty_CurrentInstantBeauty.Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Need_Beauty __instance, out CachedValueTick<float> __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance, out __state))
            {
                cachedResults[__instance] = __state = new CachedValueTick<float>();
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
