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
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                optimizations.RemoveAll(x => x is null);
            }
            Log_Error_Patch.suppressErrorMessages = false;
        }

        public static List<Optimization> uiTweaks;
        public static List<Optimization> perfTweaks;
        public static List<Optimization> throttles;
        public static void Initialize()
        {
            optimizations ??= new List<Optimization>();
            optimizations.RemoveAll(x => x is null);

            List<Type> optimizationTypes = GenTypes.AllSubclassesNonAbstract(typeof(Optimization));
            foreach (Type optimizationType in optimizationTypes)
            {
                if (!optimizations.Any(x => x.GetType() == optimizationType))
                {
                    Optimization optimization = Activator.CreateInstance(optimizationType) as Optimization;
                    optimization.Initialize();
                    optimizations.Add(optimization);
                }
            }

            uiTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.UITweak).ToList();
            perfTweaks = optimizations.Where(x => x.OptimizationType == OptimizationType.Optimization)
                .Concat(optimizations.Where(x => x.OptimizationType == OptimizationType.Misc)).ToList();
            throttles = optimizations.Where(x => x.OptimizationType is OptimizationType.Throttle or OptimizationType.CacheWithRefreshRate).ToList();

            foreach (Optimization optimization in optimizations)
            {
                optimization?.Apply();
            }
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            int uiSectionHeight = uiTweaks.Sum(x => x.DrawHeight);
            int optimizationTweaksHeight = perfTweaks.Sum(x => x.DrawHeight);
            int throttlesHeight = throttles.Sum(y => y.DrawHeight);

            int sectionHeightSize = (uiSectionHeight > optimizationTweaksHeight ? uiSectionHeight : optimizationTweaksHeight) + 8 + 30;
            int cacheSettingsHeight = throttlesHeight + 8 + 30 + 24;
            int totalHeight = sectionHeightSize + cacheSettingsHeight + 50;

            Rect rect = new(inRect.x, inRect.y, inRect.width, inRect.height - 20);
            Rect rect2 = new(0f, 0f, inRect.width - 30f, totalHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2, true);
            float sectionWidth = ((inRect.width - 30) / 2f) - 8;

            Rect uiSettingsRect = new(inRect.x, inRect.y - 30, sectionWidth, sectionHeightSize + 20);
            Listing_Standard topLeftSection = new();
            topLeftSection.Begin(uiSettingsRect);
            Listing_Standard uiSection = topLeftSection.BeginSection(sectionHeightSize, 10, 10);
            if (uiSection.ButtonTextLabeled("PO.UISettings".Translate(), "Reset".Translate()))
            {
                foreach (Optimization tweak in uiTweaks)
                {
                    tweak.Reset();
                }
            }
            uiSection.GapLine(8);
            foreach (Optimization tweak in uiTweaks.OrderBy(x => x))
            {
                tweak.DrawSettings(uiSection);
            }
            topLeftSection.EndSection(uiSection);
            topLeftSection.End();

            Listing_Standard miscSettingsSection = new();
            Rect miscSettingsRect = new(uiSettingsRect.xMax + 15, uiSettingsRect.y, sectionWidth, uiSettingsRect.height);
            miscSettingsSection.Begin(miscSettingsRect);
            Listing_Standard miscSettings = miscSettingsSection.BeginSection(sectionHeightSize, 10, 10);

            if (miscSettings.ButtonTextLabeled("PO.PerformanceSettings".Translate(), "Reset".Translate()))
            {
                foreach (Optimization tweak in perfTweaks)
                {
                    tweak.Reset();
                }
            }

            miscSettings.GapLine(8);
            foreach (Optimization tweak in perfTweaks.OrderBy(x => x))
            {
                tweak.DrawSettings(miscSettings);
            }
            miscSettingsSection.EndSection(miscSettings);
            miscSettingsSection.End();

            Listing_Standard cacheSection = new();
            Rect topRect = new(inRect.x, uiSettingsRect.yMax + 15, inRect.width - 30, cacheSettingsHeight);
            cacheSection.Begin(topRect);
            Listing_Standard cacheSettings = cacheSection.BeginSection(cacheSettingsHeight - 20, 10, 10);
            if (cacheSettings.ButtonTextLabeled("PO.CacheSettings".Translate(), "Reset".Translate()))
            {
                foreach (Optimization optimization in throttles)
                {
                    optimization.Reset();
                }
            }
            cacheSettings.GapLine(8);
            foreach (Optimization optimization in throttles.OrderBy(x => x))
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
