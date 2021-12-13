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
        }
        public static void ProcessAllowToolToggle(List<Gizmo> gizmos)
        {
            foreach (var gizmo in gizmos)
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
