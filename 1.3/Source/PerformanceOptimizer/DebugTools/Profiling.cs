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
                oldStopwatch.Start();
                for (var i = 0; i < 10000; i++)
                {
                    foreach (var thing in things)
                    {
                        ComponentCache.GetCompOld<CompForbiddable>(thing);
                    }
                }
                oldStopwatch.Stop();
                Log.Message("Profiled old comp on CompForbiddable: " + (float)oldStopwatch.ElapsedTicks / Stopwatch.Frequency);

                var newStopwatch = new Stopwatch();
                newStopwatch.Start();
                for (var i = 0; i < 10000; i++)
                {
                    foreach (var thing in things)
                    {
                        ComponentCache.ICache_ThingComp<CompForbiddable>.GetCompNew(thing);
                    }
                }
                newStopwatch.Stop();
                Log.Message("Profiled new comp on CompForbiddable: " + (float)newStopwatch.ElapsedTicks / Stopwatch.Frequency);
            }
        }
    }
}
