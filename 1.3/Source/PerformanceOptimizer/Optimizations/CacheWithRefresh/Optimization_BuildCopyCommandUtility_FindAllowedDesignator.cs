using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_BuildCopyCommandUtility_FindAllowedDesignator : Optimization_RefreshRate
    {
        public static Dictionary<BuildableDef, CachedObjectTick<Designator_Build>> cachedResults = new Dictionary<BuildableDef, CachedObjectTick<Designator_Build>>();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.FindAllowedDesignator".Translate();
        public override int RefreshRateByDefault => 120;

        public static int refreshRateStatic;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(BuildCopyCommandUtility), "FindAllowedDesignator", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(BuildableDef buildable, out CachedObjectTick<Designator_Build> __state, ref Designator_Build __result)
        {
            if (!cachedResults.TryGetValue(buildable, out __state))
            {
                cachedResults[buildable] = __state = new CachedObjectTick<Designator_Build>();
                return true;
            }
            return __state.TryRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedObjectTick<Designator_Build> __state, ref Designator_Build __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
