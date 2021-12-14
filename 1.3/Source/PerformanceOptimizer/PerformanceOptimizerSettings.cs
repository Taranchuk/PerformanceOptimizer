using RimWorld;
using System;
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
        public static bool overviewLetterSent;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref overviewLetterSent, "overviewLetterSent");
            Log_Error_Patch.suppressErrorMessages = true;
            Scribe_Collections.Look(ref optimizations, "optimizations", LookMode.Deep);
            Log_Error_Patch.suppressErrorMessages = false;
        }

        public static List<Optimization> uiMiscTweaks;
        public static List<Optimization> optimizationTweaks;
        public static List<Optimization> throttles;
        public static void Initialize()
        {
            optimizations ??= new List<Optimization>();
            optimizations.RemoveAll(x => x is null);

            var optimizationTypes = GenTypes.AllSubclassesNonAbstract(typeof(Optimization));
            foreach (var optimizationType in optimizationTypes)
            {
                if (!optimizations.Any(x => x.GetType() == optimizationType))
                {
                    var optimization = Activator.CreateInstance(optimizationType) as Optimization;
                    optimization.Initialize();
                    optimizations.Add(optimization);
                }
            }

            uiMiscTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.UITweak).Concat(optimizations.Where(x => x.OptimizationType == OptimizationType.Misc)).ToList();
            optimizationTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.Optimization).ToList();
            throttles = optimizations.Where(x => x.OptimizationType == OptimizationType.Throttle || x.OptimizationType == OptimizationType.CacheWithRefreshRate).ToList();

            foreach (var optimization in optimizations)
            {
                optimization.Apply();
            }
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var uiSectionHeight = uiMiscTweaks.Sum(x => x.DrawHeight);
            var optimizationTweaksHeight = optimizationTweaks.Sum(x => x.DrawHeight);
            var throttlesHeight = throttles.Sum(y => y.DrawHeight);

            var sectionHeightSize = (uiSectionHeight > optimizationTweaksHeight ? uiSectionHeight : optimizationTweaksHeight) + 8 + 30;
            var cacheSettingsHeight = throttlesHeight + 8 + 30 + 24;
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
                foreach (var tweak in uiMiscTweaks)
                {
                    tweak.Reset();
                }
            }
            uiSection.GapLine(8);
            foreach (var tweak in uiMiscTweaks.OrderBy(x => x))
            {
                tweak.DrawSettings(uiSection);
            }
            topLeftSection.EndSection(uiSection);
            topLeftSection.End();

            Listing_Standard miscSettingsSection = new Listing_Standard();
            Rect miscSettingsRect = new Rect(uiSettingsRect.xMax + 15, uiSettingsRect.y, sectionWidth, uiSettingsRect.height);
            miscSettingsSection.Begin(miscSettingsRect);
            var miscSettings = miscSettingsSection.BeginSection(sectionHeightSize, 10, 10);

            if (miscSettings.ButtonTextLabeled("PO.PerformanceSettings".Translate(), "Reset".Translate()))
            {
                foreach (var tweak in optimizationTweaks)
                {
                    tweak.Reset();
                }
            }

            miscSettings.GapLine(8);
            foreach (var tweak in optimizationTweaks.OrderBy(x => x))
            {
                tweak.DrawSettings(miscSettings);
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
            foreach (var optimization in throttles.OrderBy(x => x))
            {
                optimization.DrawSettings(cacheSettings);
            }
            cacheSection.EndSection(cacheSettings);
            cacheSection.End();

            Widgets.EndScrollView();
        }

        private static Vector2 scrollPosition = Vector2.zero;
    }
}
