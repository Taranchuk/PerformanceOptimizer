using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class Optimization_DoPlaySettings_DoTimespeedControls : Optimization_UITweaks
    {
        public override string Label => "PO.HideSpeedButtons".Translate();
        public string SecondLabel => "PO.DisableSpeedButtons".Translate();

        public static bool hideSpeedButtons;

        public static bool disableSpeedButtons;
        public override int DrawHeight => base.DrawHeight * 2;

        public override void Apply()
        {
            if (hideSpeedButtons || disableSpeedButtons)
            {
                enabled = true;
            }
            else
            {
                enabled = false;
            }
            base.Apply();
        }
        public override void DrawSettings(Listing_Standard section)
        {
            section.CheckboxLabeled(Label, ref hideSpeedButtons, actionOnClick: this.Apply);
            section.CheckboxLabeled(SecondLabel, ref disableSpeedButtons, actionOnClick: this.Apply);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hideSpeedButtons, "hideSpeedButtons");
            Scribe_Values.Look(ref disableSpeedButtons, "disableSpeedButtons");
        }
        public override void Reset()
        {
            base.Reset();
            hideSpeedButtons = false;
            disableSpeedButtons = false;
        }
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(GlobalControlsUtility), "DoTimespeedControls", GetMethod(nameof(Prefix)));
        }

        [TweakValue("0", 0, 2000)] public static float xTest = 154;
        [HarmonyPriority(Priority.Last)]
        public static bool Prefix()
        {
            if (Optimization_UIToggle.UIToggleOn)
            {
                if (disableSpeedButtons)
                {
                    DoTimeControlsGUI();
                    return false;
                }
                else if (hideSpeedButtons && (Event.current.mousePosition.x < (UI.screenWidth - xTest)))
                {
                    DoTimeControlsGUI();
                    return false;
                }
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
