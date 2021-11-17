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

    public class ValueCache<T>
    {
        public int refreshTick;
        public T valueInt;
        public ValueCache(T value, int resetInTicks)
        {
            SetValue(value, resetInTicks);
        }

        public void SetValue(T value, int resetInTicks)
        {
            this.valueInt = value;
            refreshTick = Find.TickManager.TicksGame + resetInTicks;
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
                cachedResults[__instance.pawn] = new ValueCache<float>(0, 30);
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
                cachedResults[__instance.pawn].SetValue(__result, 30);
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
                cachedResults[__instance.pawn] = new ValueCache<float>(0, 30);
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
                cachedResults[__instance.pawn].SetValue(__result, 30);
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
                cachedResults[__instance.pawn] = new ValueCache<float>(0, 30);
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
                cachedResults[__instance.pawn].SetValue(__result, 30);
            }
        }
    }
    
    [HarmonyPatch(typeof(ThoughtHandler), "TotalMoodOffset")]
    public static class Patch_ThoughtHandler_TotalMoodOffset
    {
        private const int RefreshRate = 300;
        private static Dictionary<Pawn, ValueCache<float>> cachedResults = new Dictionary<Pawn, ValueCache<float>>();
    
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ThoughtHandler __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new ValueCache<float>(0, RefreshRate);
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
                cachedResults[__instance.pawn].SetValue(__result, RefreshRate);
            }
        }
    }

    [HarmonyPatch(typeof(BeautyUtility), "AverageBeautyPerceptible")]
    public static class Patch_BeautyUtility_AverageBeautyPerceptible
    {
        private const int RefreshRate = 300;
        private static Dictionary<int, ValueCache<float>> cachedResults = new Dictionary<int, ValueCache<float>>();
        public struct Data
        {
            public bool state;
            public int key;
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(IntVec3 root, Map map, out Data __state, ref float __result)
        {
            var key = root.GetHashCode() + map.uniqueID;
            if (!cachedResults.TryGetValue(key, out var cache))
            {
                cachedResults[key] = new ValueCache<float>(0, RefreshRate);
                __state = new Data { state = true, key = key };
                return true;
            }
            else if (Find.TickManager.ticksGameInt > cache.refreshTick)
            {
                __state = new Data { state = true, key = key };
                return true;
            }
            else
            {
                __result = cache.valueInt;
                __state = new Data { state = false, key = key};
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Data __state, float __result)
        {
            if (__state.state)
            {
                cachedResults[__state.key].SetValue(__result, RefreshRate);
            }
        }
    }

    [HarmonyPatch(typeof(QuestUtility), "IsQuestLodger")]
    public static class Patch_QuestUtility_IsQuestLodger
    {
        private const int RefreshRate = 60;
        private static Dictionary<Pawn, ValueCache<bool>> cachedResults = new Dictionary<Pawn, ValueCache<bool>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn p, out bool __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(p, out var cache))
            {
                cachedResults[p] = new ValueCache<bool>(false, RefreshRate);
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
        public static void Postfix(Pawn p, bool __state, bool __result)
        {
            if (__state)
            {
                cachedResults[p].SetValue(__result, RefreshRate);
            }
        }
    }

    [HarmonyPatch(typeof(PawnNeedsUIUtility), "GetThoughtGroupsInDisplayOrder")]
    public static class Patch_PawnNeedsUIUtility_GetThoughtGroupsInDisplayOrder
    {
        private const int RefreshRate = 200;
        private static Dictionary<Need_Mood, ValueCache<List<Thought>>> cachedResults = new Dictionary<Need_Mood, ValueCache<List<Thought>>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Need_Mood mood, ref List<Thought> outThoughtGroupsPresent, out bool __state)
        {
            if (!cachedResults.TryGetValue(mood, out var cache))
            {
                cachedResults[mood] = new ValueCache<List<Thought>>(new List<Thought>(), RefreshRate);
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
                outThoughtGroupsPresent = cache.valueInt;
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Need_Mood mood, ref List<Thought> outThoughtGroupsPresent, bool __state)
        {
            if (__state)
            {
                cachedResults[mood].SetValue(outThoughtGroupsPresent, RefreshRate);
            }
        }
    }


    //[HarmonyPatch(typeof(JobDriver), "CheckCurrentToilEndOrFail")] // TODO: produces job errors, investigate further
    //public static class Patch_JobDriver_CheckCurrentToilEndOrFail
    //{
    //    private const int RefreshRate = 30;
    //    private static Dictionary<Pawn, int> cachedResults = new Dictionary<Pawn, int>();
    //
    //    [HarmonyPriority(Priority.First)]
    //    public static bool Prefix(JobDriver __instance)
    //    {
    //        if (!cachedResults.TryGetValue(__instance.pawn, out var cache) || Find.TickManager.ticksGameInt > cache + RefreshRate)
    //        {
    //            cachedResults[__instance.pawn] = Find.TickManager.ticksGameInt;
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(WindManager), "WindManagerTick")]
    public static class Patch_WindManager_WindManagerTick
    {
        [HarmonyPriority(Priority.First)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
        {
            var codes = instructions.ToList();
            var plantSwayHead = AccessTools.Field(typeof(WindManager), nameof(WindManager.plantSwayHead));
            for (var i = 0; i < codes.Count; i++)
            {
                if (i > 2 && codes[i - 1].opcode == OpCodes.Stfld && codes[i - 1].OperandIs(plantSwayHead) && codes[i-2].OperandIs(0.0f) && codes[i - 2].opcode == OpCodes.Ldc_R4)
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_WindManager_WindManagerTick), nameof(ShouldDo))).MoveLabelsFrom(codes[i]);
                    var label = iLGenerator.DefineLabel();
                    yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    yield return new CodeInstruction(OpCodes.Ret);
                    codes[i].labels.Add(label);
                }
                yield return codes[i];
            }
        }

        public static bool ShouldDo()
        {
            if (Prefs.PlantWindSway)
            {
                return true;
            }
            return false;
        }
    }
}
