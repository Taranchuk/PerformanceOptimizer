using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PerformanceOptimizer
{

    [HarmonyPatch(typeof(BuildCopyCommandUtility))]
    [HarmonyPatch("FindAllowedDesignator")]
    public static class Patch_FindAllowedDesignator
    {
        private static Dictionary<BuildableDef, Designator_Build> cache = new Dictionary<BuildableDef, Designator_Build>();

        private static int lastCacheTick = -1;
        public static bool Prefix(ref Designator_Build __result, BuildableDef buildable, bool mustBeVisible = true)
        {
            __result = FindAllowedDesignator(buildable, mustBeVisible);
            return false;
        }
        public static Designator_Build FindAllowedDesignator(BuildableDef buildable, bool mustBeVisible = true)
        {
            Game game = Current.Game;
            if (game != null)
            {
                if (game.tickManager.TicksGame > lastCacheTick + 1000)
                {
                    cache.Clear();
                    lastCacheTick = game.tickManager.TicksGame;
                }
                if (cache.TryGetValue(buildable, out var value))
                {
                    return value;
                }
            }
            else
            {
                cache.Clear();
            }
            List<DesignationCategoryDef> allDefsListForReading = DefDatabase<DesignationCategoryDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                List<Designator> allResolvedDesignators = allDefsListForReading[i].AllResolvedDesignators;
                for (int j = 0; j < allResolvedDesignators.Count; j++)
                {
                    Designator_Build designator_Build = BuildCopyCommandUtility.FindAllowedDesignatorRecursive(allResolvedDesignators[j], buildable, mustBeVisible);
                    if (designator_Build != null)
                    {
                        if (!cache.ContainsKey(buildable))
                        {
                            cache.Add(buildable, designator_Build);
                        }
                        return designator_Build;
                    }
                }
            }
            if (!cache.ContainsKey(buildable))
            {
                cache.Add(buildable, null);
            }
            return null;
        }
    }

    [HarmonyPatch(typeof(ResourceReadout))]
    [HarmonyPatch("ResourceReadoutOnGUI")]
    public static class Patch_ResourceReadoutOnGUI
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ResourceReadout __instance)
        {
            float width = Prefs.ResourceReadoutCategorized ? 124f : 110f;
            Rect rect2 = new Rect(0f, 0f, width, Mathf.Max(__instance.lastDrawnHeight + 50, 200));
            if (!Mouse.IsOver(rect2))
            {
                return false;
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(MainButtonsRoot), "DoButtons")]
    public static class MainButtonsRoot_DoButtons
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            if (Event.current.mousePosition.y < UI.screenHeight - 35)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GlobalControlsUtility), "DoPlaySettings")]
    public static class DoPlaySettings_DoPlaySettings
    {
        [TweakValue("0", 0, 2000)] public static float xTest = 150;
        [TweakValue("0", 0, 2000)] public static float yTest = 150;
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(WidgetRow rowVisibility, bool worldView, ref float curBaseY)
        {
            if (Event.current.mousePosition.x < (UI.screenWidth - xTest) || Event.current.mousePosition.y < (UI.screenHeight - yTest))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GlobalControlsUtility), "DoTimespeedControls")]
    public static class DoPlaySettings_DoTimespeedControls
    {
        [HarmonyPriority(Priority.Last)]
        public static bool Prefix()
        {
            DoTimeControlsGUI();
            return false;
        }
        public static void DoTimeControlsGUI()
        {
            TickManager tickManager = Find.TickManager;
            if (Event.current.type != EventType.KeyDown)
            {
                return;
            }
            if (KeyBindingDefOf.TogglePause.KeyDownEvent)
            {
                Find.TickManager.TogglePaused();
                TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
                Event.current.Use();
            }
            if (!Find.WindowStack.WindowsForcePause)
            {
                if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
                {
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Normal;
                    TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
                    Event.current.Use();
                }
                if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
                {
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Fast;
                    TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
                    Event.current.Use();
                }
                if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
                {
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Superfast;
                    TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
                    Event.current.Use();
                }
            }

            if (KeyBindingDefOf.TimeSpeed_Ultrafast.KeyDownEvent)
            {
                Find.TickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                TimeControls.PlaySoundOf(Find.TickManager.CurTimeSpeed);
                Event.current.Use();
            }
            if (KeyBindingDefOf.Dev_TickOnce.KeyDownEvent && tickManager.CurTimeSpeed == TimeSpeed.Paused)
            {
                tickManager.DoSingleTick();
                SoundDefOf.Clock_Stop.PlayOneShotOnCamera();
            }
        }
    }
}
