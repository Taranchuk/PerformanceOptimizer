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

        public bool enabled;
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
            if (!enabled && patchedMethods != null && patchedMethods.Any())
            {
                UnPatchAll();
            }
            else if (enabled && (patchedMethods is null || !patchedMethods.Any()))
            {
                patchedMethods ??= new Dictionary<MethodInfo, List<MethodInfo>>();
                DoPatches();
            }
            Watcher.Reset();
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
                if (PERFORMANCEPROFILE && prefix != null && prefix.ReturnType == typeof(bool))
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

        public static Dictionary<Type, float> performanceTweaksOn = new Dictionary<Type, float>();
        public static Dictionary<Type, float> performanceTweaksOff = new Dictionary<Type, float>();
        public static bool profileOn = true;
        public static int lastProfileCheckTick;
        public static Stopwatch stopwatch = new Stopwatch();
        public const bool PERFORMANCEPROFILE = false;
        public const int PROFILINGINTERVAL = 10000;
        public static bool MeasurePrefix(ref bool __result)
        {
            if (Current.gameInt?.tickManager != null && lastProfileCheckTick != Find.TickManager.ticksGameInt && Find.TickManager.ticksGameInt % PROFILINGINTERVAL == 0)
            {
                profileOn = !profileOn;
                lastProfileCheckTick = Find.TickManager.ticksGameInt;
                Watcher.Reset();
                if (profileOn)
                {
                    Log.Message("Profiling result: -------------------");
                    var result = new Dictionary<Type, float>();
                    foreach (var kvp in performanceTweaksOff.OrderByDescending(x => x.Value))
                    {
                        if (performanceTweaksOn.TryGetValue(kvp.Key, out var performanceOn))
                        {
                            Log.Message(kvp.Key + " - performance on: " + performanceOn + " - performance off: " + kvp.Value + " === " + kvp.Value / performanceOn);
                            result[kvp.Key] = kvp.Value / performanceOn;
                        }
                    }

                    foreach (var r in result.OrderByDescending(x => x.Value))
                    {
                        Log.Message("Result: " + r.Key + " - " + r.Value);
                    }
                }
            }
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
                    performanceTweaksOn[type] += elapsed;
                }
                else
                {
                    performanceTweaksOn[type] = elapsed;
                }
            }
            else
            {
                if (performanceTweaksOff.ContainsKey(type))
                {
                    performanceTweaksOff[type] += elapsed;
                }
                else
                {
                    performanceTweaksOff[type] = elapsed;
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
