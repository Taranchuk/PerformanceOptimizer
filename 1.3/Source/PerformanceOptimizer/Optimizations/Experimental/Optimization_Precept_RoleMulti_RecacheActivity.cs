using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Precept_RoleMulti_RecacheActivity : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<Precept_RoleMulti, int> cachedResults = new Dictionary<Precept_RoleMulti, int>();
        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.Throttle;
        public override string Label => "PO.Precept_RoleMulti_RecacheActivity".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Precept_RoleMulti), "RecacheActivity", GetMethod(nameof(Prefix)));
            Patch(typeof(Ideo), nameof(Ideo.RecacheColonistBelieverCount), GetMethod(nameof(RecacheColonistBelieverCountPrefix)), GetMethod(nameof(RecacheColonistBelieverCountPostfix)));
        }

        public static bool skipThrottling;

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Precept_RoleMulti __instance)
        {
            if (!skipThrottling)
            {
                if (!cachedResults.TryGetValue(__instance, out var cache)
                    || PerformanceOptimizerMod.tickManager.ticksGameInt > (cache + refreshRateStatic))
                {
                    cachedResults[__instance] = PerformanceOptimizerMod.tickManager.ticksGameInt;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static void RecacheColonistBelieverCountPrefix()
        {
            skipThrottling = true;
        }

        public static void RecacheColonistBelieverCountPostfix()
        {
            skipThrottling = false;
        }
        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}