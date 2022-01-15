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

        float oldTotal = 0;
        float newTotal = 0;
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                var things = map.listerThings.AllThings;
                if (Rand.Bool)
                {
                    ProfileNewCache(things);
                }
                else
                {
                    ProfileVanillaMethods(things);
                }
                Log.Message("---------------");
            }
        }

        private void ProfileVanillaMethods(List<Thing> things)
        {
            var oldStopwatch = new Stopwatch();
            oldStopwatch.Start();
            foreach (var thing in things)
            {
                GetModExtension<VanillaApparelExpanded.ApparelExtension>(thing.def);
            }
            //foreach (var hediffDef in DefDatabase<HediffDef>.AllDefs)
            //{
            //    CompProps<HediffCompProperties_CauseMentalState>(hediffDef);
            //}
            oldStopwatch.Stop();
            oldTotal += (float)oldStopwatch.ElapsedTicks / Stopwatch.Frequency;
            Log.Message("Profiled old: " + (float)oldStopwatch.ElapsedTicks / Stopwatch.Frequency + " - total: " + oldTotal);
        }

        private void ProfileNewCache(List<Thing> things)
        {
            var newStopwatch = new Stopwatch();
            newStopwatch.Start();
            foreach (var thing in things)
            {
                ComponentCache.NonGenericGetModExtensionFast(thing.def);
            }
            //foreach (var hediffDef in DefDatabase<HediffDef>.AllDefs)
            //{
            //    ComponentCache.GetHediffDefPropsFast<HediffCompProperties_CauseMentalState>(hediffDef);
            //}
            newStopwatch.Stop();
            newTotal += (float)newStopwatch.ElapsedTicks / Stopwatch.Frequency;
            Log.Message("Profiled new: " + (float)newStopwatch.ElapsedTicks / Stopwatch.Frequency + " - total: " + newTotal);
        }

        public T CompProps<T>(HediffDef def) where T : HediffCompProperties
        {
            if (def.comps != null)
            {
                for (int i = 0; i < def.comps.Count; i++)
                {
                    T val = def.comps[i] as T;
                    if (val != null)
                    {
                        return val;
                    }
                }
            }
            return null;
        }

        public T GetModExtension<T>(ThingDef def) where T : DefModExtension
        {
            if (def.modExtensions == null)
            {
                return null;
            }
            for (int i = 0; i < def.modExtensions.Count; i++)
            {
                if (def.modExtensions[i] is T)
                {
                    return def.modExtensions[i] as T;
                }
            }
            return null;
        }
    }
}
