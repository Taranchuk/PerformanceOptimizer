using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_QuestUtility_IsQuestLodger : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Pawn, CachedValueTick<bool>> cachedResults = new Dictionary<Pawn, CachedValueTick<bool>>();
        public override int RefreshRateByDefault => 30;

        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.QuestLodger".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(QuestUtility), "IsQuestLodger", GetMethod(nameof(Optimization_QuestUtility_IsQuestLodger.Prefix)), GetMethod(nameof(Optimization_QuestUtility_IsQuestLodger.Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn p, out bool __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(p, out var cache))
            {
                cachedResults[p] = new CachedValueTick<bool>(false, refreshRateStatic);
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
        public static void Postfix(Pawn p, bool __state, bool __result)
        {
            if (__state)
            {
                cachedResults[p].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
