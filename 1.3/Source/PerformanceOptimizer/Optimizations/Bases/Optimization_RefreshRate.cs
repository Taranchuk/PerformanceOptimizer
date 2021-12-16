using HarmonyLib;
using Verse;

namespace PerformanceOptimizer
{
    public abstract class Optimization_RefreshRate : Optimization
    {
        public virtual int MaxSliderValue => 2500;
        public override void Reset()
        {
            base.Reset();
            refreshRate = RefreshRateByDefault;
            SetRefreshRate();
        }
        public abstract int RefreshRateByDefault { get; }

        public int refreshRate;
        public override void Apply()
        {
            base.Apply();
            SetRefreshRate();
        }

        private void SetRefreshRate()
        {
            AccessTools.Field(GetType(), "refreshRateStatic").SetValue(null, refreshRate);
        }

        public override void DrawSettings(Listing_Standard section)
        {
            var sliderName = OptimizationType == OptimizationType.CacheWithRefreshRate ? "PO.RefreshRate" : "PO.ThrottleRate";
            section.CheckboxLabeledWithSlider(Label, sliderName, ref enabled, ref refreshRate, MaxSliderValue, actionOnClick: Apply, actionOnSlider: SetRefreshRate);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref refreshRate, "refreshRate");
        }
    }
}
