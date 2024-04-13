using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class TPSCounter
    {
        public int tpsActual = 0;
        public int fpsActual = 0;
        public List<int> allTps = new();
        public List<int> allFps = new();
        public int tpsAverageCached;
        public int fpsAverageCached;
    }
    public class Watcher : Optimization
    {
        public override bool IsEnabled => false;
        public override OptimizationType OptimizationType => OptimizationType.Dev;
        public override string Label => "";
        private static int tpsTarget = 0;
        private static int prevFrames;
        private static DateTime prevTime;
        private static int prevTicks = -1;
        public static Dictionary<TimeSpeed, TPSCounter> tpsDataByTargets = new();
        public static TimeSpeed curTimeSpeed;
        public static bool renderSettings = false;
        public static DateTime timeStartCollectingData;
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
                ResetData();
                return;
            }
            TPSCounter stats = RecordData();
            Rect rect = new(500, 500, 300, 100);
            PerformanceOptimizerMod mod = LoadedModManager.GetMod<PerformanceOptimizerMod>();
            Find.WindowStack.ImmediateWindow(65465423, rect, WindowLayer.Super, delegate
            {
                Text.Font = GameFont.Small;
                Rect labelRect = new(15, 15, rect.width - 15, 26);
                Widgets.Label(labelRect, $"TPS: {stats.tpsActual} - average: {stats.tpsAverageCached}");
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
                Rect inRect2 = new(0f, 40f, 900f, 700f - 40f - Window.CloseButSize.y);
                mod.DoSettingsWindowContents(inRect2);
            }
        }
        public static void ResetData()
        {
            prevTicks = -1;
            tpsDataByTargets.Clear();
            timeStartCollectingData = DateTime.Now.AddSeconds(2);
        }

        private static TPSCounter RecordData()
        {
            if (!tpsDataByTargets.TryGetValue(curTimeSpeed, out TPSCounter stats))
            {
                tpsDataByTargets[curTimeSpeed] = stats = new TPSCounter();
            }
            DateTime currTime = DateTime.Now;
            if (currTime > timeStartCollectingData)
            {
                float trm = Find.TickManager.TickRateMultiplier;
                tpsTarget = (int)Math.Round((trm == 0f) ? 0f : (60f * trm));
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
