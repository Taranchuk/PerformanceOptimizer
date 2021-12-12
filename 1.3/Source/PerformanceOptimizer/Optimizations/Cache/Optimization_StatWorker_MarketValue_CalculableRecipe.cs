using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_StatWorker_MarketValue_CalculableRecipe : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Name => "PO.CacheStatWorker_MarketValue".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(StatWorker_MarketValue), "CalculableRecipe", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<BuildableDef, RecipeDef> cachedResults = new Dictionary<BuildableDef, RecipeDef>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(BuildableDef def, out bool __state, ref RecipeDef __result)
        {
            if (!cachedResults.TryGetValue(def, out var cache))
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache;
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(BuildableDef def, bool __state, RecipeDef __result)
        {
            if (__state)
            {
                cachedResults[def] = __result;
            }
        }
    }
}
