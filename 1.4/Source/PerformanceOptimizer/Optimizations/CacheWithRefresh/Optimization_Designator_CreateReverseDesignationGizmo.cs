using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Designator_CreateReverseDesignationGizmo : Optimization_RefreshRate
    {
        public static Dictionary<Designator, CachedObjectTick<Command_Action>> cachedResults = new();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.CreateReverseDesignationGizmo".Translate();
        public override int RefreshRateByDefault => 30;

        public static int refreshRateStatic;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Designator), "CreateReverseDesignationGizmo", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Designator __instance, out CachedObjectTick<Command_Action> __state, ref Command_Action __result)
        {
            if (!cachedResults.TryGetValue(__instance, out __state))
            {
                cachedResults[__instance] = __state = new CachedObjectTick<Command_Action>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedObjectTick<Command_Action> __state, ref Command_Action __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
