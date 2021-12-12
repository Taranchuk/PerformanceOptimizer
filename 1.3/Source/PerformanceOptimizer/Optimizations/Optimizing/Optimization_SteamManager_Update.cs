using HarmonyLib;
using Verse;
using Verse.Steam;

namespace PerformanceOptimizer
{
    public class Optimization_SteamManager_Update : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Name => "PO.DisableSteamManagerCallbacksChecks".Translate();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(SteamManager), "Update", GetMethod(nameof(Prefix)));
        }

        [HarmonyPriority(Priority.First)]
        public static bool Prefix()
        {
            if (Current.programStateInt == ProgramState.Playing)
            {
                return false;
            }
            return true;
        }
    }
}

