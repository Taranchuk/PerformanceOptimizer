using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_DoPlaySettings_DoPlaySettings : Optimization_UITweaks
    {
        [TweakValue("0", 0, 2000)] public static float xTest = 150;

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(GlobalControlsUtility), "DoPlaySettings", GetMethod(nameof(Prefix)));
            Patch(typeof(LetterStack), "LettersOnGUI", GetMethod(nameof(LettersOnGUIPrefix)));
        }

        public static float lettersBottomY;
        public static void LettersOnGUIPrefix(float baseY)
        {
            lettersBottomY = baseY;
        }
        public override string Label => "PO.HideBottomRightOverlayButtons".Translate();

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(WidgetRow rowVisibility, bool worldView)
        {
            if (Optimization_UIToggle.UIToggleOn && rowVisibility.FinalY > 0)
            {
                bool mouseOnRight = Event.current.mousePosition.x < (UI.screenWidth - xTest);
                if (mouseOnRight || (!mouseOnRight && lettersBottomY + 50 > Event.current.mousePosition.y))
                {
                    if (!worldView)
                    {
                        PlaySettings.CheckKeyBindingToggle(KeyBindingDefOf.ToggleBeautyDisplay, ref Find.PlaySettings.showBeauty);
                        PlaySettings.CheckKeyBindingToggle(KeyBindingDefOf.ToggleRoomStatsDisplay, ref Find.PlaySettings.showRoomStats);
                        bool toggleable = Prefs.ResourceReadoutCategorized;
                        bool flag = toggleable;
                        if (toggleable != flag)
                        {
                            Prefs.ResourceReadoutCategorized = toggleable;
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
