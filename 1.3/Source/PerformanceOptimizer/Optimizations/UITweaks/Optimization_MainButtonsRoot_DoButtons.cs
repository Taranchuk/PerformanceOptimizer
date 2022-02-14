using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_MainButtonsRoot_DoButtons : Optimization_UITweaks
    {
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(MainButtonsRoot), "DoButtons", GetMethod(nameof(Prefix)));
            if (ModLister.AllInstalledMods.ToList().Exists(x => x.Active && x.PackageIdPlayerFacing == "GonDragon.UINotIncluded"))
            {
                Log.Message("[Performance Optimizer] UI Not Included detected, performing patches on it.");
                var type = AccessTools.TypeByName("UINotIncluded.UIManager");
                if (type is null)
                {
                    Log.Error("Performance Optimizer didn't found UINotIncluded.UIManager to patch. The patches were not applied.");
                }
                else
                {
                    var method1 = AccessTools.Method(type, "BarsOnGUI");
                    if (method1 != null)
                    {
                        Patch(method1, GetMethod(nameof(Prefix)));
                    }
                    try
                    {
                        var method2 = AccessTools.Method(type, "VUIE_BarsOnGUI");
                        if (method2 != null)
                        {
                            Patch(method2, GetMethod(nameof(Prefix)));
                        }
                    }
                    catch (System.TypeLoadException)
                    {

                    }
                }
            }
        }
        public override string Label => "PO.HideBottomButtonBar".Translate();

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix()
        {
            if (Optimization_UIToggle.UIToggleOn)
            {
                if (Event.current.mousePosition.y < UI.screenHeight - 35)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
