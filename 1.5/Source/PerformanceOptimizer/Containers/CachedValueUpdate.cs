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
            valueInt = value;
            refreshUpdate = Time.frameCount + (resetInFrames - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T value, int resetInFrames)
        {
            valueInt = value;
            refreshUpdate = Time.frameCount + resetInFrames;
        }
    }
}
