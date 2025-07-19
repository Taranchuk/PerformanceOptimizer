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
    //    public T cachedValue 
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
        public bool isRefreshRequired;
        public int refreshTick;
        public T cachedValue;

        public CachedObjectTick()
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
