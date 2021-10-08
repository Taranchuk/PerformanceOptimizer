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
    public static class Logging
    {
        public class StopwatchData
        {
            public float total;
            public float count;
        }
        public static void StartLog(this Stopwatch stopwatch)
        {
            stopwatch.Restart();
        }

        private static Dictionary<Stopwatch, StopwatchData> stopwatches = new Dictionary<Stopwatch, StopwatchData>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogTime(this Stopwatch stopwatch, string log, int limit = 999999)
        {
            if (!stopwatches.TryGetValue(stopwatch, out var stats))
            {
                stopwatches[stopwatch] = stats = new StopwatchData();
            }

            var elapsed = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            stats.count++;
            stats.total += elapsed;

            if (stats.count > limit)
            {
                Log.Message(log + "it took: " + stats.total);
                foreach (var data in ComponentCache.calledStats.OrderByDescending(x => x.Value))
                {
                    Log.Message("Called: " + data.Key + " - " + data.Value);
                }
                stats.total = 0;
                stats.count = 0;
            }
        }
    }
}
