﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Verse.Widgets;

namespace PerformanceOptimizer
{
    internal static class SettingsHelper
    {
        public static void CheckboxLabeled(this Listing_Standard ls, string label, ref bool checkOn, string tooltip = null, Action actionOnClick = null)
        {
            float lineHeight = Text.LineHeight;
            Rect rect = ls.GetRect(lineHeight);
            if (!ls.BoundingRectCached.HasValue || rect.Overlaps(ls.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                CheckboxLabeled(rect, label, ref checkOn, actionOnClick: actionOnClick);
            }
            ls.Gap(ls.verticalSpacing);
        }

        public static void CheckboxLabeled(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null,
            Texture2D texUnchecked = null, bool placeCheckboxNearText = false, Action actionOnClick = null)
        {
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            if (placeCheckboxNearText)
            {
                rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f);
            }
            Label(rect, label);
            if (!disabled && ButtonInvisible(rect))
            {
                checkOn = !checkOn;
                if (checkOn)
                {
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else
                {
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
                if (actionOnClick != null)
                {
                    actionOnClick();
                }
            }
            CheckboxDraw(rect.x + rect.width - 24f, rect.y, checkOn, disabled);
            Text.Anchor = anchor;
        }

        public static void CheckboxLabeledWithSlider(this Listing_Standard ls, string label, string sliderLabelKey, ref bool checkOn, ref int value, int maxSliderValue = 2500,
            string tooltip = null, Action actionOnClick = null, Action actionOnSlider = null)
        {
            float lineHeight = Text.LineHeight;
            Rect rect = ls.GetRect(lineHeight);

            if (!ls.BoundingRectCached.HasValue || rect.Overlaps(ls.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                TextAnchor anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleLeft;
                var labelRect = rect;
                labelRect.width /= 2;
                var checkboxRect = rect;
                checkboxRect.x = rect.x + rect.width - 24f;
                checkboxRect.width = 24f;
                Label(labelRect, label);

                var sliderLabelRect = rect;
                sliderLabelRect.x = labelRect.xMax;
                sliderLabelRect.width = 140;
                var sliderRect = rect;
                sliderRect.y += 5;
                sliderRect.x = sliderLabelRect.xMax + 10;
                sliderRect.width = rect.width - (labelRect.width + checkboxRect.width) - 160;
                if (checkOn)
                {
                    Widgets.Label(sliderLabelRect, sliderLabelKey.Translate(value.TicksToSeconds().ToString("F1") + "s"));
                    var oldValue = value;
                    value = (int)Widgets.HorizontalSlider(sliderRect, value, 0, maxSliderValue, false);
                    if (oldValue != value)
                    {
                        if (actionOnSlider != null)
                        {
                            actionOnSlider();
                        }
                    }
                }
                if (ButtonInvisible(checkboxRect))
                {
                    checkOn = !checkOn;
                    if (checkOn)
                    {
                        SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                    }
                    else
                    {
                        SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                    }
                    if (actionOnClick != null)
                    {
                        actionOnClick();
                    }
                }
                CheckboxDraw(rect.x + rect.width - 24f, rect.y, checkOn, false);
                Text.Anchor = anchor;
            }
            ls.Gap(ls.verticalSpacing);
        }
        // Token: 0x06000027 RID: 39 RVA: 0x00003754 File Offset: 0x00001954
        public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format, float min = 0f, float max = 100f, string tooltip = null)
        {
            float num = val;
            ls.SliderLabeled(label, ref num, format, min, max, tooltip);
            val = (int)num;
        }

        // Token: 0x06000028 RID: 40 RVA: 0x0000377C File Offset: 0x0000197C
        public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format, float min = 0f, float max = 1f, string tooltip = null)
        {
            Rect rect = ls.GetRect(Text.LineHeight);
            Rect rect2 = GenUI.Rounded(GenUI.LeftPart(rect, 0.7f));
            Rect rect3 = GenUI.Rounded(GenUI.LeftPart(GenUI.Rounded(GenUI.RightPart(rect, 0.3f)), 0.67f));
            Rect rect4 = GenUI.Rounded(GenUI.RightPart(rect, 0.1f));
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect2, label);
            float num = Widgets.HorizontalSlider(rect3, val, min, max, true, null, null, null, -1f);
            val = num;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect4, string.Format(format, val));
            if (!GenText.NullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            Text.Anchor = anchor;
            ls.Gap(ls.verticalSpacing);
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00003844 File Offset: 0x00001A44
        public static void IntRange(this Listing_Standard ls, string label, ref IntRange range, int min = 0, int max = 1, string tooltip = null, ToStringStyle valueStyle = ToStringStyle.Integer)
        {
            Rect rect = ls.GetRect(Text.LineHeight);
            Rect rect2 = GenUI.Rounded(GenUI.LeftPart(rect, 0.7f));
            Rect rect3 = GenUI.Rounded(GenUI.LeftPart(GenUI.Rounded(GenUI.RightPart(rect, 0.3f)), 0.9f));
            rect3.yMin -= 5f;
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect2, label);
            Text.Anchor = TextAnchor.MiddleRight;
            int hashCode = ls.CurHeight.GetHashCode();
            Widgets.IntRange(rect3, hashCode, ref range, min, max);
            if (!GenText.NullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            Text.Anchor = anchor;
            ls.Gap(ls.verticalSpacing);
        }

        public static void FloatRange(this Listing_Standard ls, string label, ref FloatRange range, float min = 0f, float max = 1f, string tooltip = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo)
        {
            Rect rect = ls.GetRect(Text.LineHeight);
            Rect rect2 = GenUI.Rounded(GenUI.LeftPart(rect, 0.7f));
            Rect rect3 = GenUI.Rounded(GenUI.LeftPart(GenUI.Rounded(GenUI.RightPart(rect, 0.3f)), 0.9f));
            rect3.yMin -= 5f;
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect2, label);
            Text.Anchor = TextAnchor.MiddleRight;
            int hashCode = ls.CurHeight.GetHashCode();
            Widgets.FloatRange(rect3, hashCode, ref range, min, max, null, valueStyle);
            if (!GenText.NullOrEmpty(tooltip))
            {
                TooltipHandler.TipRegion(rect, tooltip);
            }
            Text.Anchor = anchor;
            ls.Gap(ls.verticalSpacing);
        }

