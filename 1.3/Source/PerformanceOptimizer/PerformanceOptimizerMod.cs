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
    class PerformanceOptimizerMod : Mod
    {
        public static Harmony harmony;
        public PerformanceOptimizerMod(ModContentPack pack) : base(pack)
        {
            harmony = new Harmony("PerformanceOptimizer.Patches");
            harmony.PatchAll();
            GetCompPatches.DoPatchesAsync();
        }
    }
}
