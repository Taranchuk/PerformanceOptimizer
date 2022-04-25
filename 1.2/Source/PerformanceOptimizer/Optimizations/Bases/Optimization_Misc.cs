namespace PerformanceOptimizer
{
    public abstract class Optimization_Misc : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Misc;
        public override int DrawOrder => 2;
    }
}

