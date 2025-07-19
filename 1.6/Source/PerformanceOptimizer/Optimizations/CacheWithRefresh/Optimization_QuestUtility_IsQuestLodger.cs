using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_QuestUtility_IsQuestLodger : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedValueTick<bool>> cachedResults = new();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.QuestLodger".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(QuestUtility), "IsQuestLodger", GetMethod(nameof(Optimization_QuestUtility_IsQuestLodger.Prefix)), GetMethod(nameof(Optimization_QuestUtility_IsQuestLodger.Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn p, out CachedValueTick<bool> __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(p, out __state))
            {
                cachedResults[p] = __state = new CachedValueTick<bool>();
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
