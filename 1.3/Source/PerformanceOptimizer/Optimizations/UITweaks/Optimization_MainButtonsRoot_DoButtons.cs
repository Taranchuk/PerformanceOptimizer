using HarmonyLib;
using RimWorld;
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
        }
        public override string Label => "PO.HideBottomButtonBar".Translate();

        [HarmonyPriority(Priority.First)]
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
