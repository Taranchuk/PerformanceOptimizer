using System.Runtime.CompilerServices;
using Verse;

namespace PerformanceOptimizer
{
    public class CachedValueTick<T> where T : struct
    {
        public bool cached;
        public bool refreshNow;
        public int refreshTick;
        public T valueInt;
        public CachedValueTick()
        {
            refreshNow = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T value, int resetInTicks)
        {
            valueInt = value;
            refreshTick = Find.TickManager.TicksGame + resetInTicks;
            refreshNow = false;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetOrRefresh(ref T __result)
        {
            if (PerformanceOptimizerMod.tickManager.ticksGameInt > this.refreshTick)
            {
                this.refreshNow = true;
                return true;
            }
            else
            {
                __result = this.valueInt;
                this.refreshNow = false;
                this.cached = true;
                return false;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessResult(ref T __result, int refreshRate)
        {
            if (this.refreshNow)
            {
                this.SetValue(__result, refreshRate);
            }
            else if (this.cached && !this.valueInt.Equals(__result))
            {
                __result = this.valueInt;
            }
        }
    }
}
