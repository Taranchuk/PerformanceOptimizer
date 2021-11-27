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
    [HarmonyPatch(typeof(ResourceReadout))]
    [HarmonyPatch("ResourceReadoutOnGUI")]
    public static class Patch_ResourceReadoutOnGUI
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ResourceReadout __instance)
        {
            if (PerformanceOptimizerSettings.hideResourceReadout)
            {
                float width = Prefs.ResourceReadoutCategorized ? 124f : 110f;
                Rect rect2 = new Rect(0f, 0f, width, Mathf.Max(__instance.lastDrawnHeight + 50, 200));
                if (!Mouse.IsOver(rect2))
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AlertsReadout), "AlertsReadoutOnGUI")]
    public static class AlertsReadoutOnGUI_Prefix
    {
        [TweakValue("0", 0, 2000)] public static float xTest = 154;
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(AlertsReadout __instance)
        {
            if (PerformanceOptimizerSettings.minimizeAlertsReadout)
            {
                if (Event.current.mousePosition.x < (UI.screenWidth - xTest))
                {
                    AlertsReadoutOnGUI_Mini(__instance);
                    return false;
                }
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
            if (PerformanceOptimizerSettings.hideBottomButtonBar)
            {
                if (Event.current.mousePosition.y < UI.screenHeight - 35)
                {
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LetterStack), "LettersOnGUI")]
    public static class LetterStack_LettersOnGUI
    {
        public static float lettersBottomY;
        public static void Prefix(float baseY)
        {
            lettersBottomY = baseY;
        }
    }

    [HarmonyPatch(typeof(GlobalControlsUtility), "DoPlaySettings")]
    public static class DoPlaySettings_DoPlaySettings
    {
        [TweakValue("0", 0, 2000)] public static float xTest = 150;
        [TweakValue("0", -100, 200)] public static float dubsFix = -40;
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(WidgetRow rowVisibility, bool worldView, ref float curBaseY)
        {
            if (PerformanceOptimizerSettings.hideBottomRightOverlayButtons && rowVisibility.FinalY > 0)
            {
                bool mouseOnRight = Event.current.mousePosition.x < (UI.screenWidth - xTest);
                if (mouseOnRight || (!mouseOnRight && LetterStack_LettersOnGUI.lettersBottomY > Event.current.mousePosition.y))
                {
                    if (!worldView)
                    {
                        Find.PlaySettings.CheckKeyBindingToggle(KeyBindingDefOf.ToggleBeautyDisplay, ref Find.PlaySettings.showBeauty);
                        Find.PlaySettings.CheckKeyBindingToggle(KeyBindingDefOf.ToggleRoomStatsDisplay, ref Find.PlaySettings.showRoomStats);
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

    [HarmonyPatch(typeof(GlobalControlsUtility), "DoTimespeedControls")]
    public static class DoPlaySettings_DoTimespeedControls
    {
        [TweakValue("0", 0, 2000)] public static float xTest = 154;
        [HarmonyPriority(Priority.Last)]
        public static bool Prefix()
        {
            if (PerformanceOptimizerSettings.disableSpeedButtons)
            {
                DoTimeControlsGUI();
                return false;
            }
            else if (PerformanceOptimizerSettings.hideSpeedButtons && (Event.current.mousePosition.x < (UI.screenWidth - xTest)))
            {
                DoTimeControlsGUI();
                return false;
            }
            return true;
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
                tickManager.TogglePaused();
                TimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Pause, KnowledgeAmount.SpecificInteraction);
                Event.current.Use();
            }
            if (!Find.WindowStack.WindowsForcePause)
            {
                if (KeyBindingDefOf.TimeSpeed_Normal.KeyDownEvent)
                {
                    tickManager.CurTimeSpeed = TimeSpeed.Normal;
                    TimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
                    Event.current.Use();
                }
                if (KeyBindingDefOf.TimeSpeed_Fast.KeyDownEvent)
                {
                    tickManager.CurTimeSpeed = TimeSpeed.Fast;
                    TimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
                    Event.current.Use();
                }
                if (KeyBindingDefOf.TimeSpeed_Superfast.KeyDownEvent)
                {
                    tickManager.CurTimeSpeed = TimeSpeed.Superfast;
                    TimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.TimeControls, KnowledgeAmount.SpecificInteraction);
                    Event.current.Use();
                }
            }

            if (KeyBindingDefOf.TimeSpeed_Ultrafast.KeyDownEvent)
            {
                tickManager.CurTimeSpeed = TimeSpeed.Ultrafast;
                TimeControls.PlaySoundOf(tickManager.CurTimeSpeed);
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
