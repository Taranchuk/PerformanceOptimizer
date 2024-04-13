using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_HediffDef_PossibleToDevelopImmunityNaturally : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Label => "PO.CacheHediffDef_PossibleToDevelopImmunityNaturally".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(HediffDef), "PossibleToDevelopImmunityNaturally", GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        public static Dictionary<ushort, bool> cachedResults = new();

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(HediffDef __instance, out bool __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(__instance.shortHash, out bool cache))
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache;
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(HediffDef __instance, bool __state, bool __result)
        {
            if (__state)
            {
                cachedResults[__instance.shortHash] = __result;
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