        // Token: 0x0600002A RID: 42 RVA: 0x00003900 File Offset: 0x00001B00
        public static Rect GetRect(this Listing_Standard listing_Standard, float? height = null)
        {
            return listing_Standard.GetRect(height ?? Text.LineHeight);
        }

        // Token: 0x0600002B RID: 43 RVA: 0x0000392C File Offset: 0x00001B2C
        public static void AddLabeledRadioList(this Listing_Standard listing_Standard, string header, string[] labels, ref string val, float? headerHeight = null)
        {
            if (header != string.Empty)
            {
                Widgets.Label(listing_Standard.GetRect(headerHeight), header);
            }
            listing_Standard.AddRadioList(SettingsHelper.GenerateLabeledRadioValues(labels), ref val, null);
        }

        // Token: 0x0600002C RID: 44 RVA: 0x0000396C File Offset: 0x00001B6C
        private static void AddRadioList<T>(this Listing_Standard listing_Standard, List<SettingsHelper.LabeledRadioValue<T>> items, ref T val, float? height = null)
        {
            foreach (SettingsHelper.LabeledRadioValue<T> labeledRadioValue in items)
            {
                if (Widgets.RadioButtonLabeled(listing_Standard.GetRect(height), labeledRadioValue.Label, EqualityComparer<T>.Default.Equals(labeledRadioValue.Value, val)))
                {
                    val = labeledRadioValue.Value;
                }
            }
        }

        // Token: 0x0600002D RID: 45 RVA: 0x000039EC File Offset: 0x00001BEC
        private static List<SettingsHelper.LabeledRadioValue<string>> GenerateLabeledRadioValues(string[] labels)
        {
            List<SettingsHelper.LabeledRadioValue<string>> list = new List<SettingsHelper.LabeledRadioValue<string>>();
            foreach (string text in labels)
            {
                list.Add(new SettingsHelper.LabeledRadioValue<string>(text, text));
            }
            return list;
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00003A24 File Offset: 0x00001C24
        public static void AddLabeledTextField(this Listing_Standard listing_Standard, string label, ref string settingsValue, float leftPartPct = 0.5f)
        {
            listing_Standard.LineRectSpilter(out Rect rect, out Rect rect2, leftPartPct, null);
            Widgets.Label(rect, label);
            string text = settingsValue.ToString();
            settingsValue = Widgets.TextField(rect2, text);
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003A60 File Offset: 0x00001C60
        public static void AddLabeledNumericalTextField<T>(this Listing_Standard listing_Standard, string label, ref T settingsValue, float leftPartPct = 0.5f, float minValue = 1f, float maxValue = 100000f) where T : struct
        {
            listing_Standard.LineRectSpilter(out Rect rect, out Rect rect2, leftPartPct, null);
            Widgets.Label(rect, label);
            string text = settingsValue.ToString();
            Widgets.TextFieldNumeric<T>(rect2, ref settingsValue, ref text, minValue, maxValue);
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00003AA4 File Offset: 0x00001CA4
        public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf, float leftPartPct = 0.5f, float? height = null)
        {
            Rect rect = listing_Standard.GetRect(height);
            leftHalf = GenUI.Rounded(GenUI.LeftPart(rect, leftPartPct));
            return rect;
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00003ACC File Offset: 0x00001CCC
        public static Rect LineRectSpilter(this Listing_Standard listing_Standard, out Rect leftHalf, out Rect rightHalf, float leftPartPct = 0.5f, float? height = null)
        {
            Rect rect = listing_Standard.LineRectSpilter(out leftHalf, leftPartPct, height);
            rightHalf = GenUI.Rounded(GenUI.RightPart(rect, 1f - leftPartPct));
            return rect;
        }

        // Token: 0x02000016 RID: 22
        public class LabeledRadioValue<T>
        {
            // Token: 0x0600008C RID: 140 RVA: 0x000052A7 File Offset: 0x000034A7
            public LabeledRadioValue(string label, T val)
            {
                Label = label;
                Value = val;
            }

            // Token: 0x17000011 RID: 17
            // (get) Token: 0x0600008D RID: 141 RVA: 0x000052BD File Offset: 0x000034BD
            // (set) Token: 0x0600008E RID: 142 RVA: 0x000052C5 File Offset: 0x000034C5
            public string Label { get; set; }

            // Token: 0x17000012 RID: 18
            // (get) Token: 0x0600008F RID: 143 RVA: 0x000052CE File Offset: 0x000034CE
            // (set) Token: 0x06000090 RID: 144 RVA: 0x000052D6 File Offset: 0x000034D6
            public T Value { get; set; }
        }
    }
}
