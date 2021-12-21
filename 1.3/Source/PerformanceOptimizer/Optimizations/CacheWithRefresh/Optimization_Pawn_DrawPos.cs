using HarmonyLib;
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
            List<MethodInfo> methods = new()
            {
                AccessTools.Method("DubsMintMinimap.MainTabWindow_MiniMap:DrawAllPawns"),
                AccessTools.Method(typeof(Designation), "Draw")
            };
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

        public static Dictionary<int, CachedValueTick<Vector3>> cachedResults = new Dictionary<int, CachedValueTick<Vector3>>();

        public static bool shouldReturnCachedValue;

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn __instance, out CachedValueTick<Vector3> __state, ref Vector3 __result)
        {
            if (shouldReturnCachedValue)
            {
                if (!cachedResults.TryGetValue(__instance.thingIDNumber, out __state))
                {
                    cachedResults[__instance.thingIDNumber] = __state = new CachedValueTick<Vector3>();
                    return true;
                }
                return __state.SetOrRefresh(ref __result);
            }
            __state = null;
            return true;
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedValueTick<Vector3> __state, ref Vector3 __result)
        {
            if (__state != null)
            {
                __state.ProcessResult(ref __result, refreshRateStatic);
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
