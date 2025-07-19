using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_PawnUtility_IsInvisible : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<int, CachedValueTick<bool>> cachedResults = new();
        public override int RefreshRateByDefault => 60;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.IsInvisible".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(InvisibilityUtility), "IsPsychologicallyInvisible", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
            Patch(typeof(HediffComp_Invisibility), "UpdateTarget", GetMethod(nameof(ClearCache)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn pawn, out CachedValueTick<bool> __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(pawn.thingIDNumber, out __state))
            {
                cachedResults[pawn.thingIDNumber] = __state = new CachedValueTick<bool>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<bool> __state, ref bool __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public static void ClearCache(HediffComp_Invisibility __instance)
        {
            if (__instance.Pawn != null)
            {
                cachedResults.Remove(__instance.Pawn.thingIDNumber);
            }
        }
        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
