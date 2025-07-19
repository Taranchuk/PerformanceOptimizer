using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace PerformanceOptimizer
{
    public class CachedValueTick<T> where T : struct
    {
        public bool isRefreshRequired;
        public int refreshTick;
        public T cachedValue;

        public CachedValueTick()
        {
            isRefreshRequired = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetOrRefresh(ref T __result)
        {
            if (PerformanceOptimizerMod.tickManager.ticksGameInt > refreshTick)
            {
                isRefreshRequired = true;
                return true;
            }
            __result = cachedValue;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessResult(ref T __result, int refreshRate)
        {
            if (isRefreshRequired)
            {
                cachedValue = __result;
                refreshTick = PerformanceOptimizerMod.tickManager.ticksGameInt + (refreshRate - 1);
                isRefreshRequired = false;
            }
            else
            {
                __result = cachedValue;
            }
        }
    }
}
