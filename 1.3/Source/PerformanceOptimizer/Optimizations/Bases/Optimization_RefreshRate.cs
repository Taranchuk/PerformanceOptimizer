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
            AccessTools.Field(GetType(), "refreshRateStatic").SetValue(null, refreshRate);
        }
        public abstract int RefreshRateByDefault { get; }

        public int refreshRate;
        public override void Apply()
        {
            base.Apply();
            AccessTools.Field(GetType(), "refreshRateStatic").SetValue(null, refreshRate);
        }

        public override void DrawSettings(Listing_Standard section)
        {
            var sliderName = this.OptimizationType == OptimizationType.CacheWithRefreshRate ? "PO.RefreshRate" : "PO.ThrottleRate";
            section.CheckboxLabeledWithSlider(this.Label, sliderName, ref this.enabled, ref this.refreshRate, this.MaxSliderValue, actionOnClick: this.Apply);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref refreshRate, "refreshRate");
        }
    }
}
