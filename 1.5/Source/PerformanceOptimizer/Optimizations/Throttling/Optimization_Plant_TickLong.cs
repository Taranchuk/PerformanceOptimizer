﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Plant_TickLong : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public static Dictionary<int, int> cachedResults = new();
        public override int RefreshRateByDefault => 6000;
        public override int MaxSliderValue => 10000;
        public override OptimizationType OptimizationType => OptimizationType.Throttle;
        public override string Label => "PO.Plant_TickLong".Translate();

        public static HashSet<ThingDef> throttledPlants = new();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Plant), "TickLong", GetMethod(nameof(Prefix)), transpiler: GetMethod(nameof(Transpiler)));
            static bool predicator(ThingDef x)
            {
                if (x.plant != null)
                {
                    if (x.tickerType != TickerType.Long)
                    {
                        return false;
                    }

                    if (x.plant.harvestedThingDef is null || x.plant.harvestTag == "Wood")
                    {
                        return true;
                    }
                }
                return false;
            }
            throttledPlants.AddRange(DefDatabase<ThingDef>.AllDefs.Where(x => predicator(x)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Plant __instance)
        {
            if (throttledPlants.Contains(__instance.def))
            {
                if (!cachedResults.TryGetValue(__instance.thingIDNumber, out int cache) || PerformanceOptimizerMod.tickManager.ticksGameInt >= (cache + refreshRateStatic))
                {
                    cachedResults[__instance.thingIDNumber] = PerformanceOptimizerMod.tickManager.ticksGameInt;
                    return true;
                }
                else
                {
                    if (__instance.comps != null)
                    {
                        int i = 0;
                        for (int count = __instance.comps.Count; i < count; i++)
                        {
                            __instance.comps[i].CompTickLong();
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4 && codes[i].OperandIs(2000))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Optimization_Plant_TickLong), nameof(ThrottleRateInt)));
                }
                else if (codes[i].opcode == OpCodes.Ldc_R4 && codes[i].OperandIs(2000f))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(codes[i]);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Optimization_Plant_TickLong), nameof(ThrottleRateFloat)));
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static int ThrottleRateInt(Plant plant)
        {
            return ShouldBeThrottled(plant) ? refreshRateStatic : 2000;
        }

        public static float ThrottleRateFloat(Plant plant)
        {
            return ShouldBeThrottled(plant) ? refreshRateStatic : 2000f;
        }

        public static bool ShouldBeThrottled(Plant plant)
        {
            return throttledPlants.Contains(plant.def);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
