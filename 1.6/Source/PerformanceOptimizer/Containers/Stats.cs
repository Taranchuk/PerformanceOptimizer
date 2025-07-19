using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PerformanceOptimizer
{
    public class Stats
    {
        public List<int> getTicks = new();
        public List<int> setTicks = new();
        public int lastRetrieveTick = 0;
        public int getCount;
        public int lastSetTick = 0;
        public int setCount;
        public int lastLogTick;
        public void LogStats(Stats profile, MethodBase methodCaller)
        {
            if (Find.TickManager.ticksGameInt < profile.lastLogTick + 60)
            {
                return;
            }
            profile.lastLogTick = Find.TickManager.ticksGameInt;
            string name = methodCaller?.DeclaringType?.FullName + ":" + methodCaller?.Name;
            Log.ResetMessageCount();
            if (profile.lastSetTick > 0)
            {
                if (profile.setTicks.Count > 1)
                {
                    double averageTick = profile.setTicks.Average();
                    float getToSetRate = profile.getCount / (float)profile.setCount;
                    if (profile.setCount >= profile.getCount)
                    {
                        if (profile.getCount > 2)
                        {
                            Log.Error("Fail: Calling " + name + ", set count is " + profile.setCount + ", get set rate is " + getToSetRate);
                        }
                    }
                    else if (getToSetRate < 3)
                    {
                        Log.Warning("Weak: Calling " + name + ", set count is " + profile.setCount + " average tick between setting is " + averageTick + ", get set rate is " + getToSetRate);
                    }
                    else
                    {
                        Log.Message("Success: Calling " + name + ", set count is " + profile.setCount + " average tick between setting is " + averageTick + ", get set rate is " + getToSetRate);
                    }
                }
            }

            if (profile.lastRetrieveTick > 0)
            {
                if (profile.getTicks.Count > 1)
                {
                    double averageTick = profile.getTicks.Average();
                    float getToSetRate = profile.getCount / (float)profile.setCount;

                    if (profile.setCount >= profile.getCount)
                    {
                        if (profile.getCount > 2)
                        {
                            Log.Error("Fail: Calling " + name + ", get count is " + profile.getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
                        }
                    }
                    else if (getToSetRate < 3)
                    {
                        Log.Warning("Weak: Calling " + name + ", get count is " + profile.getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
                    }
                    else
                    {
                        Log.Message("Success: Calling " + name + ", get count is " + profile.getCount + " average tick between getting is " + averageTick + ", get set rate is " + getToSetRate);
                    }
                }
                else if (profile.setCount > 10)
                {
                    Log.Error("Fail: Calling " + name + ", get count is " + profile.getCount + " set count is " + profile.setCount);
                }
            }
            else if (profile.setCount > 10)
            {
                Log.Error("Fail: Calling " + name + ", get count is " + profile.getCount + " set count is " + profile.setCount);
            }
        }
    }
}
