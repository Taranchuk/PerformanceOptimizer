﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Verse;

namespace PerformanceOptimizer
{
    public abstract class Optimization : IExposable, IComparable<Optimization>
    {
        public Dictionary<MethodBase, List<MethodInfo>> patchedMethods;
        public virtual bool EnabledByDefault => true;
        public abstract OptimizationType OptimizationType { get; }

        protected bool enabled;
        public virtual bool IsEnabled => enabled;
        public abstract string Label { get; }
        public virtual void DrawSettings(Listing_Standard section)
        {
            section.CheckboxLabeled(Label, ref enabled, actionOnClick: Apply);
        }

        public virtual int DrawOrder => 0;
        public virtual int DrawHeight => 24;
        public virtual void Initialize()
        {
            Reset();
        }

        public virtual void Reset()
        {
            enabled = EnabledByDefault;
            Apply();
        }
        public virtual void Clear()
        {

        }
        public virtual void Apply()
        {
            if (!IsEnabled && patchedMethods != null && patchedMethods.Any())
            {
                UnPatchAll();
            }
            else if (IsEnabled && (patchedMethods is null || !patchedMethods.Any()))
            {
                patchedMethods ??= new Dictionary<MethodBase, List<MethodInfo>>();
                DoPatches();
            }
            Watcher.ResetData();
        }

        public virtual void DoPatches() { }

        public void Patch(Type type, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            MethodInfo originalMethod = AccessTools.Method(type, methodName);
            try
            {
                Patch(originalMethod, prefix, postfix, transpiler);
            }
            catch (Exception e)
            {
                Log.Error("Error patching " + methodName + " for " + this.GetType());
            }
        }

        public static Dictionary<MethodBase, Type> mappedValues = new();
        public void Patch(MethodBase methodInfo, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            var harmonyPrefix = prefix != null ? new HarmonyMethod(prefix) : null;
            var harmonyPostfix = postfix != null ? new HarmonyMethod(postfix) : null;
            var harmonyTranspiler = transpiler != null ? new HarmonyMethod(transpiler) : null;
            PerformanceOptimizerMod.harmony.Patch(methodInfo, prefix: harmonyPrefix, postfix: harmonyPostfix, transpiler: harmonyTranspiler);
            List<MethodInfo> patches = new();
            if (prefix != null)
            {
                patches.Add(prefix);
                if (ProfilePerformanceImpact && prefix != null)
                {
                    if (prefix.ReturnType == typeof(bool))
                    {
                        Type type = prefix.DeclaringType;
                        PerformanceOptimizerMod.harmony.Patch(prefix, prefix: new HarmonyMethod(GetMethod(nameof(MeasureBefore))));
                        if (postfix != null)
                        {
                            PerformanceOptimizerMod.harmony.Patch(postfix, prefix: new HarmonyMethod(GetMethod(nameof(ControlPostfix))), postfix: new HarmonyMethod(GetMethod(nameof(MeasureAfter))));
                            mappedValues[postfix] = type;
                        }
                        else
                        {
                            PerformanceOptimizerMod.harmony.Patch(methodInfo, postfix: new HarmonyMethod(GetMethod(nameof(MeasureAfter))));
                            mappedValues[methodInfo] = type;
                        }
                    }
                }
            }
            if (postfix != null)
            {
                patches.Add(postfix);
            }
            if (transpiler != null)
            {
                patches.Add(transpiler);
            }
            patchedMethods[methodInfo] = patches;
        }

        public static Dictionary<Type, List<float>> performanceTweaksOn = new();
        public static Dictionary<Type, List<float>> performanceTweaksOff = new();
        public static bool profileOn = true;
        public static int lastProfileCheckTick;
        public static Stopwatch stopwatch = new();
        public virtual bool ProfilePerformanceImpact => false; // if you change it to true, don't forget to disable AggressiveInlining atribute on CachedObjectTick and CachedValueTick class methods...
                                                               // they don't get profiled with it
        public const int PROFILINGINTERVAL = 250;
        private struct MeasureData
        {
            public float performanceImpactOn;
            public float performanceImpactOff;
            public float perfRate;
            public string Log()
            {
                return "performance on: " + performanceImpactOn + " - performance off: " + performanceImpactOff + " - perf rate: " + perfRate;
            }
        }

