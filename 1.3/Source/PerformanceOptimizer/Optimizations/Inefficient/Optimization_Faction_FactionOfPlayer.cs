using HarmonyLib;
using RimWorld;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Faction_FactionOfPlayer : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Label => "PO.CacheFactionOfPlayer".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Faction), "get_OfPlayer", GetMethod(nameof(Prefix)));
        }
        public static Faction cachedResult;

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(ref Faction __result)
        {
            if (cachedResult != null)
            {
                __result = cachedResult;
                return false;
            }
            return true;
        }

        public override void Clear()
        {
            cachedResult = null;
            if (Find.World?.factionManager?.OfPlayer != null)
            {
                cachedResult = Find.World.factionManager.OfPlayer;
            }
        }
    }
}
