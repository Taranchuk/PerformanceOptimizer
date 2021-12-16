using HarmonyLib;
using Mono.Unix.Native;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace PerformanceOptimizer
{
    public class Optimization_IdeoBuildingPresenceDemand_BuildingPresent : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Label => "PO.IdeoBuildingPresenceDemand_BuildingPresent".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(IdeoBuildingPresenceDemand), "BuildingPresent", GetMethod(nameof(Prefix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(IdeoBuildingPresenceDemand __instance, Map map, ref bool __result)
        {
            __result = map.listerThings.ThingsOfDef(__instance.parent.ThingDef).Any(t => t.Faction == Faction.OfPlayer && t.StyleSourcePrecept == __instance.parent);
            return false;
        }
    }
}
