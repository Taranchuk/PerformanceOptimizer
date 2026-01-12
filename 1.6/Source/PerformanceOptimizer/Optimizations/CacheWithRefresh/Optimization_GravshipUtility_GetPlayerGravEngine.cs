using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_GravshipUtility_GetPlayerGravEngine : Optimization_RefreshRate
    {
        public static Dictionary<int, CachedObjectTick<Building_GravEngine>> cachedResults = new();

        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.GetPlayerGravEngine".Translate();
        public override int RefreshRateByDefault => GenTicks.TicksPerRealSecond;
        // Don't allow for very rare throttling to try and avoid potential issues (like grav engine quests triggering while player has one already).
        public override int MaxSliderValue => GenTicks.TicksPerRealSecond * 5;
        public static int refreshRateStatic;

        public override void DoPatches()
        {
            base.DoPatches();

            // Only relevant if Odyssey is active. Method returns early if it's not active, so we don't need to worry patching it.
            if (!ModsConfig.OdysseyActive)
                return;

            // The issue in vanilla is that the vanilla cache is only applied to a singular map.
            // In case several maps are present, the game will cache which map the engine is on...
            // And then proceed to clear that cache when trying to check a different map on the same tick.
            // The fix here is to keep cache for all the active maps in the game.
            // This by itself isn't good enough if we recache it each tick - around 50-75% improvement,
            // which is still pretty slow. However, keeping the cache for a little while is much more efficient.

            Patch(typeof(GravshipUtility).DeclaredMethod(nameof(GravshipUtility.GetPlayerGravEngine_NewTemp)), prefix: GetMethod(nameof(Prefix)), postfix: GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Map map, out CachedObjectTick<Building_GravEngine> __state, ref Building_GravEngine __result)
        {
            if (!cachedResults.TryGetValue(map.uniqueID, out __state))
            {
                cachedResults[map.uniqueID] = __state = new CachedObjectTick<Building_GravEngine>();
                return true;
            }

            if (__state.SetOrRefresh(ref __result))
                return true;

            // Make sure the grav engine didn't change its map/was destroyed. Force recache if it did.
            if (__result == null || __result.MapHeld == map || !__result.Destroyed)
                return false;

            __state.isRefreshRequired = true;
            return true;
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(CachedObjectTick<Building_GravEngine> __state, ref Building_GravEngine __result)
        {
            __state.ProcessResult(ref __result, refreshRateStatic);
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
