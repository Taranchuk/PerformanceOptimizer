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
        public virtual bool EnabledAlways => false;
        public abstract OptimizationType OptimizationType { get; }

        protected bool enabled;
        public bool IsEnabled => enabled || EnabledAlways;
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
                    PerformanceOptimizerMod.harmony.Patch(prefix, prefix: new HarmonyMethod(GetMethod(nameof(MeasurePrefix))));
                    if (postfix != null)
                    {
                        PerformanceOptimizerMod.harmony.Patch(postfix, postfix: new HarmonyMethod(GetMethod(nameof(MeasurePostfix))));
                        mappedValues[postfix] = type;
                    }
                    else
                    {
                        PerformanceOptimizerMod.harmony.Patch(methodInfo, postfix: new HarmonyMethod(GetMethod(nameof(MeasurePostfix))));
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
        public virtual bool ProfilePerformanceImpact => false;
        public const int PROFILINGINTERVAL = 60;
        public static bool MeasurePrefix(ref bool __result)
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
        public static void MeasurePostfix(MethodBase __originalMethod)
        {
            stopwatch.Stop();
            var elapsed = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            var type = mappedValues[__originalMethod];
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

            if (Current.gameInt?.tickManager != null && lastProfileCheckTick != Find.TickManager.ticksGameInt
                && Find.TickManager.ticksGameInt % PROFILINGINTERVAL == 0)
            {
                if (performanceTweaksOn.ContainsKey(type) && performanceTweaksOff.ContainsKey(type))
                {
                    if (performanceTweaksOn[type].Count > 1000)
                    {
                        performanceTweaksOn[type] = performanceTweaksOn[type].Skip(100).ToList();
                        performanceTweaksOff[type] = performanceTweaksOff[type].Skip(100).ToList();
                    }
                    if (!profileOn && performanceTweaksOff.Count > 1 && performanceTweaksOn.Count > 1)
                    {
                        Log.Message("Refreshing: " + profileOn + " - " + performanceTweaksOn[type].Count + " - " + performanceTweaksOff[type].Count);
                        Log.Message("Profiling result: -------------------");
                        var result = new Dictionary<Type, float>();
                        foreach (var kvp in performanceTweaksOff.OrderByDescending(x => x.Value.Sum()))
                        {
                            if (performanceTweaksOn.TryGetValue(kvp.Key, out var performanceOn))
                            {
                                Log.Message(kvp.Key + " - performance on: " + performanceOn.Sum() + " - performance off: " + kvp.Value.Sum() + " === " + kvp.Value.Average() / performanceOn.Average());
                                result[kvp.Key] = kvp.Value.Average() / performanceOn.Average();
                            }
                        }

                        foreach (var r in result.OrderByDescending(x => x.Value))
                        {
                            Log.Message("Result: " + r.Key + " - " + r.Value);
                        }
                    }
                }
                profileOn = !profileOn;
                lastProfileCheckTick = Find.TickManager.ticksGameInt;
                Watcher.ResetData();
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
            Scribe_Values.Look(ref enabled, "enabled");
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
