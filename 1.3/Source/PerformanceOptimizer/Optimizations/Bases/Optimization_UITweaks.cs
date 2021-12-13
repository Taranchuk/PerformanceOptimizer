using System;
using System.Collections.Generic;

namespace PerformanceOptimizer
{
    public abstract class Optimization_UITweaks : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.UITweak;
        public override bool EnabledByDefault => false;
    }
}
