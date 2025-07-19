using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_ResourceReadoutOnGUI : Optimization_UITweaks
    {
        public override string Label => "PO.HideResourceReadout".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(ResourceReadout), "ResourceReadoutOnGUI", GetMethod(nameof(Prefix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(ResourceReadout __instance)
        {
            if (Optimization_UIToggle.UIToggleOn)
            {
                float width = Prefs.ResourceReadoutCategorized ? 124f : 110f;
                Rect rect2 = new(0f, 0f, width, Mathf.Max(__instance.lastDrawnHeight + 50, 200));
                if (!Mouse.IsOver(rect2))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
