using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_MindState : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.MindState".Translate();
        public override bool ProfilePerformanceImpact => true;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(AccessTools.Method(typeof(GenLocalDate), "DayTick", new Type[] {typeof(Thing)}), 
                prefix: GetMethod(nameof(DayTickPrefix)), postfix: GetMethod(nameof(DayTickPostfix)));
        }

        private static void DayTickPrefix()
        {

        }

        private static void DayTickPostfix()
        {
            //Log.Message(thing + " - " + __result);
        }

        public struct NearbyEnemiesCache
        {
            public int lastTick;

            public bool enemiesNeaby;
        }

        //[HarmonyPatch(typeof(PawnUtility), "EnemiesAreNearby", new Type[]
        //{
        //        typeof(Pawn),
        //        typeof(int),
        //        typeof(bool)
        //})]
        private static class EnemiesNearbyTick
        {
            private static bool Prefix(ref Pawn pawn, ref int regionsToScan, ref bool passDoors, ref bool __result)
            {
                if (pawn.Faction == null)
                {
                    __result = pawn.mindState.mentalStateHandler.CurState != null;
                    return false;
                }
                if (!NearbyEnemiesDataCache.ContainsKey(pawn.Faction.loadID))
                {
                    InitCache();
                }
                NearbyEnemiesCache nearbyEnemiesCache = NearbyEnemiesDataCache[pawn.Faction.loadID];
                if (PerformanceOptimizerMod.tickManager.ticksGameInt > nearbyEnemiesCache.lastTick)
                {
                    regionsToScan = 200;
                    passDoors = true;
                    return true;
                }
                __result = nearbyEnemiesCache.enemiesNeaby;
                regionsToScan = 256;
                return false;
            }

            private static void Postfix(ref Pawn pawn, ref int regionsToScan, ref bool passDoors, ref bool __result)
            {
                if (pawn.Faction != null)
                {
                    NearbyEnemiesCache value = NearbyEnemiesDataCache[pawn.Faction.loadID];
                    value.lastTick = PerformanceOptimizerMod.tickManager.ticksGameInt + 200;
                    value.enemiesNeaby = __result;
                    NearbyEnemiesDataCache[pawn.Faction.loadID] = value;
                }
            }
        }

        public static Dictionary<int, NearbyEnemiesCache> NearbyEnemiesDataCache;

        public static void InitCache()
        {
            NearbyEnemiesDataCache = new Dictionary<int, NearbyEnemiesCache>(Find.FactionManager.AllFactions.EnumerableCount());
            NearbyEnemiesCache value = default(NearbyEnemiesCache);
            foreach (Faction allFaction in Find.FactionManager.AllFactions)
            {
                value.lastTick = 0;
                value.enemiesNeaby = false;
                NearbyEnemiesDataCache[allFaction.loadID] = value;
            }
        }
    }
}
