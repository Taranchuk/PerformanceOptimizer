using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    class PerformanceOptimizerMod : Mod
    {
        public static Harmony harmony;

        public static PerformanceOptimizerSettings settings;

        public static TickManager tickManager;

        public static bool DubsPerformanceAnalyzerLoaded;
        public PerformanceOptimizerMod(ModContentPack mod) : base(mod)
        {
            harmony = new Harmony("PerformanceOptimizer.Main");
            GameLoad_Patches.DoPatches();
            harmony.PatchAll();
            var hooks = new List<MethodInfo>
            {
                AccessTools.Method(typeof(MapDeiniter), "Deinit"),
                AccessTools.Method(typeof(Game), "AddMap"),
                AccessTools.Method(typeof(World), "FillComponents"),
                AccessTools.Method(typeof(Game), "FillComponents"),
                AccessTools.Method(typeof(MapComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(WorldComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(GameComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(Game), "InitNewGame"),
                AccessTools.Method(typeof(Game), "LoadGame"),
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

        public override string SettingsCategory()
        {
            return this.Content.Name;
        }

        public static void ResetStaticData()
        {
            tickManager = Current.Game?.tickManager;
            ComponentCache.cachedWorldComps.Clear();
            ComponentCache.cachedGameComps.Clear();
            CompsOfType<Map>.mapCompsByMap.Clear();

            Patch_InspectGizmoGrid_DrawInspectGizmoGridFor.cachedResults.Clear();
            PawnCollisionPosOffsetFor.cachedResults.Clear();
            Patch_BuildCopyCommandUtility_FindAllowedDesignator.cachedResults.Clear();
            Patch_Thing_AmbientTemperature.cachedResults.Clear();
            Patch_IdeoUtility_GetStyleDominance.cachedResults.Clear();
            Patch_Need_Beauty_CurrentInstantBeauty.cachedResults.Clear();
            Patch_MentalBreaker_BreakThresholdExtreme.cachedResults.Clear();
            Patch_MentalBreaker_BreakThresholdMajor.cachedResults.Clear();
            Patch_MentalBreaker_BreakThresholdMinor.cachedResults.Clear();
            Patch_ThoughtHandler_TotalMoodOffset.cachedResults.Clear();
            Patch_PawnUtility_IsTeetotaler.cachedResults.Clear();
            Patch_QuestUtility_IsQuestLodger.cachedResults.Clear();
            Patch_ExpectationsUtility_CurrentExpectationForPawn.cachedResults.Clear();
            Patch_ExpectationsUtility_CurrentExpectationFor_Map.cachedResults.Clear();
            Patch_JobDriver_CheckCurrentToilEndOrFail.cachedResults.Clear();
            Patch_Faction_FactionOfPlayer.factionOfPlayer = null;
        }
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

    [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), "CallAll")]
    public static class StaticConstructorOnStartupUtilityCallAll
    {
        public static void Postfix()
        {
            PerformanceOptimizerMod.DubsPerformanceAnalyzerLoaded = ModLister.AllInstalledMods.Any(x => x.Active && x.Name.Contains("Dubs Performance Analyzer"));
            CachingPatches.DoPatches();
            if (PerformanceOptimizerSettings.fasterGetCompReplacement)
            {
                GetCompPatches.DoPatchesAsync();
            }
        }
    }
}
