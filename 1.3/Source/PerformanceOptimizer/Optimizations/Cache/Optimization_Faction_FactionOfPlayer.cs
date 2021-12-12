using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Faction_FactionOfPlayer : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Name => "PO.CacheFactionOfPlayer".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Faction), "get_OfPlayer", GetMethod(nameof(Prefix)));
        }
        public static Faction cachedResult;

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
            
            if (cachedResult is null)
            {
                cachedResult = Find.FactionManager.OfPlayer;
            }
            __result = cachedResult;
            return false;
        }

        public override void Clear()
        {
            cachedResult = null;
        }
    }
}
