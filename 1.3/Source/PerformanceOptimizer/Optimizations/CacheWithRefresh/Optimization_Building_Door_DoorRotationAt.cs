using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{

    public class Optimization_Building_Door_DoorRotationAt : Optimization_RefreshRate
    {
        public static int refreshRateStatic;
        public override int RefreshRateByDefault => 90;
        public override string Label => "PO.DoorRotationAt".Translate();
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
        public static Dictionary<int, CachedValueTick<Rot4>> cachedResults = new Dictionary<int, CachedValueTick<Rot4>>();
        [HarmonyPriority(int.MaxValue)]
        public static bool DoorRotationAtPrefix(out CachedValueTick<Rot4> __state, ref Rot4 __result)
        {
            if (curDoor == null)
            {
                __state = null;
                return true;
            }
            if (!cachedResults.TryGetValue(curDoor.thingIDNumber, out __state))
            {
                cachedResults[curDoor.thingIDNumber] = __state = new CachedValueTick<Rot4>();
                return true;
            }
            return __state.SetOrRefresh(ref __result);

        }

        [HarmonyPriority(int.MinValue)]
        public static void DoorRotationAtPostfix(CachedValueTick<Rot4> __state, ref Rot4 __result)
        {
            if (__state != null)
            {
                __state.ProcessResult(ref __result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
