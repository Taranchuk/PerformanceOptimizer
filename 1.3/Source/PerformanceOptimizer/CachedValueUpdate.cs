using System.Runtime.CompilerServices;
using UnityEngine;

namespace PerformanceOptimizer
{
    public class CachedValueUpdate<T>
    {
        public int refreshUpdate;
        public T valueInt;
        public CachedValueUpdate(T value, int resetInFrames)
        {
            this.valueInt = value;
            refreshUpdate = Time.frameCount + resetInFrames;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T value, int resetInFrames)
        {
            this.valueInt = value;
            refreshUpdate = Time.frameCount + resetInFrames;
        }
    }
}
