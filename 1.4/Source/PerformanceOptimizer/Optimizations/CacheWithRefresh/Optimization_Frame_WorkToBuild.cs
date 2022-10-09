using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Frame_WorkToBuild : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Frame, CachedValueTick<float>> cachedResults = new();
        public override int RefreshRateByDefault => 60;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.Frame_WorkToBuild".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Frame), "get_WorkToBuild", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Frame __instance, out CachedValueTick<float> __state, ref float __result)
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
