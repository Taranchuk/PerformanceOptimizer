﻿using HarmonyLib;
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
using Verse.Noise;

namespace PerformanceOptimizer
{
    public class TPSCounter
    {
        public int tpsActual = 0;
        public int fpsActual = 0;
        public List<int> allTps = new List<int>();
        public List<int> allFps = new List<int>();

        public int tpsAverageCached;
        public int fpsAverageCached;
    }
    public class Watcher : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Dev;
        public override string Label => "";
        private static int tpsTarget = 0;
        private static int prevFrames;
        private static DateTime prevTime;
        private static int prevTicks = -1;
        public static Dictionary<TimeSpeed, TPSCounter> tpsDataByTargets = new Dictionary<TimeSpeed, TPSCounter>();
        public static TimeSpeed curTimeSpeed;
        public static bool renderSettings = false;
        public static DateTime secondStartCollecting;
        public override bool EnabledByDefault => false;

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(UIRoot_Play), nameof(UIRoot_Play.UIRootOnGUI), GetMethod(nameof(Prefix)));
            Patch(typeof(TimeSlower), "SignalForceNormalSpeedShort", GetMethod(nameof(PreventPrefix)));
            Patch(typeof(TimeSlower), "SignalForceNormalSpeed", GetMethod(nameof(PreventPrefix)));
        }

        private static bool PreventPrefix()
        {
            return false;
        }
        public static void Prefix()
        {
            if (curTimeSpeed != Find.TickManager.CurTimeSpeed)
            {
                curTimeSpeed = Find.TickManager.CurTimeSpeed;
                Reset();
                return;
            }
            TPSCounter stats = RecordData();
            Rect rect = new Rect(500, 500, 300, 100);
            var mod = LoadedModManager.GetMod<PerformanceOptimizerMod>();
            Find.WindowStack.ImmediateWindow(65465423, rect, WindowLayer.Super, delegate
            {
                Text.Font = GameFont.Small;
                var labelRect = new Rect(15, 15, rect.width - 15, 26);
                Widgets.Label(labelRect, $"TPS: {stats.tpsActual}({tpsTarget}) - average: {stats.tpsAverageCached}");
                labelRect.y += 26;
                Widgets.Label(labelRect, $"FPS: {stats.fpsActual} - average: {stats.fpsAverageCached}");
                labelRect.y += 26;
                labelRect.width = 120;
                if (Widgets.ButtonText(labelRect, "Call settings"))
                {
                    renderSettings = !renderSettings;
                }
                Text.Font = GameFont.Small;
            });
            if (renderSettings)
            {
                Text.Font = GameFont.Small;
                Rect inRect2 = new Rect(0f, 40f, 900f, 700f - 40f - Window.CloseButSize.y);
                mod.DoSettingsWindowContents(inRect2);
            }
        }
        public static void Reset()
        {
            prevTicks = -1;
            tpsDataByTargets.Clear();
            secondStartCollecting = DateTime.Now.AddSeconds(2);
        }

        private static TPSCounter RecordData()
        {
            if (!tpsDataByTargets.TryGetValue(curTimeSpeed, out var stats))
            {
                tpsDataByTargets[curTimeSpeed] = stats = new TPSCounter();
            }
            float trm = Find.TickManager.TickRateMultiplier;
            tpsTarget = (int)Math.Round((trm == 0f) ? 0f : (60f * trm));
            var currTime = DateTime.Now;
            if (currTime > secondStartCollecting)
            {
                if (prevTicks == -1)
                {
                    prevTicks = GenTicks.TicksAbs;
                    prevTime = DateTime.Now;
                }
                else
                {
                    if (currTime.Second != prevTime.Second)
                    {
                        prevTime = currTime;
                        stats.fpsActual = prevFrames;
                        stats.allFps.Add(stats.fpsActual);
                        stats.tpsActual = GenTicks.TicksAbs - prevTicks;
                        prevTicks = GenTicks.TicksAbs;
                        if (stats.tpsActual > 0 && stats.tpsActual <= tpsTarget)
                        {
                            Log.Message("UPDATING now: " + stats.tpsActual + " - GenTicks.TicksAbs: " + GenTicks.TicksAbs + " - " + prevTicks);
                            stats.allTps.Add(stats.tpsActual);
                        }
                        prevFrames = 0;
                        if (stats.allTps.Count > 1 && stats.allFps.Count > 1)
                        {
                            stats.tpsAverageCached = (int)stats.allTps.Average();
                            stats.fpsAverageCached = (int)stats.allFps.Average();
                        }
                    }
                }
                prevFrames++;
            }
            return stats;
        }
    }
}