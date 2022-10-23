using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    [StaticConstructorOnStartup]
    public static class ModCompatUtility
    {
        public static bool AllowToolActive;
        static ModCompatUtility()
        {
            AllowToolActive = ModLister.HasActiveModWithName("Allow Tool");
            if (AllowToolActive)
            {
                PerformanceOptimizerMod.harmony.Patch(
                    AccessTools.Method("AllowTool.Context.DesignatorContextMenuController:RegisterReverseDesignatorPair",
                    new Type[] { typeof(Designator), typeof(Command) }),
                    finalizer: new HarmonyMethod(AccessTools.Method(typeof(ModCompatUtility), nameof(Finalizer))));
            }
        }

        public static Exception Finalizer(Exception __exception)
        {
            return null;
        }
        public static void ProcessAllowToolToggle(List<Gizmo> gizmos)
        {
            foreach (Gizmo gizmo in gizmos)
            {
                if (gizmo is Command_Toggle toggle && toggle.defaultLabel == "CommandAllow".TranslateWithBackup("DesignatorUnforbid"))
                {
                    try
                    {
                        AllowTool.AllowThingToggleHandler.EnhanceStockAllowToggle(toggle);
                        break;
                    }
                    catch { };
                }
            }
        }
    }
}
