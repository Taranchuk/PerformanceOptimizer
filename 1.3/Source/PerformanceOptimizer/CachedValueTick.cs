using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace PerformanceOptimizer
{
    //public class CachedValue<T> // this is just a copy with more logging and tracking, below there is a copy of this class without extra stuff
    //{
    //    public int refreshTick;
    //
    //    public int lastRetrieveTick = 0;
    //    public int getCount;
    //    public List<int> getTicks = new List<int>();
    //
    //    public int lastSetTick = 0;
    //    public int setCount;
    //    public List<int> setTicks = new List<int>();
    //
    //    private T valueInt;
    //    public CachedValue(T value, int resetInTicks)
    //    {
    //        if (PerformanceOptimizerMod.tickManager is null)
    //        {
    //            PerformanceOptimizerMod.tickManager = Current.Game.tickManager;
    //        }
    //        SetValue(value, resetInTicks);
    //    }
    //
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public T GetValue()
    //    {
    //        getCount++;
    //
    //        //if (lastRetrieveTick > 0)
    //        //{
    //        //    var diff = PerformanceOptimizerMod.tickManager.ticksGameInt - lastRetrieveTick;
    //        //    getTicks.Add(diff);
    //        //}
    //        //LogStats();
    //
    //        lastRetrieveTick = PerformanceOptimizerMod.tickManager.ticksGameInt;
    //        return valueInt;
    //    }
    //
    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public void SetValue(T value, int resetInTicks)
    //    {
    //        setCount++;
    //        this.valueInt = value;
    //        refreshTick = PerformanceOptimizerMod.tickManager.ticksGameInt + resetInTicks;
    //
    //        //if (lastSetTick > 0)
    //        //{
    //        //    var diff = PerformanceOptimizerMod.tickManager.ticksGameInt - lastSetTick;
    //        //    setTicks.Add(diff);
    //        //}
    //        //LogStats();
    //
    //        lastSetTick = PerformanceOptimizerMod.tickManager.ticksGameInt;
    //    }
    //
    //    public void LogStats()
    //    {
    //        var methodCaller = new StackTrace().GetFrame(2).GetMethod();
    //        var name = methodCaller.DeclaringType?.FullName + ":" + methodCaller.Name;
    //
    //        Log.Message(name + " - lastSetTick: " + lastSetTick + ", lastRetrieveTick: " + lastRetrieveTick + ", setTicks.Count: " + setTicks.Count + ", getTicks.Count: " + getTicks.Count);
    //        if (lastSetTick > 0)
    //        {
    //            if (setTicks.Count > 1)
    //            {
    //                var averageTick = setTicks.Average();
    //                var getToSetRate = getCount / (float)setCount;
    //                if (setCount >= getCount)
    //                {
    //                    if (getCount > 2)
    //                    {
    //                        Log.Error("Fail: Calling " + name + ", set count is " + setCount + ", get set rate is " + getToSetRate);
    //                    }
    //                }
    //                else if (getToSetRate < 3)
    //                {
    //                    Log.Warning("Weak: Calling " + name + ", set count is " + setCount + " average tick between setting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //                else
    //                {
    //                    Log.Message("Success: Calling " + name + ", set count is " + setCount + " average tick between setting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //            }
    //        }
    //
    //        if (lastRetrieveTick > 0)
    //        {
    //            if (getTicks.Count > 1)
    //            {
    //                var averageTick = getTicks.Average();
    //                var getToSetRate = getCount / (float)setCount;
    //
    //                if (setCount >= getCount)
    //                {
    //                    if (getCount > 2)
    //                    {
    //                        Log.Error("Fail: Calling " + name + ", get count is " + getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
    //                    }
    //                }
    //                else if (getToSetRate < 3)
    //                {
    //                    Log.Warning("Weak: Calling " + name + ", get count is " + getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //                else
    //                {
    //                    Log.Message("Success: Calling " + name + ", get count is " + getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
    //                }
    //            }
    //        }
    //    }
    //}

    public class CachedValueTick<T>
    {
        public int refreshTick;
        private T valueInt;
        public CachedValueTick()
        {
            valueInt = default(T);
            refreshTick = -999999999;
        }
        public CachedValueTick(T value, int resetInTicks)
        {
            SetValue(value, resetInTicks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetValue()
        {
            return valueInt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(T value, int resetInTicks)
        {
            this.valueInt = value;
            refreshTick = Find.TickManager.TicksGame + resetInTicks;
        }
    }
}