        public static bool ControlPostfix()
        {
            return profileOn;
        }
        public static bool MeasureBefore(ref bool __result)
        {
            stopwatch.Restart();
            if (profileOn)
            {
                return true;
            }
            else
            {
                __result = true;
                return false;
            }
        }
        public static void MeasureAfter(MethodBase __originalMethod)
        {
            stopwatch.Stop();
            float elapsed = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            Type type = mappedValues[__originalMethod];
            RegisterElapsedTicks(elapsed, type);
            if (Current.gameInt?.tickManager != null && Find.TickManager.ticksGameInt > lastProfileCheckTick + PROFILINGINTERVAL)
            {
                LogStats(type);
                profileOn = !profileOn;
                lastProfileCheckTick = Find.TickManager.ticksGameInt;
                Watcher.ResetData();
            }
        }

        private static void RegisterElapsedTicks(float elapsed, Type type)
        {
            if (profileOn)
            {
                if (performanceTweaksOn.ContainsKey(type))
                {
                    performanceTweaksOn[type].Add(elapsed);
                }
                else
                {
                    performanceTweaksOn[type] = new List<float> { elapsed };
                }
            }
            else
            {
                if (performanceTweaksOff.ContainsKey(type))
                {
                    performanceTweaksOff[type].Add(elapsed);
                }
                else
                {
                    performanceTweaksOff[type] = new List<float> { elapsed };
                }
            }
        }

        private static void LogStats(Type type)
        {
            //Log.Message("performanceTweaksOn.ContainsKey(type): " + performanceTweaksOn.ContainsKey(type));
            //Log.Message("performanceTweaksOff.ContainsKey(type): " + performanceTweaksOff.ContainsKey(type));
            if (!profileOn && performanceTweaksOn.ContainsKey(type) && performanceTweaksOff.ContainsKey(type))
            {
                //Log.Message("performanceTweaksOn[type].Count: " + performanceTweaksOn[type].Count);
                //Log.Message("performanceTweaksOff[type].Count: " + performanceTweaksOff[type].Count);
                if (performanceTweaksOff[type].Count > 1 && performanceTweaksOn[type].Count > 1)
                {
                    Log.Message("Profiling result: -------------------");
                    Dictionary<Type, MeasureData> result = new();
                    foreach (KeyValuePair<Type, List<float>> kvp in performanceTweaksOff)
                    {
                        if (performanceTweaksOn.TryGetValue(kvp.Key, out List<float> performanceOn))
                        {
                            int smallerListCount = kvp.Value.Count > performanceOn.Count ? performanceOn.Count : kvp.Value.Count;
                            List<float> performanceOnNew = performanceOn.Take(smallerListCount).ToList();
                            List<float> performanceOffNew = kvp.Value.Take(smallerListCount).ToList();
                            result[kvp.Key] = new MeasureData
                            {
                                performanceImpactOn = performanceOnNew.Sum(),
                                performanceImpactOff = performanceOffNew.Sum(),
                                perfRate = performanceOffNew.Average() / performanceOnNew.Average()
                            };
                        }
                    }

                    foreach (KeyValuePair<Type, MeasureData> r in result.OrderByDescending(x => x.Value.performanceImpactOff))
                    {
                        if (r.Value.perfRate <= 1)
                        {
                            Log.Message("FAIL: Result: " + r.Key + " - " + r.Value.Log());
                        }
                        else
                        {
                            Log.Message("SUCCESS: Result: " + r.Key + " - " + r.Value.Log());
                        }
                    }
                }
            }
        }

        public void UnPatchAll()
        {
            if (patchedMethods != null)
            {
                foreach (KeyValuePair<MethodBase, List<MethodInfo>> kvp in patchedMethods)
                {
                    MethodBase method = kvp.Key;
                    foreach (MethodInfo patch in kvp.Value)
                    {
                        PerformanceOptimizerMod.harmony.Unpatch(method, patch);
                    }
                }
            }
            patchedMethods.Clear();
        }
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled", EnabledByDefault);
        }
        public MethodInfo GetMethod(string name)
        {
            return AccessTools.Method(GetType(), name);
        }

        public int CompareTo(Optimization other)
        {
            return string.Compare(Label, other.Label) + DrawOrder;
        }
    }
}
