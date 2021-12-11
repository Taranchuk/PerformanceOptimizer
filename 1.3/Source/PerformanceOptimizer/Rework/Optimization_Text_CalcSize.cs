using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_Text_CalcSize : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Cache;
        public override string Name => throw new NotImplementedException();
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(Text), "CalcSize", GetMethod(nameof(Prefix)));
        }

        public static Dictionary<string, Vector2> tinyCachedResults = new Dictionary<string, Vector2>();
        public static Dictionary<string, Vector2> smallCachedResults = new Dictionary<string, Vector2>();

        [HarmonyPriority(Priority.First)]
        public static bool Prefix(string text, ref Vector2 __result)
        {
            if (text != null)
            {
                if (Text.fontInt == GameFont.Tiny && Text.anchorInt == TextAnchor.UpperLeft && Text.wordWrapInt)
                {
                    var guiStyle = Text.CurFontStyle;
                    if (guiStyle.fontStyle == FontStyle.Normal && guiStyle.wordWrap && guiStyle.alignment == TextAnchor.UpperLeft)
                    {
                        if (!tinyCachedResults.TryGetValue(text, out __result))
                        {
                            Text.tmpTextGUIContent.text = text.StripTags();
                            tinyCachedResults[text] = __result = guiStyle.CalcSize(Text.tmpTextGUIContent);
                        }
                        return false;
                    }
                }
                else if (Text.fontInt == GameFont.Small && Text.anchorInt == TextAnchor.UpperLeft && Text.wordWrapInt)
                {
                    var guiStyle = Text.CurFontStyle;
                    if (guiStyle.fontStyle == FontStyle.Normal && guiStyle.wordWrap && guiStyle.alignment == TextAnchor.UpperLeft)
                    {
                        if (!smallCachedResults.TryGetValue(text, out __result))
                        {
                            Text.tmpTextGUIContent.text = text.StripTags();
                            smallCachedResults[text] = __result = guiStyle.CalcSize(Text.tmpTextGUIContent);
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
