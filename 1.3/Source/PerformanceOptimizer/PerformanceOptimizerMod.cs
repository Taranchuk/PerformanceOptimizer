﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    internal class PerformanceOptimizerMod : Mod
    {
        public static Harmony harmony;

        public static PerformanceOptimizerSettings settings;

        public static TickManager tickManager;

        public static bool DubsPerformanceAnalyzerLoaded;
        public PerformanceOptimizerMod(ModContentPack mod) : base(mod)
        {
            harmony = new Harmony("PerformanceOptimizer.Main");
            harmony.PatchAll();
            var hooks = new List<MethodInfo>
            {
                AccessTools.Method(typeof(MapDeiniter), "Deinit"),
                AccessTools.Method(typeof(Game), "AddMap"),
                AccessTools.Method(typeof(World), "FillComponents"),
                AccessTools.Method(typeof(Game), "FillComponents"),
                AccessTools.Method(typeof(Map), "FillComponents"),
                AccessTools.Method(typeof(MapComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(WorldComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(GameComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(Game), "InitNewGame"),
                AccessTools.Method(typeof(Game), "LoadGame"),
                AccessTools.Method(typeof(GameInitData), "ResetWorldRelatedMapInitData"),
                AccessTools.Method(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow")
            };

            foreach (var hook in hooks)
            {
                harmony.Patch(hook, new HarmonyMethod(typeof(PerformanceOptimizerMod), nameof(PerformanceOptimizerMod.ResetStaticData)));
            }

            settings = GetSettings<PerformanceOptimizerSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            KeyPrefs.Save();
        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }

        public static void ResetStaticData()
        {
            tickManager = Current.Game?.tickManager;
            foreach (var optimization in PerformanceOptimizerSettings.optimizations)
            {
                optimization?.Clear();
            }
        }
        public static KeyPrefsData keyPrefsData;
    }

    [HarmonyPatch(typeof(Map), "FinalizeInit")]
    public static class Map_FinalizeInit
    {
        public static void Postfix()
        {
            if (!PerformanceOptimizerSettings.overviewLetterSent)
            {
                Find.LetterStack.ReceiveLetter("PO.PerformanceOptimizerOverview".Translate(), "PO.PerformanceOptimizerOverviewDesc".Translate(), LetterDefOf.PositiveEvent);
                PerformanceOptimizerSettings.overviewLetterSent = true;
                LoadedModManager.GetMod<PerformanceOptimizerMod>().WriteSettings();
            }
        }
    }

    [HarmonyPatch]
    public static class InitializeMod
    {
        public static MethodBase targetMethod;

        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(x => x.Name == "BetterLoading"))
            {
                return AccessTools.Method("BetterLoading.BetterLoadingMain:CreateTimingReport");
            }
            return AccessTools.Method(typeof(StaticConstructorOnStartupUtility), "CallAll");
        }
        public static void Postfix()
        {
            PerformanceOptimizerMod.keyPrefsData = KeyPrefs.KeyPrefsData;
            PerformanceOptimizerMod.DubsPerformanceAnalyzerLoaded = ModLister.AllInstalledMods.Any(x => x.Active && x.Name.Contains("Dubs Performance Analyzer"));
            PerformanceOptimizerSettings.Initialize();
        }
    }

    [HarmonyPatch(typeof(Log), nameof(Log.Error), new Type[] { typeof(string) })]
    public static class Log_Error_Patch
    {
        public static bool suppressErrorMessages;
        public static bool Prefix()
        {
            if (suppressErrorMessages)
            {
                return false;
            }
            return true;
        }
    }
}
