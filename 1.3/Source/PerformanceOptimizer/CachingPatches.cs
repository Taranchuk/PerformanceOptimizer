using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PerformanceOptimizer
{
    public static class CachingPatches
    {
        public static void DoPatches()
        {
            if (PerformanceOptimizerSettings.AmbientTemperatureCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(Thing), "get_AmbientTemperature"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_Thing_AmbientTemperature), nameof(Patch_Thing_AmbientTemperature.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_Thing_AmbientTemperature), nameof(Patch_Thing_AmbientTemperature.Postfix))));
            }
            if (PerformanceOptimizerSettings.GetStyleDominanceCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(IdeoUtility), "GetStyleDominance"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_IdeoUtility_GetStyleDominance), nameof(Patch_IdeoUtility_GetStyleDominance.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_IdeoUtility_GetStyleDominance), nameof(Patch_IdeoUtility_GetStyleDominance.Postfix))));
            }
            if (PerformanceOptimizerSettings.CurrentInstantBeautyCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(Need_Beauty), "CurrentInstantBeauty"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_Need_Beauty_CurrentInstantBeauty), nameof(Patch_Need_Beauty_CurrentInstantBeauty.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_Need_Beauty_CurrentInstantBeauty), nameof(Patch_Need_Beauty_CurrentInstantBeauty.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.BreakThresholdExtremeCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(MentalBreaker), "get_BreakThresholdExtreme"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_MentalBreaker_BreakThresholdExtreme), nameof(Patch_MentalBreaker_BreakThresholdExtreme.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_MentalBreaker_BreakThresholdExtreme), nameof(Patch_MentalBreaker_BreakThresholdExtreme.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.BreakThresholdMajorCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(MentalBreaker), "get_BreakThresholdMajor"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_MentalBreaker_BreakThresholdMajor), nameof(Patch_MentalBreaker_BreakThresholdMajor.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_MentalBreaker_BreakThresholdMajor), nameof(Patch_MentalBreaker_BreakThresholdMajor.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.BreakThresholdMinorCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(MentalBreaker), "get_BreakThresholdMinor"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_MentalBreaker_BreakThresholdMinor), nameof(Patch_MentalBreaker_BreakThresholdMinor.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_MentalBreaker_BreakThresholdMinor), nameof(Patch_MentalBreaker_BreakThresholdMinor.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.TotalMoodOffsetCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(ThoughtHandler), "TotalMoodOffset"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ThoughtHandler_TotalMoodOffset), nameof(Patch_ThoughtHandler_TotalMoodOffset.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ThoughtHandler_TotalMoodOffset), nameof(Patch_ThoughtHandler_TotalMoodOffset.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.IsQuestLodgerCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(QuestUtility), "IsQuestLodger"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_QuestUtility_IsQuestLodger), nameof(Patch_QuestUtility_IsQuestLodger.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_QuestUtility_IsQuestLodger), nameof(Patch_QuestUtility_IsQuestLodger.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.IsTeetotalerCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(PawnUtility), "IsTeetotaler"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_PawnUtility_IsTeetotaler), nameof(Patch_PawnUtility_IsTeetotaler.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_PawnUtility_IsTeetotaler), nameof(Patch_PawnUtility_IsTeetotaler.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.CurrentExpectationForPawnCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(ExpectationsUtility), "CurrentExpectationFor", new Type[] { typeof(Pawn) }),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ExpectationsUtility_CurrentExpectationForPawn), nameof(Patch_ExpectationsUtility_CurrentExpectationForPawn.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ExpectationsUtility_CurrentExpectationForPawn), nameof(Patch_ExpectationsUtility_CurrentExpectationForPawn.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.CurrentExpectationForMapCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(ExpectationsUtility), "CurrentExpectationFor", new Type[] { typeof(Map) }),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ExpectationsUtility_CurrentExpectationFor_Map), nameof(Patch_ExpectationsUtility_CurrentExpectationFor_Map.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_ExpectationsUtility_CurrentExpectationFor_Map), nameof(Patch_ExpectationsUtility_CurrentExpectationFor_Map.Postfix))));
            }
            
            PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(PawnNeedsUIUtility), "GetThoughtGroupsInDisplayOrder"),
            new HarmonyMethod(AccessTools.Method(typeof(Patch_PawnNeedsUIUtility_GetThoughtGroupsInDisplayOrder), nameof(Patch_PawnNeedsUIUtility_GetThoughtGroupsInDisplayOrder.Prefix))),
            new HarmonyMethod(AccessTools.Method(typeof(Patch_PawnNeedsUIUtility_GetThoughtGroupsInDisplayOrder), nameof(Patch_PawnNeedsUIUtility_GetThoughtGroupsInDisplayOrder.Postfix))));
            
            if (PerformanceOptimizerSettings.FindAllowedDesignatorCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(BuildCopyCommandUtility), "FindAllowedDesignator"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_BuildCopyCommandUtility_FindAllowedDesignator), nameof(Patch_BuildCopyCommandUtility_FindAllowedDesignator.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_BuildCopyCommandUtility_FindAllowedDesignator), nameof(Patch_BuildCopyCommandUtility_FindAllowedDesignator.Postfix))));
            }
            
            if (PerformanceOptimizerSettings.CheckCurrentToilEndOrFailThrottleActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(JobDriver), "CheckCurrentToilEndOrFail"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_JobDriver_CheckCurrentToilEndOrFail), nameof(Patch_JobDriver_CheckCurrentToilEndOrFail.Prefix))));
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(Pawn_PathFollower), "StartPath"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patch_Pawn_PathFollower_StartPath), nameof(Patch_Pawn_PathFollower_StartPath.Transpiler))));
            }
            
            if (PerformanceOptimizerSettings.CacheFactionOfPlayer)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(Faction), "get_OfPlayer"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_Faction_FactionOfPlayer), nameof(Patch_Faction_FactionOfPlayer.Prefix))));
            }

            if (PerformanceOptimizerSettings.CacheStatWorker_MarketValue)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(StatWorker_MarketValue), "CalculableRecipe"),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_StatWorker_MarketValue_CalculableRecipe), nameof(Patch_StatWorker_MarketValue_CalculableRecipe.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(Patch_StatWorker_MarketValue_CalculableRecipe), nameof(Patch_StatWorker_MarketValue_CalculableRecipe.Postfix))));
            }

            if (PerformanceOptimizerSettings.PawnCollisionPosOffsetForCacheActive)
            {
                PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(PawnCollisionTweenerUtility), "PawnCollisionPosOffsetFor"),
                    new HarmonyMethod(AccessTools.Method(typeof(PawnCollisionPosOffsetFor), nameof(PawnCollisionPosOffsetFor.Prefix))),
                    new HarmonyMethod(AccessTools.Method(typeof(PawnCollisionPosOffsetFor), nameof(PawnCollisionPosOffsetFor.Postfix))));
            }
        }
    }

    public struct Data
    {
        public int key;
        public bool state;
    }

    public static class PawnCollisionPosOffsetFor
    {
        public static Dictionary<Pawn, CachedValue<Vector3>> cachedResults = new Dictionary<Pawn, CachedValue<Vector3>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn, out bool __state, ref Vector3 __result)
        {
            if (!cachedResults.TryGetValue(pawn, out var cache))
            {
                cachedResults[pawn] = new CachedValue<Vector3>(default, PerformanceOptimizerSettings.PawnCollisionPosOffsetForRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn pawn, bool __state, ref Vector3 __result)
        {
            if (__state)
            {
                cachedResults[pawn].SetValue(__result, PerformanceOptimizerSettings.PawnCollisionPosOffsetForRefreshRate);
            }
        }
    }

    public static class Patch_Faction_FactionOfPlayer
    {
        public static Faction factionOfPlayer;
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref Faction __result)
        {
            if (Current.programStateInt != ProgramState.Playing)
            {
                GameInitData gameInitData = Find.GameInitData;
                if (gameInitData != null && gameInitData.playerFaction != null)
                {
                    __result = gameInitData.playerFaction;
                    return false;
                }
            }

            if (factionOfPlayer is null)
            {
                factionOfPlayer = Find.FactionManager.OfPlayer;
            }
            __result = factionOfPlayer;
            return false;
        }
    }
    public static class Patch_StatWorker_MarketValue_CalculableRecipe
    {
        public static Dictionary<BuildableDef, RecipeDef> cachedResults = new Dictionary<BuildableDef, RecipeDef>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(BuildableDef def, out bool __state, ref RecipeDef __result)
        {
            if (!cachedResults.TryGetValue(def, out var cache))
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache;
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(BuildableDef def, bool __state, RecipeDef __result)
        {
            if (__state)
            {
                cachedResults[def] = __result;
            }
        }
    }

    public static class Patch_BuildCopyCommandUtility_FindAllowedDesignator
    {
        public static Dictionary<BuildableDef, CachedValue<Designator_Build>> cachedResults = new Dictionary<BuildableDef, CachedValue<Designator_Build>>();
        [HarmonyPriority(Priority.First)]
        public static bool Prefix(BuildableDef buildable, out bool __state, ref Designator_Build __result)
        {
            if (!cachedResults.TryGetValue(buildable, out var cache))
            {
                cachedResults[buildable] = new CachedValue<Designator_Build>(default, PerformanceOptimizerSettings.FindAllowedDesignatorRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(BuildableDef buildable, bool __state, Designator_Build __result)
        {
            if (__state)
            {
                cachedResults[buildable].SetValue(__result, PerformanceOptimizerSettings.FindAllowedDesignatorRefreshRate);
            }
        }
    }

    public static class Patch_Thing_AmbientTemperature
    {
        public static Dictionary<Thing, CachedValue<float>> cachedResults = new Dictionary<Thing, CachedValue<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Thing __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance, out var cache))
            {
                cachedResults[__instance] = new CachedValue<float>(0, PerformanceOptimizerSettings.AmbientTemperatureRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Thing __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance].SetValue(__result, PerformanceOptimizerSettings.AmbientTemperatureRefreshRate);
            }
        }
    }
    public static class Patch_IdeoUtility_GetStyleDominance
    {
        public static Dictionary<int, CachedValue<float>> cachedResults = new Dictionary<int, CachedValue<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Thing t, Ideo ideo, out Data __state, ref float __result)
        {
            var key = t.GetHashCode() + ideo.GetHashCode();
            if (!cachedResults.TryGetValue(key, out var cache))
            {
                cachedResults[key] = new CachedValue<float>(0, PerformanceOptimizerSettings.GetStyleDominanceRefreshRate);
                __state = new Data { key = key, state = true };
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = new Data { key = key, state = true };
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = new Data { key = key, state = false };
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Data __state, float __result)
        {
            if (__state.state)
            {
                cachedResults[__state.key].SetValue(__result, PerformanceOptimizerSettings.GetStyleDominanceRefreshRate);
            }
        }
    }
    public static class Patch_Need_Beauty_CurrentInstantBeauty
    {
        public static Dictionary<Need_Beauty, CachedValue<float>> cachedResults = new Dictionary<Need_Beauty, CachedValue<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Need_Beauty __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance, out var cache))
            {
                cachedResults[__instance] = new CachedValue<float>(0, PerformanceOptimizerSettings.CurrentInstantBeautyRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Need_Beauty __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance].SetValue(__result, PerformanceOptimizerSettings.CurrentInstantBeautyRefreshRate);
            }
        }
    }
    public static class Patch_MentalBreaker_BreakThresholdExtreme
    {
        public static Dictionary<Pawn, CachedValue<float>> cachedResults = new Dictionary<Pawn, CachedValue<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(MentalBreaker __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new CachedValue<float>(0, PerformanceOptimizerSettings.BreakThresholdExtremeRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(MentalBreaker __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].SetValue(__result, PerformanceOptimizerSettings.BreakThresholdExtremeRefreshRate);
            }
        }
    }
    public static class Patch_MentalBreaker_BreakThresholdMajor
    {
        public static Dictionary<Pawn, CachedValue<float>> cachedResults = new Dictionary<Pawn, CachedValue<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(MentalBreaker __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new CachedValue<float>(0, PerformanceOptimizerSettings.BreakThresholdMajorRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(MentalBreaker __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].SetValue(__result, PerformanceOptimizerSettings.BreakThresholdMajorRefreshRate);
            }
        }
    }
    public static class Patch_MentalBreaker_BreakThresholdMinor
    {
        public static Dictionary<Pawn, CachedValue<float>> cachedResults = new Dictionary<Pawn, CachedValue<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(MentalBreaker __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new CachedValue<float>(0, PerformanceOptimizerSettings.BreakThresholdMinorRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(MentalBreaker __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].SetValue(__result, PerformanceOptimizerSettings.BreakThresholdMinorRefreshRate);
            }
        }
    }
    public static class Patch_ThoughtHandler_TotalMoodOffset
    {
        public static Dictionary<Pawn, CachedValue<float>> cachedResults = new Dictionary<Pawn, CachedValue<float>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ThoughtHandler __instance, out bool __state, ref float __result)
        {
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache))
            {
                cachedResults[__instance.pawn] = new CachedValue<float>(0, PerformanceOptimizerSettings.TotalMoodOffsetRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ThoughtHandler __instance, bool __state, float __result)
        {
            if (__state)
            {
                cachedResults[__instance.pawn].SetValue(__result, PerformanceOptimizerSettings.TotalMoodOffsetRefreshRate);
            }
        }
    }

    public static class Patch_PawnUtility_IsTeetotaler
    {
        public static Dictionary<Pawn, CachedValue<bool>> cachedResults = new Dictionary<Pawn, CachedValue<bool>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn pawn, out bool __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(pawn, out var cache))
            {
                cachedResults[pawn] = new CachedValue<bool>(false, PerformanceOptimizerSettings.IsTeetotalerRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn pawn, bool __state, bool __result)
        {
            if (__state)
            {
                cachedResults[pawn].SetValue(__result, PerformanceOptimizerSettings.IsTeetotalerRefreshRate);
            }
        }
    }

    public static class Patch_QuestUtility_IsQuestLodger
    {
        public static Dictionary<Pawn, CachedValue<bool>> cachedResults = new Dictionary<Pawn, CachedValue<bool>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn p, out bool __state, ref bool __result)
        {
            if (!cachedResults.TryGetValue(p, out var cache))
            {
                cachedResults[p] = new CachedValue<bool>(false, PerformanceOptimizerSettings.IsQuestLodgerRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn p, bool __state, bool __result)
        {
            if (__state)
            {
                cachedResults[p].SetValue(__result, PerformanceOptimizerSettings.IsQuestLodgerRefreshRate);
            }
        }
    }
    public static class Patch_ExpectationsUtility_CurrentExpectationForPawn
    {
        public static Dictionary<Pawn, CachedValue<ExpectationDef>> cachedResults = new Dictionary<Pawn, CachedValue<ExpectationDef>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Pawn p, out bool __state, ref ExpectationDef __result)
        {
            if (!cachedResults.TryGetValue(p, out var cache))
            {
                cachedResults[p] = new CachedValue<ExpectationDef>(null, PerformanceOptimizerSettings.CurrentExpectationForPawnRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Pawn p, bool __state, ExpectationDef __result)
        {
            if (__state)
            {
                cachedResults[p].SetValue(__result, PerformanceOptimizerSettings.CurrentExpectationForPawnRefreshRate);
            }
        }
    }
    public static class Patch_ExpectationsUtility_CurrentExpectationFor_Map
    {
        public static Dictionary<Map, CachedValue<ExpectationDef>> cachedResults = new Dictionary<Map, CachedValue<ExpectationDef>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Map m, out bool __state, ref ExpectationDef __result)
        {
            if (!cachedResults.TryGetValue(m, out var cache))
            {
                cachedResults[m] = new CachedValue<ExpectationDef>(null, PerformanceOptimizerSettings.CurrentExpectationForMapRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                __result = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Map m, bool __state, ExpectationDef __result)
        {
            if (__state)
            {
                cachedResults[m].SetValue(__result, PerformanceOptimizerSettings.CurrentExpectationForMapRefreshRate);
            }
        }
    }
    public static class Patch_PawnNeedsUIUtility_GetThoughtGroupsInDisplayOrder
    {
        public static Dictionary<Need_Mood, CachedValue<List<Thought>>> cachedResults = new Dictionary<Need_Mood, CachedValue<List<Thought>>>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(Need_Mood mood, ref List<Thought> outThoughtGroupsPresent, out bool __state)
        {
            if (!cachedResults.TryGetValue(mood, out var cache))
            {
                cachedResults[mood] = new CachedValue<List<Thought>>(new List<Thought>(), PerformanceOptimizerSettings.GetThoughtGroupsInDisplayOrderRefreshRate);
                __state = true;
                return true;
            }
            else if (PerformanceOptimizerMod.tickManager.ticksGameInt > cache.refreshTick)
            {
                __state = true;
                return true;
            }
            else
            {
                outThoughtGroupsPresent = cache.GetValue();
                __state = false;
                return false;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Need_Mood mood, ref List<Thought> outThoughtGroupsPresent, bool __state)
        {
            if (__state)
            {
                cachedResults[mood].SetValue(outThoughtGroupsPresent, PerformanceOptimizerSettings.GetThoughtGroupsInDisplayOrderRefreshRate);
            }
        }
    }
    public static class Patch_JobDriver_CheckCurrentToilEndOrFail
    {
        public static Dictionary<Pawn, int> cachedResults = new Dictionary<Pawn, int>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(JobDriver __instance)
        {
            if (__instance.pawn.pather.moving || __instance is JobDriver_OperateScanner)
            {
                return true;
            }
            if (!cachedResults.TryGetValue(__instance.pawn, out var cache) || PerformanceOptimizerMod.tickManager.ticksGameInt > cache + PerformanceOptimizerSettings.CheckCurrentToilEndOrFailThrottleRate)
            {
                cachedResults[__instance.pawn] = PerformanceOptimizerMod.tickManager.ticksGameInt;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public static class Patch_Pawn_PathFollower_StartPath
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (i + 2 < codes.Count && codes[i + 2].OperandIs(" pathing to destroyed thing "))
                {
                    i += 7; // we skip 				Log.Error(string.Concat(pawn, " pathing to destroyed thing ", dest.Thing));
                }
                yield return codes[i];
            }
        }
    }
}
