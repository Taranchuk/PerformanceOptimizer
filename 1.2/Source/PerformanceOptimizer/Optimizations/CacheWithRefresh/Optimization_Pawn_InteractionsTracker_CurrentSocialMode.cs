using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Pawn_InteractionsTracker_CurrentSocialMode : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<int, CachedValueTick<RandomSocialMode>> cachedResults = new();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.Pawn_InteractionsTracker_CurrentSocialMode".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Pawn_InteractionsTracker), "get_CurrentSocialMode", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn ___pawn, out CachedValueTick<RandomSocialMode> __state, ref RandomSocialMode __result)
        {
            if (!cachedResults.TryGetValue(___pawn.thingIDNumber, out __state))
            {
                cachedResults[___pawn.thingIDNumber] = __state = new CachedValueTick<RandomSocialMode>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<RandomSocialMode> __state, ref RandomSocialMode __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }
        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
