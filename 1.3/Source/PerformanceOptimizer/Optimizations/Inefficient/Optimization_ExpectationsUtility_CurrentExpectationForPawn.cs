using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_ExpectationsUtility_CurrentExpectationForPawn : Optimization_RefreshRate
    {
        public static Dictionary<Pawn, CachedValueTick<ExpectationDef>> cachedResults = new Dictionary<Pawn, CachedValueTick<ExpectationDef>>();
        public override int RefreshRateByDefault => 10000;

        public static int refreshRateStatic;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.CurrentExpectationForPawn".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(AccessTools.Method(typeof(ExpectationsUtility), "CurrentExpectationFor", new Type[] { typeof(Pawn) }), GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn p, out bool __state, ref ExpectationDef __result)
        {
            if (!cachedResults.TryGetValue(p, out var cache))
            {
                cachedResults[p] = new CachedValueTick<ExpectationDef>(null, refreshRateStatic);
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
        public static void Postfix(Pawn p, bool __state, ExpectationDef __result)
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
