﻿using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class PerformanceOptimizerMod : Mod
    {
        public static Harmony harmony;

        public static PerformanceOptimizerSettings settings;

        public static TickManager tickManager;

        public static bool DubsPerformanceAnalyzerLoaded;
        public static PerformPatchesPerFrames performPatchesPerFrames;
        public PerformanceOptimizerMod(ModContentPack mod) : base(mod)
        {
            harmony = new Harmony("PerformanceOptimizer.Main");
            harmony.PatchAll();
            List<MethodInfo> hooks = new()
            {
                AccessTools.Method(typeof(MapDeiniter), nameof(MapDeiniter.Deinit)),
                AccessTools.Method(typeof(Game), nameof(Game.AddMap)),
                AccessTools.Method(typeof(World), nameof(World.FillComponents)),
                AccessTools.Method(typeof(Game), nameof(Game.FillComponents)),
                AccessTools.Method(typeof(Map), nameof(Map.FillComponents)),
                AccessTools.Method(typeof(MapComponentUtility), nameof(MapComponentUtility.FinalizeInit)),
                AccessTools.Method(typeof(WorldComponentUtility), nameof(WorldComponentUtility.FinalizeInit)),
                AccessTools.Method(typeof(GameComponentUtility), nameof(GameComponentUtility.FinalizeInit)),
                AccessTools.Method(typeof(Game), nameof(Game.InitNewGame)),
                AccessTools.Method(typeof(Game), nameof(Game.LoadGame)),
                AccessTools.Method(typeof(GameInitData), nameof(GameInitData.ResetWorldRelatedMapInitData)),
                AccessTools.Method(typeof(SavedGameLoaderNow), nameof(SavedGameLoaderNow.LoadGameFromSaveFileNow)),
            };

            foreach (MethodInfo hook in hooks)
            {
                try
                {
                    harmony.Patch(hook, new HarmonyMethod(typeof(PerformanceOptimizerMod), nameof(PerformanceOptimizerMod.ResetStaticData)));
                }
                catch (Exception e)
                {
                    Log.Error("Failed to patch " + hook);
                }
            }
            var gameObject = new GameObject("PerformanceOptimizerMod");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            performPatchesPerFrames = gameObject.AddComponent<PerformPatchesPerFrames>();
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
            foreach (Optimization optimization in PerformanceOptimizerSettings.optimizations)
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
            return ModsConfig.ActiveModsInLoadOrder.Any(x => x.Name == "BetterLoading")
                ? AccessTools.Method("BetterLoading.BetterLoadingMain:CreateTimingReport")
                : (MethodBase)AccessTools.Method(typeof(StaticConstructorOnStartupUtility), "CallAll");
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
            return !suppressErrorMessages;
        }
    }
}
