using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public class Optimization_ForbidUtility_IsForbidden : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<int, Dictionary<Thing, CachedValueTick<bool>>> cachedResults = new();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.IsForbidden".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(JobDriver), "CheckCurrentToilEndOrFail", GetMethod(nameof(CheckCurrentToilEndOrFailPrefix)), GetMethod(nameof(CheckCurrentToilEndOrFailPostfix)));
            System.Reflection.MethodInfo forbiddedMethod = AccessTools.Method(typeof(ForbidUtility), "IsForbidden", new Type[] { typeof(Thing), typeof(Pawn) });
            Patch(forbiddedMethod, GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static bool checkCurrentToilEndOrFailIsRunning;
        public static void CheckCurrentToilEndOrFailPrefix()
        {
            checkCurrentToilEndOrFailIsRunning = true;
        }

        public static void CheckCurrentToilEndOrFailPostfix()
        {
            checkCurrentToilEndOrFailIsRunning = false;
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Thing t, Pawn pawn, out CachedValueTick<bool> __state, ref bool __result)
        {
            if (checkCurrentToilEndOrFailIsRunning)
            {
                if (!cachedResults.TryGetValue(pawn.thingIDNumber, out Dictionary<Thing, CachedValueTick<bool>> cachedResult))
                {
                    cachedResults[pawn.thingIDNumber] = cachedResult = new Dictionary<Thing, CachedValueTick<bool>>();
                }
                if (!cachedResult.TryGetValue(t, out __state))
                {
                    cachedResult[t] = __state = new CachedValueTick<bool>();
                }
                return __state.SetOrRefresh(ref __result);
            }
            else
            {
                __state = null;
                return true;
            }
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<bool> __state, ref bool __result)
        {
            __state?.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
