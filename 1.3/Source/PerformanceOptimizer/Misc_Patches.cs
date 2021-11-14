using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using Verse.Steam;

namespace PerformanceOptimizer
{

    [HarmonyPatch(typeof(SteamManager))]
    [HarmonyPatch("Update")]
    public static class Patch_SteamManager_Update
    {
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ResourceReadout __instance)
        {
            return false;
        }
    }

    public class ValueCache<T> where T : struct
    {
        public int refreshTick;
        public T valueInt;
        public ValueCache(T value)
        {
            Value = value;
        }

        public T Value
        {
            get
            {
                return valueInt;
            }
            set
            {
                this.valueInt = value;
                refreshTick = Find.TickManager.TicksGame + 30;
            }
        }
    }

    [HarmonyPatch(typeof(MentalBreaker), "BreakThresholdExtreme", MethodType.Getter)]
    public static class Patch_MentalBreaker_BreakThresholdExtreme
    {
        private static Dictionary<Pawn, ValueCache<float>> cachedResults = new Dictionary<Pawn, ValueCache<float>>();
    
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(MentalBreaker __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new ValueCache<float>(0);
                __state = true;
                return true;
            }
            else if (Find.TickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.valueInt;
                __state = false;
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(MentalBreaker __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].Value = __result;
            }
        }
    }
    
    [HarmonyPatch(typeof(MentalBreaker), "BreakThresholdMajor", MethodType.Getter)]
    public static class Patch_MentalBreaker_BreakThresholdMajor
    {
        private static Dictionary<Pawn, ValueCache<float>> cachedResults = new Dictionary<Pawn, ValueCache<float>>();
    
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(MentalBreaker __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new ValueCache<float>(0);
                __state = true;
                return true;
            }
            else if (Find.TickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.valueInt;
                __state = false;
                return false;
            }
        }
    
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(MentalBreaker __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].Value = __result;
            }
        }
    }
    
    [HarmonyPatch(typeof(MentalBreaker), "BreakThresholdMinor", MethodType.Getter)]
    public static class Patch_MentalBreaker_BreakThresholdMinor
    {
        private static Dictionary<Pawn, ValueCache<float>> cachedResults = new Dictionary<Pawn, ValueCache<float>>();
    
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(MentalBreaker __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new ValueCache<float>(0);
                __state = true;
                return true;
            }
            else if (Find.TickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.valueInt;
                __state = false;
                return false;
            }
        }
    
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(MentalBreaker __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].Value = __result;
            }
        }
    }
    
    [HarmonyPatch(typeof(ThoughtHandler), "TotalMoodOffset")]
    public static class Patch_ThoughtHandler_TotalMoodOffset
    {
        private static Dictionary<Pawn, ValueCache<float>> cachedResults = new Dictionary<Pawn, ValueCache<float>>();
    
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ThoughtHandler __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new ValueCache<float>(0);
                __state = true;
                return true;
            }
            else if (Find.TickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.valueInt;
                __state = false;
                return false;
            }
        }
    
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ThoughtHandler __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].Value = __result;
            }
        }
    }
}
