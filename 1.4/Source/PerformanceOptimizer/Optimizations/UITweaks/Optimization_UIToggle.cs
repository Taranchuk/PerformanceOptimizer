using RimWorld;
using UnityEngine;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_UIToggle : Optimization_UITweaks
    {
        public override int DrawOrder => 2;
        public override bool EnabledByDefault => true;
        public static bool UIToggleOn = true;
        public static bool oneKeyMode = false;
        public static bool UITogglePressed => oneKeyMode
                    ? Input.GetKeyDown(PerformanceOptimizerMod.keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B))
                    : Input.GetKey(PerformanceOptimizerMod.keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.A))
                    && Input.GetKeyDown(PerformanceOptimizerMod.keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B));

        public override void Reset()
        {
            base.Reset();
            UIToggleOn = true;
            oneKeyMode = false;
        }

        public static int curFrameCount = 0;
        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(MapInterface), "HandleMapClicks", postfix: GetMethod(nameof(Postfix)));
        }
        public static void Postfix()
        {
            if (UITogglePressed && curFrameCount != Time.frameCount)
            {
                curFrameCount = Time.frameCount;
                UIToggleOn = !UIToggleOn;
                LoadedModManager.GetMod<PerformanceOptimizerMod>().WriteSettings();
            }
        }
        public override string Label => "PO.UIToggle".Translate();
        public override void DrawSettings(Listing_Standard section)
        {
            KeyPrefsData keyPrefsData = KeyPrefs.KeyPrefsData;
            string keyHidingText = Label;
            Vector2 size = Text.CalcSize(keyHidingText);
            Rect keybindingRect = new(section.curX + size.x + 10, section.curY, 50, 24f);
            Rect keybindingRectPlus = new(keybindingRect.xMax, section.curY - 3, 24, 24);
            if (!oneKeyMode)
            {
                if (Widgets.ButtonText(keybindingRect, keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.A).ToStringReadable()))
                {
                    Find.WindowStack.Add(new Dialog_DefineBinding(keyPrefsData, PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.A));
                    Event.current.Use();
                }
                Text.Font = GameFont.Medium;
                Widgets.Label(keybindingRectPlus, " + ");
                Text.Font = GameFont.Small;
            }
            Rect keybinding2Rect = new(keybindingRectPlus.xMax, section.curY, 50, 24f);
            if (Widgets.ButtonText(keybinding2Rect, keyPrefsData.GetBoundKeyCode(PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B).ToStringReadable()))
            {
                Find.WindowStack.Add(new Dialog_DefineBinding(keyPrefsData, PODefOf.PerformanceOptimizerKey, KeyPrefs.BindingSlot.B));
                Event.current.Use();
            }

            Rect checkboxOneKey = new(keybinding2Rect.xMax + 10, keybinding2Rect.y, 120, 24);
            Widgets.CheckboxLabeled(checkboxOneKey, "PO.OneKeyMode".Translate(), ref oneKeyMode);
            section.Label(keyHidingText);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref oneKeyMode, "OneKeyMode", false);
            Scribe_Values.Look(ref UIToggleOn, "UIToggleOn", true);
        }
    }
}
