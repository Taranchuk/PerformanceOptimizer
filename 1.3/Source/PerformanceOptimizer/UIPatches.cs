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

    [HarmonyPatch(typeof(AlertsReadout), "AlertsReadoutUpdate")]
    public static class AlertsReadoutUpdate_Prefix
    {
    
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(AlertsReadout __instance)
        {
            if (Event.current.mousePosition.x < (UI.screenWidth - AlertsReadoutOnGUI_Prefix.xTest))
            {
                AlertsReadoutUpdate_Mini(__instance);
                return false;
            }
            return true;
        }

        public static void AlertsReadoutUpdate_Mini(AlertsReadout __instance)
        {
            __instance.curAlertIndex++;
            if (__instance.curAlertIndex >= 24)
            {
                __instance.curAlertIndex = 0;
            }
            var alerts = __instance.AllAlerts.Where(x => x.Priority == AlertPriority.Critical || x.Priority == AlertPriority.High).ToList();
            for (int i = __instance.curAlertIndex; i < alerts.Count; i += 24)
            {
                __instance.CheckAddOrRemoveAlert(alerts[i]);
            }

            for (int num3 = alerts.Count - 1; num3 >= 0; num3--)
            {
                Alert alert = alerts[num3];
                try
                {
                    alerts[num3].AlertActiveUpdate();
                }
                catch (Exception ex)
                {
                    Log.ErrorOnce("Exception updating alert " + alert.ToString() + ": " + ex.ToString(), 743575);
                    alerts.RemoveAt(num3);
                }
            }
        }
    }


    [HarmonyPatch(typeof(AlertsReadout), "AlertsReadoutOnGUI")]
    public static class AlertsReadoutOnGUI_Prefix
    {
        [TweakValue("0", 0, 2000)] public static float xTest = 154;
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(AlertsReadout __instance)
        {
            if (Event.current.mousePosition.x < (UI.screenWidth - xTest))
            {
                AlertsReadoutOnGUI_Mini(__instance);
                return false;
            }
            return true;
        }

        private static float lastFinalY;
        public static void AlertsReadoutOnGUI_Mini(AlertsReadout __instance)
        {
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.MouseDrag || __instance.activeAlerts.Count == 0)
            {
                return;
            }
            Alert alert = null;
            AlertPriority alertPriority = AlertPriority.Critical;
            bool flag = false;
            float num = 0f;
            var activeAlerts = __instance.activeAlerts.Where(x => x.Priority == AlertPriority.Critical || x.Priority == AlertPriority.High).ToList();
            for (int i = 0; i < activeAlerts.Count; i++)
            {
                num += activeAlerts[i].Height;
            }
            float num2 = Find.LetterStack.LastTopY - num;
            Rect rect = new Rect((float)UI.screenWidth - 154f, num2, 154f, lastFinalY - num2);
            float num3 = GenUI.BackgroundDarkAlphaForText();
            if (num3 > 0.001f)
            {
                GUI.color = new Color(1f, 1f, 1f, num3);
                Widgets.DrawShadowAround(rect);
                GUI.color = Color.white;
            }
            float num4 = num2;
            if (num4 < 0f)
            {
                num4 = 0f;
            }
            for (int j = 0; j < __instance.PriosInDrawOrder.Count; j++)
            {
                AlertPriority alertPriority2 = __instance.PriosInDrawOrder[j];
                for (int k = 0; k < activeAlerts.Count; k++)
                {
                    Alert alert2 = activeAlerts[k];
                    if (alert2.Priority == alertPriority2)
                    {
                        if (!flag)
                        {
                            alertPriority = alertPriority2;
                            flag = true;
                        }
                        Rect rect2 = alert2.DrawAt(num4, alertPriority2 != alertPriority);
                        if (Mouse.IsOver(rect2))
                        {
                            alert = alert2;
                            __instance.mouseoverAlertIndex = k;
                        }
                        num4 += rect2.height;
                    }
                }
            }
            lastFinalY = num4;
            UIHighlighter.HighlightOpportunity(rect, "Alerts");
            if (alert != null)
            {
                alert.DrawInfoPane();
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Alerts, KnowledgeAmount.FrameDisplayed);
                __instance.CheckAddOrRemoveAlert(alert);
            }
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
