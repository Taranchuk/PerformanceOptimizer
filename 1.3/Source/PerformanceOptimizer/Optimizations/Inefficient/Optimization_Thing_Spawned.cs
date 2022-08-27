using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Thing_Spawned : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Name => "PO.OptimizeThingSpawned".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Thing), "get_Spawned", transpiler: GetMethod(nameof(Transpiler)));
        }

        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            var codes = instructions.ToList();
            CodeInstruction firstBGe_S = null;
            bool found = false;
            for (var i = 0; i < codes.Count; i++)
            {
                if (!found && codes[i].opcode == OpCodes.Bge_S)
                {
                    firstBGe_S = codes[i];
                    found = true;
                }
                if (i > 0 && codes[i - 1].opcode == OpCodes.Ret && codes[i].opcode == OpCodes.Ldarg_0)
                {
                    i += 10;
                    codes[i].labels.Add(((Label)firstBGe_S.operand));
                    yield return codes[i];
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }
}

