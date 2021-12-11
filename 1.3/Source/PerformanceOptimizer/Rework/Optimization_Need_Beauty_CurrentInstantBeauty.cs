using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;

namespace PerformanceOptimizer
{
    public class Optimization_Need_Beauty_CurrentInstantBeauty : Optimization_RefreshRate
    {
        public static Dictionary<Need_Beauty, CachedValueTick<float>> cachedResults = new Dictionary<Need_Beauty, CachedValueTick<float>>();
        public override int RefreshRateByDefault => throw new NotImplementedException();
        public override OptimizationType OptimizationType => throw new NotImplementedException();

        public override string Name => throw new NotImplementedException();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Need_Beauty), "CurrentInstantBeauty", GetMethod(nameof(Optimization_Need_Beauty_CurrentInstantBeauty.Prefix)), GetMethod(nameof(Optimization_Need_Beauty_CurrentInstantBeauty.Postfix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Need_Beauty __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance, out var cache))
            {
                cachedResults[__instance] = new CachedValueTick<float>(0, refreshRateStatic);
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
        public static void Postfix(Need_Beauty __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance].SetValue(__result, refreshRateStatic);
            }
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
