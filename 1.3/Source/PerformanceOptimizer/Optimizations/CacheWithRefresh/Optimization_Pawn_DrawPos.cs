using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Pawn_DrawPos : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.PawnDrawPos".Translate();
        public override int RefreshRateByDefault => 10;
        public override int MaxSliderValue => 60;
        public override void DoPatches()
        {
            base.DoPatches();
            bool cache = false;
            List<MethodInfo> methods = new List<MethodInfo>();
            methods.Add(AccessTools.Method("DubsMintMinimap.MainTabWindow_MiniMap:DrawAllPawns"));
            methods.Add(AccessTools.Method(typeof(Designation), "Draw"));
            foreach (var method in methods)
            {
                if (method != null)
                {
                    cache = true;
                    Patch(method, GetMethod(nameof(Optimization_Pawn_DrawPos.EnableCache)), GetMethod(nameof(Optimization_Pawn_DrawPos.DisableCache)));
                }
            }

            if (cache)
            {
                Patch(typeof(Pawn), "get_DrawPos", GetMethod(nameof(Optimization_Pawn_DrawPos.Prefix)), GetMethod(nameof(Optimization_Pawn_DrawPos.Postfix)));
            }
        }

        public static Dictionary<Pawn, CachedValueTick<Vector3>> cachedResults = new Dictionary<Pawn, CachedValueTick<Vector3>>();

        public static bool shouldReturnCachedValue;

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn __instance, out bool __state, ref Vector3 __result)
        {
            if (shouldReturnCachedValue)
            {
                if (!cachedResults.TryGetValue(__instance, out var cache))
                {
                    cachedResults[__instance] = new CachedValueTick<Vector3>(default, refreshRateStatic);
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
            __state = false;
            return true;
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn __instance, bool __state, ref Vector3 __result)
        {
            if (__state)
            {
                cachedResults[__instance].SetValue(__result, refreshRateStatic);
            }
        }

        public static void EnableCache()
        {
            shouldReturnCachedValue = true;
        }

        public static void DisableCache()
        {
            shouldReturnCachedValue = false;

        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
