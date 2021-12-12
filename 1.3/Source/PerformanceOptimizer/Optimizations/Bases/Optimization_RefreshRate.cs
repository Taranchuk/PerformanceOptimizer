using Verse;

namespace PerformanceOptimizer
{
    public abstract class Optimization_RefreshRate : Optimization
    {
        public virtual int MaxSliderValue => 2500;
        public override void Reset()
        {
            base.Reset();
            refreshRateStatic = refreshRate = RefreshRateByDefault;
        }
        public abstract int RefreshRateByDefault { get; }

        public int refreshRate;

        public static int refreshRateStatic;

        public override void Apply()
        {
            base.Apply();
            refreshRateStatic = refreshRate;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref refreshRate, "refreshRate");
        }
    }
}
