using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_PawnUtility_IsTeetotaler : Optimization_RefreshRate
    {
        public static Dictionary<Pawn, CachedValueTick<bool>> cachedResults = new Dictionary<Pawn, CachedValueTick<bool>>();
        public override int RefreshRateByDefault => throw new NotImplementedException();
        public override OptimizationType OptimizationType => throw new NotImplementedException();
        public override string Name => throw new NotImplementedException();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(PawnUtility), "IsTeetotaler", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn, out bool __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(pawn, out var cache))
            {
                cachedResults[pawn] = new CachedValueTick<bool>(false, refreshRateStatic);
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
        public static void Postfix(Pawn pawn, bool __state, bool __result)
        {
            if (__state)
            {
                cachedResults[pawn].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
