using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    class PerformanceOptimizerMod : Mod
    {
        public static Harmony harmony;

        public static PerformanceOptimizerSettings settings;

        public static TickManager tickManager;
        public PerformanceOptimizerMod(ModContentPack mod) : base(mod)
        {
            harmony = new Harmony("PerformanceOptimizer.Main");
            GameLoad_Patches.DoPatches();
            harmony.PatchAll();
            settings = GetSettings<PerformanceOptimizerSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
    }

    [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), "CallAll")]
    public static class StaticConstructorOnStartupUtilityCallAll
    {
        public static void Postfix()
        {
            CachingPatches.DoPatches();
            if (PerformanceOptimizerSettings.fasterGetCompReplacement)
            {
                GetCompPatches.DoPatchesAsync();
            }
        }
    }
}
