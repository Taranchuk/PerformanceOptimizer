using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_AlertsReadoutOnGUI_Prefix : Optimization_UITweaks
    {
        public override string Label => "PO.MinimizeAlertsReadout".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(AlertsReadout), "AlertsReadoutOnGUI", GetMethod(nameof(Prefix)));
        }

        [TweakValue("0", 0, 2000)] public static float xTest = 154;

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(AlertsReadout __instance)
        {
            if (Optimization_UIToggle.UIToggleOn)
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
            System.Collections.Generic.List<Alert> activeAlerts = __instance.activeAlerts.Where(x => x.Priority is AlertPriority.Critical or AlertPriority.High).ToList();
            for (int i = 0; i < activeAlerts.Count; i++)
            {
                num += activeAlerts[i].Height;
            }
            float num2 = Find.LetterStack.LastTopY - num;
            Rect rect = new(UI.screenWidth - 154f, num2, 154f, lastFinalY - num2);
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
}
