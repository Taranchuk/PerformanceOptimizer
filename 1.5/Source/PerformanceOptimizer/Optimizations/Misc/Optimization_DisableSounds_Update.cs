using HarmonyLib;
using System;
using UnityEngine;
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
            Patch(AccessTools.Method(typeof(MouseoverSounds), "DoRegion", new Type[] { typeof(Rect), typeof(SoundDef) }), GetMethod(nameof(Prefix)));
            Patch(AccessTools.Method(typeof(MouseoverSounds), "DoRegion", new Type[] { typeof(Rect) }), GetMethod(nameof(Prefix)));
            Patch(typeof(MouseoverSounds), "ResolveFrame", GetMethod(nameof(Prefix)));
        }
        public static bool Prefix()
        {
            return false;
        }
    }
}

