using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_PawnCollisionPosOffsetFor : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.PawnCollisionPosOffsetFor".Translate();
        public override int RefreshRateByDefault => 30;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(PawnCollisionTweenerUtility), "PawnCollisionPosOffsetFor", GetMethod(nameof(Optimization_PawnCollisionPosOffsetFor.Prefix)), GetMethod(nameof(Optimization_PawnCollisionPosOffsetFor.Postfix)));
        }

        public static Dictionary<Pawn, CachedValueTick<Vector3>> cachedResults = new Dictionary<Pawn, CachedValueTick<Vector3>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn, out bool __state, ref Vector3 __result)
        {
            if (!cachedResults.TryGetValue(pawn, out var cache))
            {
                cachedResults[pawn] = new CachedValueTick<Vector3>(default, refreshRateStatic);
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
        public static void Postfix(Pawn pawn, bool __state, ref Vector3 __result)
        {
            if (__state)
            {
                cachedResults[pawn].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
