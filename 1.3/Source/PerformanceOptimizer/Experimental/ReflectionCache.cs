using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;
using static Mono.Security.X509.X520;

namespace PerformanceOptimizer
{
    [StaticConstructorOnStartup]
    public static class ReflectionCache
    {

        public static Dictionary<object, Traverse> createdTraverses = new Dictionary<object, Traverse>();
        public static Dictionary<Traverse, Dictionary<string, Traverse>> fields = new Dictionary<Traverse, Dictionary<string, Traverse>>();
        public static Dictionary<Traverse, object> fieldValues = new Dictionary<Traverse, object>();

        static ReflectionCache()
        {
            PerformanceOptimizerMod.harmony.Patch(AccessTools.Method(typeof(Traverse), "Create", new Type[] { typeof(object) }),
                new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseCreatePrefix))),
                new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseCreatePostfix))));

            var fieldMethod = AccessTools.FirstMethod(typeof(Traverse), (MethodInfo mi) => mi.Name == "Field" && !mi.IsGenericMethod && mi.GetParameters().Count() == 1);
            PerformanceOptimizerMod.harmony.Patch(fieldMethod,
                new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseFieldPrefix))),
                new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseFieldPostfix))));

            var getValueMethod = AccessTools.FirstMethod(typeof(Traverse), (MethodInfo mi) => mi.Name == "GetValue" && !mi.IsGenericMethod && mi.GetParameters().Count() == 0 
            && mi.ReturnType == typeof(object));
            PerformanceOptimizerMod.harmony.Patch(getValueMethod,
                new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseGetValuePrefix))),
                new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseGetValuePostfix))));
        }

        private static bool TraverseCreatePrefix(ref Traverse __result, out bool __state, object root)
        {
            if (createdTraverses.TryGetValue(root, out __result))
            {
                __state = false;
                return false;
            }
            __state = true;
            return true;
        }

        private static void TraverseCreatePostfix(Traverse __result, bool __state, object root)
        {
            if (__state)
            {
                createdTraverses[root] = __result;
            }
        }

        private static bool TraverseFieldPrefix(Traverse __instance, ref Traverse __result, out bool __state, string name)
        {
            if (!fields.TryGetValue(__instance, out var dict))
            {
                fields[__instance] = dict = new Dictionary<string, Traverse>();
            }
            if (dict.TryGetValue(name, out __result))
            {
                __state = false;
                return false;
            }

            __state = true;
            return true;
        }

        private static void TraverseFieldPostfix(Traverse __result, bool __state, string name)
        {
            if (__state && fields.TryGetValue(__result, out var dict))
            {
                dict[name] = __result;
            }
        }

        private static bool TraverseGetValuePrefix(Traverse __instance, ref object __result, out bool __state)
        {
            if (fieldValues.TryGetValue(__instance, out __result))
            {
                __state = false;
                return false;
            }
            __state = true;
            return true;
        }

        private static void TraverseGetValuePostfix(Traverse __instance, object __result, bool __state)
        {
            if (__state)
            {
                fieldValues[__instance] = __result;
            }
        }
    }
}

