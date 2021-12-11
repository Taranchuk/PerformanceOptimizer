using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PerformanceOptimizer
{
    public abstract class Optimization : IExposable
    {
        public List<MethodBase> originals;

        public List<MethodInfo> patches;
        public virtual bool EnabledByDefault => true;
        public abstract OptimizationType OptimizationType { get; }

        public bool enabled;
        public abstract string Name { get; }
        public virtual string Key => this.GetType().Name;
        public virtual void Initialize()
        {
            Reset();
        }

        public virtual void Reset()
        {
            enabled = EnabledByDefault;
        }
        public void Apply()
        {
            if (!enabled && patches != null && patches.Any())
            {
                UnPatchAll();
            }
            else if (enabled && (patches is null || !patches.Any()))
            {
                DoPatches();
            }
        }

        public virtual void DoPatches()
        {
            Log.Message("Patching it: " + GetType());
            originals ??= new List<MethodBase>();
            patches ??= new List<MethodInfo>();
        }

        public void Patch(Type type, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, MethodInfo transpiler = null)
        {
            var originalMethod = AccessTools.Method(type, methodName);
            var patch = PerformanceOptimizerMod.harmony.Patch(originalMethod, prefix != null ? new HarmonyMethod(prefix) : null, postfix != null ? new HarmonyMethod(postfix) : null, transpiler != null ? new HarmonyMethod(transpiler) : null);
            originals.Add(originalMethod);
            patches.Add(patch);
        }

        public void UnPatchAll()
        {
            Log.Message("Unpatching it: " + GetType());
            if (originals != null)
            {
                for (var i = 0; i < originals.Count; i++)
                {
                    var original = originals[i];
                    var patch = patches[i];
                    PerformanceOptimizerMod.harmony.Unpatch(original, patch);
                }
            }
            originals.Clear();
            patches.Clear();
        }
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled");
        }

        public MethodInfo GetMethod(string name) => AccessTools.Method(this.GetType(), name);
    }
}
