using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public class Optimization_JobDriver_CheckCurrentToilEndOrFail : Optimization_RefreshRate
    {
        public static Dictionary<Pawn, int> cachedResults = new Dictionary<Pawn, int>();
        public override int RefreshRateByDefault => 10;
        public override OptimizationType OptimizationType => OptimizationType.Throttle;
        public override string Name => "PO.CheckCurrentToilEndOrFail".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(JobDriver), "CheckCurrentToilEndOrFail", GetMethod(nameof(Prefix)));
            Patch(typeof(Pawn_PathFollower), "StartPath", transpiler: GetMethod(nameof(Pawn_PathFollower_StartPathTranspiler)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(JobDriver __instance)
        {
            if (__instance is JobDriver_LayDown || __instance is JobDriver_Goto || __instance is JobDriver_DoBill || __instance is JobDriver_Research || __instance is JobDriver_Refuel 
                || __instance is JobDriver_GoForWalk || __instance is JobDriver_ConstructFinishFrame)
            {
                if (!cachedResults.TryGetValue(__instance.pawn, out var cache)
                    || PerformanceOptimizerMod.tickManager.ticksGameInt > (cache + refreshRateStatic))
                {
                    cachedResults[__instance.pawn] = PerformanceOptimizerMod.tickManager.ticksGameInt;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<CodeInstruction> Pawn_PathFollower_StartPathTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (i + 2 < codes.Count && codes[i + 2].OperandIs(" pathing to destroyed thing "))
                {
                    i += 7; // we skip Log.Error(string.Concat(pawn, " pathing to destroyed thing ", dest.Thing));
                }
                yield return codes[i];
            }
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
