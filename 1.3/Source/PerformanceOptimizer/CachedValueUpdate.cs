using System.Runtime.CompilerServices;
using UnityEngine;

namespace PerformanceOptimizer
{
    public class CachedValueUpdate<T>
    {
        public int refreshUpdate;
        private T valueInt;
        public CachedValueUpdate(T value, int resetInFrames)
        {
            SetValue(value, resetInFrames);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue()
        {
            return valueInt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T value, int resetInFrames)
        {
            this.valueInt = value;
            refreshUpdate = Time.frameCount + resetInFrames;
        }
    }
}
