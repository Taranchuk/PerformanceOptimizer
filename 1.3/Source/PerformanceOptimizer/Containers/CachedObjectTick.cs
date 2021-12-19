using System.Runtime.CompilerServices;
using Verse;

namespace PerformanceOptimizer
{
    //public class CachedValueTick<T> // this is just a copy with more logging and tracking, below there is a copy of this class without extra stuff
    //{
    //    public MethodBase methodCaller;
    //    public static Dictionary<MethodBase, Stats> stats = new Dictionary<MethodBase, Stats>();
    //
    //    public int refreshTick;
    //
    //    public T valueIntInt;
    //    public T valueInt 
    //    {
    //        get { return GetValue(); }
    //        set { }
    //    }
    //    
    //    public CachedValueTick(T value, int resetInTicks)
    //    {
    //        if (PerformanceOptimizerMod.tickManager is null)
    //        {
    //            PerformanceOptimizerMod.tickManager = Current.Game.tickManager;
    //        }
    //        SetValue(value, resetInTicks);
    //        methodCaller = new StackTrace().GetFrame(1).GetMethod();
    //        if (!stats.TryGetValue(methodCaller, out var profile))
    //        {
    //            stats[methodCaller] = new Stats();
    //        }
    //    }
    //
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public T GetValue()
    //    {
    //        if (methodCaller is null)
    //        {
    //            methodCaller = new StackTrace().GetFrame(2).GetMethod();
    //        }
    //        if (methodCaller != null && stats.TryGetValue(methodCaller, out var profile))
    //        {
    //            profile.getCount++;
    //            if (profile.lastRetrieveTick > 0)
    //            {
    //                var diff = PerformanceOptimizerMod.tickManager.ticksGameInt - profile.lastRetrieveTick;
    //                profile.getTicks.Add(diff);
    //            }
    //            LogStats(profile, methodCaller);
    //            profile.lastRetrieveTick = PerformanceOptimizerMod.tickManager.ticksGameInt;
    //        }
    //        return valueIntInt;
    //    }
    //
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void SetValue(T value, int resetInTicks)
    //    {
    //        Stats profile = null;
    //        if (methodCaller is null)
    //        {
    //            methodCaller = new StackTrace().GetFrame(3).GetMethod();
    //        }
    //        if (methodCaller != null && stats.TryGetValue(methodCaller, out profile))
    //        {
    //            profile.setCount++;
    //        }
    //        this.valueIntInt = value;
    //        refreshTick = PerformanceOptimizerMod.tickManager.ticksGameInt + resetInTicks;
    //        if (profile != null)
    //        {
    //            if (profile.lastSetTick > 0)
    //            {
    //                var diff = PerformanceOptimizerMod.tickManager.ticksGameInt - profile.lastSetTick;
    //                profile.setTicks.Add(diff);
    //            }
    //            LogStats(profile, methodCaller);
    //            profile.lastSetTick = PerformanceOptimizerMod.tickManager.ticksGameInt;
    //        }
    //
    //    }
    //
    //}

    public class CachedObjectTick<T> where T : class
    {
        public bool cached;
        public bool refreshNow;
        public int refreshTick;
        public T valueInt;
        public CachedObjectTick()
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRefresh(ref T __result)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessResult(ref T __result, int refreshRate)
        {
            if (this.refreshNow)
            {
                this.SetValue(__result, refreshRate);
            }
            else if (this.cached && this.valueInt != __result)
            {
                __result = this.valueInt;
            }
        }
    }
}
