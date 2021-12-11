using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Verse;
using static Verse.TKeySystem;

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
        public static bool cacheFindAllowedDesignator = true;

        public static bool fasterGetCompReplacement = true;
        public static bool disableSteamManagerCallbacksChecks = true;
        public static bool disablePlantSwayShaderUpdateIfSwayDisabled = true;
        public static bool disableSoundsCompletely = false;

        public static bool disableReportProbablyMissingAttributes = true;
        public static bool disableLogHarmonyPatchIssueErrors = true;
        public static bool cacheCustomDataLoadMethodOf = true;
        public static bool cacheHasGenericDefinition = true;
        public static bool fixCheckForDuplicateNodes = true;

        public static bool overviewLetterSent;
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
        public static bool UIToggleOn = true;
        public static bool OneKeyMode = true;
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
            Scribe_Values.Look(ref cacheFindAllowedDesignator, "cacheFindAllowedDesignator", true);

            Scribe_Values.Look(ref fasterGetCompReplacement, "fasterGetCompReplacement", true);
            Scribe_Values.Look(ref disableSteamManagerCallbacksChecks, "disableSteamManagerCallbacksChecks", true);
            Scribe_Values.Look(ref disablePlantSwayShaderUpdateIfSwayDisabled, "disablePlantSwayShaderUpdateIfSwayDisabled", true);
            Scribe_Values.Look(ref disableSoundsCompletely, "disableSoundsCompletely", false);
            Scribe_Values.Look(ref fixCheckForDuplicateNodes, "fixCheckForDuplicateNodes", true);

            Scribe_Collections.Look(ref optimizations, "optimizations", LookMode.Deep);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            Find.WindowStack.currentlyDrawnWindow.absorbInputAroundWindow = false;
            var uiTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.UITweak).ToList();
            var performanceTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.Cache).ToList();
            var miscTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.Misc).ToList();
            var throttles = optimizations.Where(x => x.OptimizationType == OptimizationType.Throttle 
                || x.OptimizationType == OptimizationType.CacheWithRefreshRate).Cast<Optimization_RefreshRate>().ToList();

            var sectionHeightSize = (9 * 24) + 8 + 30;
            var cacheSettingsHeight = (19 * 24) + 8 + 30 + 24;
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
                fasterGetCompReplacement = true;
                cacheFindAllowedDesignator = true;
                disableSteamManagerCallbacksChecks = true;
                disablePlantSwayShaderUpdateIfSwayDisabled = true;
                disableSoundsCompletely = false;
                fixCheckForDuplicateNodes = true;
                foreach (var performanceTweak in performanceTweaks)
                {
                    performanceTweak.Reset();
                }
            }

            miscSettings.GapLine(8);
            miscSettings.CheckboxLabeled("PO.FasterGetCompReplacement".Translate(), ref fasterGetCompReplacement);

            foreach (var tweak in performanceTweaks)
            {
                miscSettings.CheckboxLabeled(tweak.Name, ref tweak.enabled);
            }

            foreach (var tweak in miscTweaks)
            {
                miscSettings.CheckboxLabeled(tweak.Name, ref tweak.enabled);
            }

            miscSettings.CheckboxLabeled("PO.DisableSteamManagerCallbacksChecks".Translate(), ref disableSteamManagerCallbacksChecks);
            miscSettings.CheckboxLabeled("PO.DisablePlantSwayShaderUpdateIfSwayDisabled".Translate(), ref disablePlantSwayShaderUpdateIfSwayDisabled);
            miscSettings.CheckboxLabeled("PO.DisableSoundsCompletely".Translate(), ref disableSoundsCompletely);
            miscSettings.CheckboxLabeled("PO.FixCheckForDuplicateNodes".Translate(), ref fixCheckForDuplicateNodes);

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
                if (optimization.OptimizationType == OptimizationType.CacheWithRefreshRate)
                {
                    cacheSettings.CheckboxLabeledWithSlider(optimization.Name, "PO.RefreshRate", ref optimization.enabled, ref optimization.refreshRate);
                }
            }

            cacheSection.EndSection(cacheSettings);
            cacheSection.End();

            Widgets.EndScrollView();
        }

        private static Vector2 scrollPosition = Vector2.zero;
    }
}
