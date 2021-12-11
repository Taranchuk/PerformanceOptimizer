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

    public class Optimization_DoorRotationAt : Optimization_RefreshRate
    {
        public override int RefreshRateByDefault => 30;
        public override string Name => "PO.DoorRotationAt".Translate();
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Building_Door), "DoorRotationAt", GetMethod(nameof(DoorRotationAtPrefix)), GetMethod(nameof(DoorRotationAtPostfix)));
            Patch(typeof(Building_Door), "Draw", GetMethod(nameof(DrawPrefix)), GetMethod(nameof(DrawPostfix)));
        }

        public static Building_Door curDoor;
        public static void DrawPrefix(Building_Door __instance)
        {
            curDoor = __instance;
        }

        public static void DrawPostfix()
        {
            curDoor = null;
        }

        public static Dictionary<Building_Door, CachedValueTick<Rot4>> cachedResults = new Dictionary<Building_Door, CachedValueTick<Rot4>>();

        [HarmonyPriority(Priority.First)]
        public static bool DoorRotationAtPrefix(out bool __state, ref Rot4 __result)
        {
            if (curDoor == null)
            {
                __state = false;
                return true;
            }
            if (!cachedResults.TryGetValue(curDoor, out var cache))
            {
                cachedResults[curDoor] = new CachedValueTick<Rot4>(Rot4.Invalid, refreshRateStatic);
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
        public static void DoorRotationAtPostfix(bool __state, Rot4 __result)
        {
            if (__state)
            {
                cachedResults[curDoor].SetValue(__result, refreshRateStatic);
            }
        }
    }
}
