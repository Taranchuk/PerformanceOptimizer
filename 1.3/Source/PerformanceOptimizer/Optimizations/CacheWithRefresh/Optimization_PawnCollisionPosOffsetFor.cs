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

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn pawn, out CachedValueTick<Vector3> __state, ref Vector3 __result)
        {
            if (!cachedResults.TryGetValue(pawn, out __state))
            {
                cachedResults[pawn] = __state = new CachedValueTick<Vector3>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<Vector3> __state, ref Vector3 __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
