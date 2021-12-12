using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace PerformanceOptimizer
{
    public class Optimization_ReflectionCache : Optimization
    {
        public static Dictionary<object, Traverse> createdTraverses = new Dictionary<object, Traverse>();
        public static Dictionary<Traverse, Dictionary<string, Traverse>> fields = new Dictionary<Traverse, Dictionary<string, Traverse>>();
        public static Dictionary<Traverse, object> fieldValues = new Dictionary<Traverse, object>();
        public static Dictionary<object, Dictionary<object, object>> objectValues = new Dictionary<object, Dictionary<object, object>>();
        public override OptimizationType OptimizationType => OptimizationType.Cache;
        public override string Name => "PO.CacheTraverseReflections".Translate();
        public override void DoPatches()
        {
            base.DoPatches();

            Patch(AccessTools.Method(typeof(Traverse), "Create", new Type[] { typeof(object) }), GetMethod(nameof(TraverseCreatePrefix)), GetMethod(nameof(TraverseCreatePostfix)));

            var fieldMethod = AccessTools.FirstMethod(typeof(Traverse), (MethodInfo mi) => mi.Name == "Field" && !mi.IsGenericMethod && mi.GetParameters().Count() == 1);
            Patch(fieldMethod, GetMethod(nameof(TraverseFieldPrefix)), GetMethod(nameof(TraverseFieldPostfix)));

            // TODO: look into replacing with actual field reference access. GetValue cache is buggy, it doesn't track changed values

            //var getValueMethod = AccessTools.FirstMethod(typeof(Traverse), (MethodInfo mi) => mi.Name == "GetValue" && !mi.IsGenericMethod && mi.GetParameters().Count() == 0 
            //    && mi.ReturnType == typeof(object));
            //PerformanceOptimizerMod.harmony.Patch(getValueMethod,
            //    new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseGetValuePrefix))),
            //    new HarmonyMethod(AccessTools.Method(typeof(ReflectionCache), nameof(TraverseGetValuePostfix))));
        }

        //[HarmonyPatch(typeof(Pawn), nameof(Pawn.Tick))]
        //public static class Pawn_Tick_Patch
        //{
        //    static void Postfix(Pawn __instance)
        //    {
        //        var name = __instance.nameInt;
        //        var nameField = Traverse.Create(__instance).Field("nameInt").GetValue();
        //        __instance.nameInt = new NameSingle("TEST");
        //        var name2 = __instance.nameInt;
        //        var nameField2 = Traverse.Create(__instance).Field("nameInt").GetValue();
        //        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
        //        Log.Message("Before: " + name + " - " + nameField + " - after: " + name2 + " - " + nameField2);
        //    }
        //}

        private static bool TraverseCreatePrefix(ref Traverse __result, out bool __state, object root)
        {
            try
            {
                if (root != null && createdTraverses.TryGetValue(root, out __result))
                {
                    __state = false;
                    return false;
                }
                __state = true;
            }
            catch
            {
                __state = false;
            }
            return true;
        }

        private static void TraverseCreatePostfix(Traverse __result, bool __state, object root)
        {
            try
            {
                if (__state && root != null)
                {
                    createdTraverses[root] = __result;
                }
            }
            catch { }
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

        private static void TraverseFieldPostfix(Traverse __instance, Traverse __result, bool __state, string name)
        {
            if (__state && fields.TryGetValue(__instance, out var dict))
            {
                dict[name] = __result;
            }
        }

        private static bool TraverseGetValuePrefix(Traverse __instance, ref object __result, out bool __state)
        {     
            if (objectValues.TryGetValue(__instance._root, out var dict) && dict.TryGetValue(__instance._info, out __result))
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
                if (__instance._root != null && __instance._info != null)
                {
                    // doesn't work
                    var fieldRefType = AccessTools.Method(typeof(AccessTools), "FieldRefAccess", new Type[] { typeof(string) }, new Type[] { __instance._root?.GetType(), __instance._info.GetUnderlyingType() });
                    var method = fieldRefType.Invoke(null, new object[] { __instance._info.Name });
                    var methodInvoke = method.GetType().GetMethods(AccessTools.all).FirstOrDefault(mi => mi.Name.StartsWith("Invoke"));
                    var value = methodInvoke.Invoke(method, new object[] { __instance._root });

                    Log.Message("VALUE: " + value);
                }
            }
        }

        public override void Clear()
        {

        }
    }
}

