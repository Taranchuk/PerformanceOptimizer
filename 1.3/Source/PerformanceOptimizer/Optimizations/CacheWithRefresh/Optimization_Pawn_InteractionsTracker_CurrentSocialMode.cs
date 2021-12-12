using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Pawn_InteractionsTracker_CurrentSocialMode : Optimization_RefreshRate
    {
        public static Dictionary<Pawn, CachedValueTick<RandomSocialMode>> cachedResults = new Dictionary<Pawn, CachedValueTick<RandomSocialMode>>();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Name => "PO.Pawn_InteractionsTracker_CurrentSocialMode".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Pawn_InteractionsTracker), "get_CurrentSocialMode", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn ___pawn, out bool __state, ref RandomSocialMode __result)
        {
            if (!cachedResults.TryGetValue(___pawn, out var cache))
            {
                cachedResults[___pawn] = new CachedValueTick<RandomSocialMode>(RandomSocialMode.Normal, refreshRateStatic);
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
        public static void Postfix(Pawn ___pawn, bool __state, RandomSocialMode __result)
        {
            if (__state)
            {
                cachedResults[___pawn].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
