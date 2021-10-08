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
using Verse;

namespace PerformanceOptimizer
{
    class PerformanceOptimizerMain : Mod
    {
        public static Harmony harmony;
        public PerformanceOptimizerMain(ModContentPack mod) : base(mod)
        {
            harmony = new Harmony("PerformanceOptimizer.Main");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), "CallAll")]
    public static class StaticConstructorOnStartupUtilityCallAll
    {
        public static void Postfix()
        {
            GetCompPatches.DoPatchesAsync();
        }
    }
}
