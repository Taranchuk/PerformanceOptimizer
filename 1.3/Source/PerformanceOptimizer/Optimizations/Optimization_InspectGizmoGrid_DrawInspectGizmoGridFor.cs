using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace PerformanceOptimizer
{
    public class Optimization_InspectGizmoGrid_DrawInspectGizmoGridFor : Optimization_RefreshRate
    {
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Name => "PO.InspectGizmoGrid".Translate();

        public override int RefreshRateByDefault => 30;

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(InspectGizmoGrid), "DrawInspectGizmoGridFor", transpiler: GetMethod(nameof(InspectGizmoGrid_DrawInspectGizmoGridForTranspiler)));

            var method = typeof(GizmoGridDrawer).GetMethods(AccessTools.all).FirstOrDefault(x => x.Name.Contains("<DrawGizmoGrid>") && x.Name.Contains("ProcessGizmoState"));
            Patch(method, transpiler: GetMethod(nameof(GizmoGridDrawer_ProcessGizmoStateTranspiler)));
        }

        public static Dictionary<ISelectable, CachedValueUpdate<List<Gizmo>>> cachedResults = new Dictionary<ISelectable, CachedValueUpdate<List<Gizmo>>>();
        public static IEnumerable<CodeInstruction> InspectGizmoGrid_DrawInspectGizmoGridForTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var method = AccessTools.Method(typeof(ISelectable), "GetGizmos");
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(method))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Optimization_InspectGizmoGrid_DrawInspectGizmoGridFor), nameof(GetGizmosFast))).MoveLabelsFrom(codes[i]);
                }
                else
                {
                    yield return codes[i];
                }
            }
        }

        public static List<Gizmo> GetGizmosFast(ISelectable selectable)
        {
            List<Gizmo> gizmos;
            if (!cachedResults.TryGetValue(selectable, out var cache))
            {
                gizmos = selectable.GetGizmos().ToList();
                cachedResults[selectable] = new CachedValueUpdate<List<Gizmo>>(gizmos, refreshRateStatic);
            }
            else if (Time.frameCount > cache.refreshUpdate)
            {
                gizmos = selectable.GetGizmos().ToList();
                cache.SetValue(gizmos, refreshRateStatic);
            }
            else
            {
                gizmos = cache.valueInt;
            }
            if (ModCompatUtility.AllowToolActive)
            {
                ModCompatUtility.ProcessAllowToolToggle(gizmos);
            }
            return gizmos;
        }

        public static IEnumerable<CodeInstruction> GizmoGridDrawer_ProcessGizmoStateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            var method = AccessTools.Method(typeof(ISelectable), "GetGizmos");
            bool found = false;
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];
                if (!found && codes[i].opcode == OpCodes.Stfld)
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Optimization_InspectGizmoGrid_DrawInspectGizmoGridFor), nameof(ResetSelectable)));
                }
            }
        }
        public static void ResetSelectable()
        {
            cachedResults.Clear();
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
