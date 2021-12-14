using HarmonyLib;
using System;
using System.Collections.Generic;
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

        public void Patch(MethodInfo methodInfo, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            PerformanceOptimizerMod.harmony.Patch(methodInfo, prefix != null ? new HarmonyMethod(prefix) : null, postfix != null ? new HarmonyMethod(postfix) : null, transpiler != null ? new HarmonyMethod(transpiler) : null);
            //Log.Message(this.GetType() +  " - Patching " + methodInfo.FullDescription());
            //Log.ResetMessageCount();
            List<MethodInfo> patches = new List<MethodInfo>();
            if (prefix != null) patches.Add(prefix);
            if (postfix != null) patches.Add(postfix);
            if (transpiler != null) patches.Add(transpiler);
            patchedMethods[methodInfo] = patches;
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
