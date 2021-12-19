using HarmonyLib;
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
        public Dictionary<MethodInfo, List<MethodInfo>> patchedMethods;
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
                patchedMethods ??= new Dictionary<MethodInfo, List<MethodInfo>>();
                DoPatches();
            }
            Watcher.ResetData();
        }

        public virtual void DoPatches() { }

        public void Patch(Type type, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            var originalMethod = AccessTools.Method(type, methodName);
            Patch(originalMethod, prefix, postfix, transpiler);
        }

        public static Dictionary<MethodBase, Type> mappedValues = new Dictionary<MethodBase, Type>();
        public void Patch(MethodInfo methodInfo, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            PerformanceOptimizerMod.harmony.Patch(methodInfo, prefix != null ? new HarmonyMethod(prefix) : null, postfix != null ? new HarmonyMethod(postfix) : null, transpiler != null ? new HarmonyMethod(transpiler) : null);
            //Log.Message(this.GetType() +  " - Patching " + methodInfo.FullDescription());
            //Log.ResetMessageCount();
            List<MethodInfo> patches = new List<MethodInfo>();
            if (prefix != null)
            {
                patches.Add(prefix);
                if (ProfilePerformanceImpact && prefix != null && prefix.ReturnType == typeof(bool))
                {
                    var type = prefix.DeclaringType;
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

        public static Dictionary<Type, List<float>> performanceTweaksOn = new Dictionary<Type, List<float>>();
        public static Dictionary<Type, List<float>> performanceTweaksOff = new Dictionary<Type, List<float>>();
        public static bool profileOn = true;
        public static int lastProfileCheckTick;
        public static Stopwatch stopwatch = new Stopwatch();
        public virtual bool ProfilePerformanceImpact => false; // if you change it to true, don't forget to disable AggressiveInlining atribute on CachedObjectTick and CachedValueTick class methods... they don't get profiled with it
        public const int PROFILINGINTERVAL = 2500;
        struct MeasureData
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
            if (profileOn)
            {
                return true;
            }
            else
            {
                return false;
            }
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
            var elapsed = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            var type = mappedValues[__originalMethod];
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
                    var result = new Dictionary<Type, MeasureData>();
                    foreach (var kvp in performanceTweaksOff)
                    {
                        if (performanceTweaksOn.TryGetValue(kvp.Key, out var performanceOn))
                        {
                            var smallerListCount = kvp.Value.Count > performanceOn.Count ? performanceOn.Count : kvp.Value.Count;
                            var performanceOnNew = performanceOn.Take(smallerListCount).ToList();
                            var performanceOffNew = kvp.Value.Take(smallerListCount).ToList();
                            result[kvp.Key] = new MeasureData
                            {
                                performanceImpactOn = performanceOnNew.Sum(),
                                performanceImpactOff = performanceOffNew.Sum(),
                                perfRate = performanceOffNew.Average() / performanceOnNew.Average()
                            };
                        }
                    }

                    foreach (var r in result.OrderByDescending(x => x.Value.performanceImpactOff))
                    {
                        Log.Message("Result: " + r.Key + " - " + r.Value.Log());
                    }
                }
            }
        }

        public void UnPatchAll()
        {
            if (patchedMethods != null)
            {
                foreach (var kvp in patchedMethods)
                {
                    MethodInfo method = kvp.Key;
                    foreach (var patch in kvp.Value)
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
