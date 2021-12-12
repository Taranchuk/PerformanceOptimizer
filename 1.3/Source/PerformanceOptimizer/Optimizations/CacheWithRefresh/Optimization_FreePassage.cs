using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PerformanceOptimizer
{

    public class Optimization_FreePassage : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override int RefreshRateByDefault => 10;
        public override string Name => "PO.FreePassage".Translate();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Building_Door), "get_FreePassage", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<Building_Door, CachedValueTick<bool>> cachedResults = new Dictionary<Building_Door, CachedValueTick<bool>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Building_Door __instance, out bool __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(__instance, out var cache))
            {
                cachedResults[__instance] = new CachedValueTick<bool>(false, refreshRateStatic);
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
        public static void Postfix(Building_Door __instance, bool __state, bool __result)
        {
            if (__state)
            {
                cachedResults[__instance].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
