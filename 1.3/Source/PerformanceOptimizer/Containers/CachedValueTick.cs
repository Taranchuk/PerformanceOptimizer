using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Verse;

namespace PerformanceOptimizer
{
    //public class Stats
    //{
    //    public List<int> getTicks = new List<int>();
    //    public List<int> setTicks = new List<int>();
    //    public int lastRetrieveTick = 0;
    //    public int getCount;
    //    public int lastSetTick = 0;
    //    public int setCount;
    //    public int lastLogTick;
    //}
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
    //    public void LogStats(Stats profile, MethodBase methodCaller)
    //    {
    //        if (Find.TickManager.ticksGameInt < profile.lastLogTick + 60)
    //        {
    //            return;
    //        }
    //        profile.lastLogTick = Find.TickManager.ticksGameInt;
    //        var name = methodCaller?.DeclaringType?.FullName + ":" + methodCaller?.Name;
    //        Log.ResetMessageCount();
    //        if (profile.lastSetTick > 0)
    //        {
    //            if (profile.setTicks.Count > 1)
    //            {
    //                var averageTick = profile.setTicks.Average();
    //                var getToSetRate = profile.getCount / (float)profile.setCount;
    //                if (profile.setCount >= profile.getCount)
    //                {
    //                    if (profile.getCount > 2)
    //                    {
    //                        Log.Error("Fail: Calling " + name + ", set count is " + profile.setCount + ", get set rate is " + getToSetRate);
    //                    }
    //                }
    //                else if (getToSetRate < 3)
    //                {
    //                    Log.Warning("Weak: Calling " + name + ", set count is " + profile.setCount + " average tick between setting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //                else
    //                {
    //                    Log.Message("Success: Calling " + name + ", set count is " + profile.setCount + " average tick between setting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //            }
    //        }
    //
    //        if (profile.lastRetrieveTick > 0)
    //        {
    //            if (profile.getTicks.Count > 1)
    //            {
    //                var averageTick = profile.getTicks.Average();
    //                var getToSetRate = profile.getCount / (float)profile.setCount;
    //
    //                if (profile.setCount >= profile.getCount)
    //                {
    //                    if (profile.getCount > 2)
    //                    {
    //                        Log.Error("Fail: Calling " + name + ", get count is " + profile.getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
    //                    }
    //                }
    //                else if (getToSetRate < 3)
    //                {
    //                    Log.Warning("Weak: Calling " + name + ", get count is " + profile.getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //                else
    //                {
    //                    Log.Message("Success: Calling " + name + ", get count is " + profile.getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //            }
    //            else if (profile.setCount > 10)
    //            {
    //                Log.Error("Fail: Calling " + name + ", get count is " + profile.getCount + " set count is " + profile.setCount);
    //            }
    //        }
    //        else if (profile.setCount > 10)
    //        {
    //            Log.Error("Fail: Calling " + name + ", get count is " + profile.getCount + " set count is " + profile.setCount);
    //        }
    //    }
    //}

    public class CachedValueTick<T>
    {
        public int refreshTick;
        public T valueInt;
        public CachedValueTick()
        {
            valueInt = default(T);
            refreshTick = -999999999;
        }
        public CachedValueTick(T value, int resetInTicks)
        {
            this.valueInt = value;
            refreshTick = Find.TickManager.TicksGame + resetInTicks;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T value, int resetInTicks)
        {
            this.valueInt = value;
            refreshTick = Find.TickManager.TicksGame + resetInTicks;
        }
    }
}
