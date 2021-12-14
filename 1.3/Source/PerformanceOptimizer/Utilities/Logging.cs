using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Verse;

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

        private static readonly Dictionary<Stopwatch, StopwatchData> stopwatches = new Dictionary<Stopwatch, StopwatchData>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogTime(this Stopwatch stopwatch, string log, int limit = 1)
        {
            if (!stopwatches.TryGetValue(stopwatch, out var stats))
            {
                stopwatches[stopwatch] = stats = new StopwatchData();
            }
            stopwatch.Stop();
            var elapsed = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            stats.count++;
            stats.total += elapsed;
            if (stats.count >= limit)
            {
                Log.Message(log + "it took: " + stats.total);
                stats.total = 0;
                stats.count = 0;
            }
        }
    }
}
