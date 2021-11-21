using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace PerformanceOptimizer
{

    [HarmonyPatch(typeof(SteamManager))]
    [HarmonyPatch("Update")]
    public static class Patch_SteamManager_Update
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            if (PerformanceOptimizerSettings.disableSteamManagerCallbacksChecks && Current.programStateInt == ProgramState.Playing)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(WindManager), "WindManagerTick")]
    public static class Patch_WindManager_WindManagerTick
    {
        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            var codes = instructions.ToList();
            var plantSwayHead = AccessTools.Field(typeof(WindManager), nameof(WindManager.plantSwayHead));
            for (var i = 0; i < codes.Count; i++)
            {
                if (i > 2 && codes[i - 1].opcode == OpCodes.Stfld && codes[i - 1].OperandIs(plantSwayHead) && codes[i - 2].OperandIs(0.0f) && codes[i - 2].opcode == OpCodes.Ldc_R4)
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_WindManager_WindManagerTick), nameof(ShouldDo))).MoveLabelsFrom(codes[i]);
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
            if (PerformanceOptimizerSettings.disablePlantSwayShaderUpdateIfSwayDisabled)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SoundRoot))]
    [HarmonyPatch("Update")]
    public static class SoundRoot_Update
    {
        public static bool Prefix()
        {
            if (PerformanceOptimizerSettings.disableSoundsCompletely)
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(SoundStarter))]
    [HarmonyPatch("PlayOneShotOnCamera")]
    public static class SoundStarter_PlayOneShotOnCamera
    {
        public static bool Prefix()
        {
            if (PerformanceOptimizerSettings.disableSoundsCompletely)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SoundStarter))]
    [HarmonyPatch("PlayOneShot")]
    public static class SoundStarter_PlayOneShot
    {
        public static bool Prefix()
        {
            if (PerformanceOptimizerSettings.disableSoundsCompletely)
            {
                return false;
            }
            return true;
        }
    }
}

