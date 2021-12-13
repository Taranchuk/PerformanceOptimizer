using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using Verse;
using Verse.Sound;

namespace PerformanceOptimizer
{
    public class Optimization_DisableSounds_Update : Optimization_Misc
    {
        public override bool EnabledByDefault => false;
        public override string Label => "PO.DisableSoundsCompletely".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(SoundRoot), "Update", GetMethod(nameof(Prefix)));
            Patch(typeof(SoundStarter), "PlayOneShotOnCamera", GetMethod(nameof(Prefix)));
            Patch(typeof(SoundStarter), "PlayOneShot", GetMethod(nameof(Prefix)));
        }
        public static bool Prefix()
        {
            return false;
        }
    }
}

