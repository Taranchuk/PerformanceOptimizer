using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_WindManager_WindManagerTick : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Label => "PO.DisablePlantSwayShaderUpdateIfSwayDisabled".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(WindManager), "WindManagerTick", transpiler: GetMethod(nameof(Transpiler)));
        }
        [HarmonyPriority(int.MaxValue)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            var codes = instructions.ToList();
            var plantSwayHead = AccessTools.Field(typeof(WindManager), nameof(WindManager.plantSwayHead));
            for (var i = 0; i < codes.Count; i++)
            {
                if (i > 2 && codes[i - 1].opcode == OpCodes.Stfld && codes[i - 1].OperandIs(plantSwayHead) && codes[i - 2].OperandIs(0.0f) && codes[i - 2].opcode == OpCodes.Ldc_R4)
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Optimization_WindManager_WindManagerTick), nameof(ShouldDo))).MoveLabelsFrom(codes[i]);
                    var label = iLGenerator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    yield return new CodeInstruction(OpCodes.Ret);
                    codes[i].labels.Add(label);
                }
                yield return codes[i];
            }
        }
        public static bool ShouldDo()
        {
            if (Prefs.PlantWindSway)
            {
                return true;
            }
            return false;
        }
    }
}

