using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class MapComponent_Profiling : MapComponent
    {
        public MapComponent_Profiling(Map map) : base(map)
        {

        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                var things = map.listerThings.AllThings.OfType<ThingWithComps>().ToList();
                var oldStopwatch = new Stopwatch();

                var dictWithThing = new Dictionary<ThingWithComps, ThingWithComps>();
                var dictWithInt = new Dictionary<int, ThingWithComps>();
                foreach (var thing in things)
                {
                    dictWithThing[thing] = thing;
                    dictWithInt[thing.thingIDNumber] = thing;
                }
                oldStopwatch.Start();
                foreach (var thing in things)
                {
                    dictWithThing.TryGetValue(thing, out var val);
                }
                oldStopwatch.Stop();
                Log.Message("Profiled dict with thing: " + (float)oldStopwatch.ElapsedTicks / Stopwatch.Frequency);

                var newStopwatch = new Stopwatch();
                newStopwatch.Start();
                foreach (var thing in things)
                {
                    dictWithInt.TryGetValue(thing.thingIDNumber, out var val);
                }
                newStopwatch.Stop();
                Log.Message("Profiled dict with int: " + (float)newStopwatch.ElapsedTicks / Stopwatch.Frequency);
            }
        }
    }
}
