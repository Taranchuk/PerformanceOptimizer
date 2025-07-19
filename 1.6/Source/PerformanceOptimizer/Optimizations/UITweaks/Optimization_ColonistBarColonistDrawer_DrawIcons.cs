using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_ColonistBarColonistDrawer_DrawIcons : Optimization_RefreshRate
    {
        public static int refreshRateStatic;

        public struct CachedIcons
        {
            public int ticks;
            public List<ColonistBarColonistDrawer.IconDrawCall> Icons;
        }

        public static Dictionary<Pawn, CachedIcons> cachedResults = new();

        public override int RefreshRateByDefault => 30;
        public override OptimizationType OptimizationType => OptimizationType.CacheWithRefreshRate;
        public override string Label => "PO.CacheColonistBarIcons".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(AccessTools.Method(typeof(ColonistBarColonistDrawer), "DrawIcons"), GetMethod(nameof(Prefix)), GetMethod(nameof(Postfix)));
        }

        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(ColonistBarColonistDrawer __instance, Rect rect, Pawn colonist, out bool __state)
        {
            if (colonist.Dead)
            {
                __state = false;
            }
            else
            {
                __state = true;
                if (cachedResults.TryGetValue(colonist, out var cache))
                {
                    if (PerformanceOptimizerMod.tickManager.ticksGameInt < (cache.ticks + refreshRateStatic))
                    {
                        __state = false;
                        DrawCachedIcons(__instance, rect, cache.Icons);
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPriority(int.MinValue)]
        public static void Postfix(Pawn colonist, bool __state)
        {
            if (__state)
            {
                var newCacheEntry = new CachedIcons
                {
                    ticks = PerformanceOptimizerMod.tickManager.ticksGameInt,
                    Icons = new List<ColonistBarColonistDrawer.IconDrawCall>(ColonistBarColonistDrawer.tmpIconsToDraw)
                };
                cachedResults[colonist] = newCacheEntry;
            }
        }

        private static void DrawCachedIcons(ColonistBarColonistDrawer drawerInstance, Rect rect, List<ColonistBarColonistDrawer.IconDrawCall> iconsToDraw)
        {
            if (iconsToDraw.Count == 0)
            {
                return;
            }

            float iconSize = Mathf.Min(ColonistBarColonistDrawer.BaseIconAreaWidth / iconsToDraw.Count, ColonistBarColonistDrawer.BaseIconMaxSize) * Find.ColonistBar.Scale;
            Vector2 pos = new Vector2(rect.x + 1f, rect.yMax - iconSize - 1f);

            foreach (var item in iconsToDraw)
            {
                GUI.color = item.color ?? Color.white;
                drawerInstance.DrawIcon(item.texture, ref pos, iconSize, item.tooltip);
            }
            GUI.color = Color.white;
        }

        public override void Clear()
        {
            cachedResults.Clear();
        }
    }
}
