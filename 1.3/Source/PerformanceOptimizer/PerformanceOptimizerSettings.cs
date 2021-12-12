using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    [DefOf]
    public static class PODefOf
    {
        public static KeyBindingDef PerformanceOptimizerKey;
    }
    public class PerformanceOptimizerSettings : ModSettings
    {
        public static List<Optimization> optimizations;
        public static bool hideResourceReadout = true;
        public static bool hideBottomButtonBar = true;
        public static bool hideBottomRightOverlayButtons = true;
        public static bool minimizeAlertsReadout = true;
        public static bool hideSpeedButtons = true;
        public static bool disableSpeedButtons = false;
        public static bool overviewLetterSent;
        public static bool UIToggleOn = true;
        public static bool OneKeyMode = true;
        public static bool UITogglePressed
        {
            get
            {
                if (OneKeyMode)
                {
                    return Input.GetKeyDown(PerformanceOptimizerMod.keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B));
                }
                return Input.GetKey(PerformanceOptimizerMod.keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.A)) 
                    && Input.GetKeyDown(PerformanceOptimizerMod.keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref UIToggleOn, "UIToggleOn", true);
            Scribe_Values.Look(ref OneKeyMode, "OneKeyMode", false);
            Scribe_Values.Look(ref overviewLetterSent, "overviewLetterSent");
            Scribe_Values.Look(ref hideResourceReadout, "hideResourceReadout", true);
            Scribe_Values.Look(ref hideBottomButtonBar, "hideBottomButtonBar", true);
            Scribe_Values.Look(ref hideBottomRightOverlayButtons, "hideBottomRightOverlayButtons", true);
            Scribe_Values.Look(ref minimizeAlertsReadout, "minimizeAlertsReadout", true);
            Scribe_Values.Look(ref hideSpeedButtons, "hideSpeedButtons", true);
            Scribe_Values.Look(ref disableSpeedButtons, "disableSpeedButtons", false);

            Log_Error_Patch.suppressErrorMessages = true;
            Scribe_Collections.Look(ref optimizations, "optimizations", LookMode.Deep);
            Log_Error_Patch.suppressErrorMessages = false;
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Find.WindowStack.currentlyDrawnWindow.absorbInputAroundWindow = false;
            var uiTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.UITweak).ToList();
            var performanceTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.Optimization);
            var miscTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.Misc);
            var tweaks = performanceTweaks.Concat(miscTweaks).ToList();

            var throttles = optimizations.Where(x => x.OptimizationType == OptimizationType.Throttle 
                || x.OptimizationType == OptimizationType.CacheWithRefreshRate).Cast<Optimization_RefreshRate>().ToList();

            var sectionHeightSize = (tweaks.Count * 24) + 8 + 30;
            var cacheSettingsHeight = (throttles.Count * 24) + 8 + 30 + 24;
            var totalHeight = sectionHeightSize + cacheSettingsHeight + 50;

            Rect rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 20);
            Rect rect2 = new Rect(0f, 0f, inRect.width - 30f, totalHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);

            var sectionWidth = ((inRect.width - 30) / 2f) - 8;
            Rect uiSettingsRect = new Rect(inRect.x, inRect.y - 30, sectionWidth, sectionHeightSize + 20);
            Listing_Standard topLeftSection = new Listing_Standard();
            topLeftSection.Begin(uiSettingsRect);
            var uiSection = topLeftSection.BeginSection(sectionHeightSize, 10, 10);
            if (uiSection.ButtonTextLabeled("PO.UISettings".Translate(), "Reset".Translate()))
            {
                hideResourceReadout = true;
                hideBottomButtonBar = true;
                hideBottomRightOverlayButtons = true;
                minimizeAlertsReadout = true;
                hideSpeedButtons = true;
                disableSpeedButtons = false;
            }

            uiSection.GapLine(8);
            uiSection.CheckboxLabeled("PO.HideResourceReadout".Translate(), ref hideResourceReadout);
            uiSection.CheckboxLabeled("PO.HideBottomButtonBar".Translate(), ref hideBottomButtonBar);
            uiSection.CheckboxLabeled("PO.HideBottomRightOverlayButtons".Translate(), ref hideBottomRightOverlayButtons);
            uiSection.CheckboxLabeled("PO.MinimizeAlertsReadout".Translate(), ref minimizeAlertsReadout);
            uiSection.CheckboxLabeled("PO.HideSpeedButtons".Translate(), ref hideSpeedButtons);
            uiSection.CheckboxLabeled("PO.DisableSpeedButtons".Translate(), ref disableSpeedButtons);

            var keyPrefsData = KeyPrefs.KeyPrefsData;
            var keyHidingText = "PO.UIToggle".Translate();
            var size = Text.CalcSize(keyHidingText);
            var keybindingRect = new Rect(uiSection.curX + size.x + 10, uiSection.curY, 50, 24f);
            var keybindingRectPlus = new Rect(keybindingRect.xMax, uiSection.curY - 3, 24, 24);
            if (!OneKeyMode)
            {
                if (Widgets.ButtonText(keybindingRect, keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.A).ToStringReadable()))
                {
                    Find.WindowStack.Add(new Dialog_DefineBinding(keyPrefsData, PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.A));
                    Event.current.Use();
                }
                Text.Font = GameFont.Medium;
                Widgets.Label(keybindingRectPlus, " + ");
                Text.Font = GameFont.Small;
            }
            var keybinding2Rect = new Rect(keybindingRectPlus.xMax, uiSection.curY, 50, 24f);
            if (Widgets.ButtonText(keybinding2Rect, keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B).ToStringReadable()))
            {
                Find.WindowStack.Add(new Dialog_DefineBinding(keyPrefsData, PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B));
                Event.current.Use();
            }

            var checkboxOneKey = new Rect(keybinding2Rect.xMax + 10, keybinding2Rect.y, 120, 24);
            Widgets.CheckboxLabeled(checkboxOneKey, "PO.OneKeyMode".Translate(), ref OneKeyMode);

            uiSection.Label(keyHidingText);
            topLeftSection.EndSection(uiSection);
            topLeftSection.End();
            
            Listing_Standard miscSettingsSection = new Listing_Standard();
            Rect miscSettingsRect = new Rect(uiSettingsRect.xMax + 15, uiSettingsRect.y, sectionWidth, uiSettingsRect.height);
            miscSettingsSection.Begin(miscSettingsRect);
            var miscSettings = miscSettingsSection.BeginSection(sectionHeightSize, 10, 10);

            if (miscSettings.ButtonTextLabeled("PO.PerformanceSettings".Translate(), "Reset".Translate()))
            {
                foreach (var tweak in tweaks)
                {
                    tweak.Reset();
                }
            }

            miscSettings.GapLine(8);
            foreach (var tweak in tweaks)
            {
                miscSettings.CheckboxLabeled(tweak.Name, ref tweak.enabled);
            }
            miscSettingsSection.EndSection(miscSettings);
            miscSettingsSection.End();

            Listing_Standard cacheSection = new Listing_Standard();
            Rect topRect = new Rect(inRect.x, uiSettingsRect.yMax + 15, inRect.width - 30, cacheSettingsHeight);
            cacheSection.Begin(topRect);
            var cacheSettings = cacheSection.BeginSection(cacheSettingsHeight - 20, 10, 10);
            if (cacheSettings.ButtonTextLabeled("PO.CacheSettings".Translate(), "Reset".Translate()))
            {
                foreach (var optimization in throttles)
                {
                    optimization.Reset();
                }
            }
            cacheSettings.GapLine(8);
            foreach (var optimization in throttles)
            {
                var sliderName = optimization.OptimizationType == OptimizationType.CacheWithRefreshRate ? "PO.RefreshRate" : "PO.ThrottleRate";
                cacheSettings.CheckboxLabeledWithSlider(optimization.Name, sliderName, ref optimization.enabled, ref optimization.refreshRate);
            }

            cacheSection.EndSection(cacheSettings);
            cacheSection.End();

            Widgets.EndScrollView();
        }

        private static Vector2 scrollPosition = Vector2.zero;
    }
}
