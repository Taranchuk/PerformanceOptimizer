using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using static PerformanceOptimizer.ComponentCache;

namespace PerformanceOptimizer
{
    public class Optimization_FasterGetCompReplacement : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Label => "PO.FasterGetCompReplacement".Translate();
        public override int DrawOrder => -99999;

        public static MethodInfo genericMapGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetMapComponentFast));
        public static MethodInfo genericWorldGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldComponentFast));
        public static MethodInfo genericGameGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetGameComponentFast));
        public static MethodInfo genericThingGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetThingCompFast));
        public static MethodInfo genericThingTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetThingCompFast));
        public static MethodInfo genericHediffTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetHediffCompFast));
        public static MethodInfo genericWorldObjectGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldObjectCompFast));
        public static MethodInfo genericThingDefCompProps = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetThingDefPropsFast));
        public static MethodInfo genericHediffDefCompProps = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetHediffDefPropsFast));
        public static MethodInfo genericGetModExtension = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetModExtensionFast));
        public static MethodInfo genericHasModExtension = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.HasModExtensionFast));

        public static HashSet<string> assembliesToSkip = new HashSet<string>
        {
            "System", "Cecil", "Multiplayer", "Prepatcher", "HeavyMelee", "0Harmony", "UnityEngine", "mscorlib", "ICSharpCode", "Newtonsoft", "TranspilerExplorer"
        };

        public static HashSet<string> typesToSkip = new HashSet<string>
        {
            "AnimalGenetics.ColonyManager+JobsWrapper", "AutoMachineTool", "NightVision.CombatHelpers", "RJWSexperience.UI.SexStatusWindow"
        };

        public static HashSet<string> methodsToSkip = new HashSet<string>
        {
            "Numbers.MainTabWindow_Numbers", "Numbers.OptionsMaker", "<PawnSelector>g__Action", "<AllBuildingsColonistWithComp>", "<FailOnOwnerStatus>", "Transpiler"
        };

        private static bool TypeValidator(Type type)
        {
            return !assembliesToSkip.Any(asmName => type.Assembly?.FullName?.Contains(asmName) ?? false) && !typesToSkip.Any(x => type.FullName.Contains(x));
        }

        private static List<Type> types;
        public static List<Type> GetTypesToParse()
        {
            if (types is null)
            {
                types = GenTypes.AllTypes.Where(type => TypeValidator(type)).Distinct().ToList();
            }
            return types;
        }

        public static List<MethodInfo> GetMethodsToParse(Type type)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (var method in AccessTools.GetDeclaredMethods(type))
            {
                if (method != null && !method.IsAbstract)
                {
                    try
                    {
                        var description = method.FullDescription();
                        if (!methodsToSkip.Any(x => description.Contains(x)))
                        {
                            if (!method.IsGenericMethod && !method.ContainsGenericParameters && !method.IsGenericMethodDefinition)
                            {
                                methods.Add(method);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //Log.Error("Exception: " + ex);
                    }
                }
            }
            return methods;
        }

        public struct PatchInfo
        {
            public MethodInfo targetMethod;
            public CodeInstruction targetInstruction;
            public MethodInfo methodToReplace;
        }

        private static Dictionary<MethodBase, List<PatchInfo>> patchInfos;
        public static void ParseMethod(MethodInfo method)
        {
            try
            {
                var instructions = PatchProcessor.GetCurrentInstructions(method);
                foreach (var instr in instructions)
                {
                    if (instr.operand is MethodInfo mi && mi.IsGenericMethod)
                    {
                        var parameterLength = mi.GetParameters().Length;
                        if (parameterLength == 0)
                        {
                            if (instr.opcode == OpCodes.Callvirt || instr.opcode == OpCodes.Call)
                            {
                                var miName = mi.Name;
                                if (miName == "GetComponent")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (typeof(MapComponent).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericMapGetComp);
                                    }
                                    else if (typeof(GameComponent).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericGameGetComp);
                                    }
                                    else if (typeof(WorldComponent).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericWorldGetComp);
                                    }
                                    else if (typeof(WorldObjectComp).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericWorldObjectGetComp);
                                    }
                                }
                                else if (miName == "GetComp")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (typeof(ThingComp).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericThingGetComp);
                                    }
                                }
                                else if (miName == "GetCompProperties")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (typeof(CompProperties).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericThingDefCompProps);
                                    }
                                }
                                else if (miName == "CompProps")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (typeof(HediffCompProperties).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericHediffDefCompProps);
                                    }
                                }
                                else if (miName == "GetModExtension")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (typeof(DefModExtension).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericGetModExtension);
                                    }
                                }
                                else if (miName == "HasModExtension")
                                {
                                    var genericArgs = mi.GetGenericArguments();
                                    if (genericArgs.Length > 0)
                                    {
                                        var type = genericArgs[0];
                                        if (typeof(DefModExtension).IsAssignableFrom(type))
                                        {
                                            AddPatchInfo(method, instr, type, genericHasModExtension);
                                        }
                                    }
                                    else
                                    {
                                        Log.Message("FAIL: " + mi.FullDescription());
                                    }
                                }
                            }
                        }
                        else if (parameterLength == 1)
                        {
                            if (instr.opcode == OpCodes.Call)
                            {
                                if (mi.Name == "TryGetComp")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (typeof(ThingComp).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericThingTryGetComp);
                                    }
                                    else if (typeof(HediffComp).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericHediffTryGetComp);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Error("Error when parsing " + method.FullDescription() + " - exception: " + ex);
            }

            void AddPatchInfo(MethodInfo targetMethod, CodeInstruction instr, Type genericType, MethodInfo genericMethod)
            {
                //Log.Message("Patched " + method.FullDescription() + " - instr: " + instr);
                //Log.ResetMessageCount();
                var patchInfo = new PatchInfo
                {
                    targetMethod = targetMethod,
                    targetInstruction = instr,
                    methodToReplace = genericMethod.MakeGenericMethod(new Type[] { genericType })
                };

                if (patchInfos.ContainsKey(method))
                {
                    patchInfos[method].Add(patchInfo);
                }
                else
                {
                    patchInfos[method] = new List<PatchInfo>
                    {
                        patchInfo
                    };
                }
            }
        }
        public override void DoPatches()
        {
            base.DoPatches();
            bool parse = false;
            if (patchInfos is null)
            {
                patchInfos = new Dictionary<MethodBase, List<PatchInfo>>();
                parse = true;
            }
            DoPatchesAsync(parse);
        }
        public async void DoPatchesAsync(bool parse)
        {
            await Task.Run(() =>
            {
                if (parse)
                {
                    ParseEverything();
                }
            });
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var transpiler = GetMethod(nameof(Optimization_FasterGetCompReplacement.Transpiler));
            foreach (var kvp in patchInfos)
            {
                Patch(kvp.Key, transpiler: transpiler);
            }
            stopwatch.Stop();
            //stopwatch.LogTime("Transpiled " + patchedMethods.Count + " methods");
        }

        private void ParseEverything()
        {
            try
            {
                var methodsToParse = new HashSet<MethodInfo>();
                var types = GetTypesToParse();
                foreach (var type in types)
                {
                    foreach (var method in GetMethodsToParse(type))
                    {
                        methodsToParse.Add(method);
                    }
                }
                foreach (var method in methodsToParse)
                {
                    ParseMethod(method);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception in Performance Optimizer: " + ex);
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            var curPatchInfos = patchInfos[method];
            bool patchedSomething = false;
            foreach (var instr in instructions)
            {
                for (var j = 0; j < curPatchInfos.Count; j++)
                {
                    var patchInfo = curPatchInfos[j];
                    if (patchInfo.targetInstruction.opcode == instr.opcode && patchInfo.targetInstruction.operand == instr.operand)
                    {
                        instr.opcode = OpCodes.Call;
                        instr.operand = patchInfo.methodToReplace;
                        //Log.Message("Patched: " + patchInfo.targetMethod.FullDescription() + " - Replaced " + instr);
                        patchedSomething = true;
                        break;
                    }
                }
                yield return instr;
            }
            //if (!patchedSomething)
            //{
            //    Log.Error("Performance Optimizer failed to transpile:");
            //    foreach (var patchInfo in curPatchInfos)
            //    {
            //        Log.Error(" - " + patchInfo.targetMethod.FullDescription());
            //    }
            //}
        }

        private static List<MethodInfo> clearMethods;
        public override void Clear()
        {
            if (clearMethods is null)
            {
                clearMethods = GetClearMethods().ToList();
            }
            foreach (var method in clearMethods)
            {
                try
                {
                    method.Invoke(null, null);
                }
                catch { }
            }
        }

        private static IEnumerable<MethodInfo> GetClearMethods()
        {
            foreach (var type in typeof(ThingComp).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_ThingComp<>), type);
            }
            foreach (var type in typeof(HediffComp).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_HediffComp<>), type);
            }
            foreach (var type in typeof(WorldObjectComp).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_WorldObjectComp<>), type);
            }
            foreach (var type in typeof(GameComponent).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_GameComponent<>), type);
            }
            foreach (var type in typeof(WorldComponent).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_WorldComponent<>), type);
            }
            foreach (var type in typeof(MapComponent).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_MapComponent<>), type);
            }

            MethodInfo ClearMethod(Type genericType, Type genericParam)
            {
                return genericType.MakeGenericType(genericParam).GetMethod("Clear", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class ComponentCache
    {
        public static class ICache_ThingComp<T> where T : ThingComp
        {
            public static Dictionary<int, T> compsById = new Dictionary<int, T>();
            public static void Clear()
            {
                compsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetThingCompFast<T>(this ThingWithComps thingWithComps) where T : ThingComp
        {
            if (ICache_ThingComp<T>.compsById.TryGetValue(thingWithComps.thingIDNumber, out T val))
            {
                return val;
            }

            if (thingWithComps.comps == null)
            {
                return null;
            }

            for (int i = 0; i < thingWithComps.comps.Count; i++)
            {
                var props = thingWithComps.comps[i].props;
                if (props != null && props.compClass == typeof(T))
                {
                    var val2 = thingWithComps.comps[i] as T;
                    ICache_ThingComp<T>.compsById[thingWithComps.thingIDNumber] = val2;
                    return val2;
                }
            }

            for (int i = 0; i < thingWithComps.comps.Count; i++)
            {
                T val3 = thingWithComps.comps[i] as T;
                if (val3 != null)
                {
                    ICache_ThingComp<T>.compsById[thingWithComps.thingIDNumber] = val3;
                    return val3;
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TryGetThingCompFast<T>(this Thing thing) where T : ThingComp
        {
            ThingWithComps thingWithComps = thing as ThingWithComps;
            if (thingWithComps == null)
            {
                return null;
            }
            return thingWithComps.GetThingCompFast<T>();
        }

        public static class ICache_HediffComp<T> where T : HediffComp
        {
            public static Dictionary<int, T> compsById = new Dictionary<int, T>();
            public static void Clear()
            {
                compsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TryGetHediffCompFast<T>(this Hediff hd) where T : HediffComp
        {
            if (ICache_HediffComp<T>.compsById.TryGetValue(hd.loadID, out T val))
            {
                return val;
            }
            HediffWithComps hediffWithComps = hd as HediffWithComps;
            if (hediffWithComps == null)
            {
                return null;
            }
            if (hediffWithComps.comps == null)
            {
                return null;
            }

            for (int i = 0; i < hediffWithComps.comps.Count; i++)
            {
                var props = hediffWithComps.comps[i].props;
                if (props != null && props.compClass == typeof(T))
                {
                    var val2 = hediffWithComps.comps[i] as T;
                    ICache_HediffComp<T>.compsById[hd.loadID] = val2;
                    return val2;
                }
            }

            for (int i = 0; i < hediffWithComps.comps.Count; i++)
            {
                T val3 = hediffWithComps.comps[i] as T;
                if (val3 != null)
                {
                    ICache_HediffComp<T>.compsById[hd.loadID] = val3;
                    return val3;
                }
            }
            return null;
        }
        public static class ICache_WorldObjectComp<T> where T : WorldObjectComp
        {
            public static Dictionary<int, T> compsById = new Dictionary<int, T>();
            public static void Clear()
            {
                compsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetWorldObjectCompFast<T>(this WorldObject worldObject) where T : WorldObjectComp
        {
            if (ICache_WorldObjectComp<T>.compsById.TryGetValue(worldObject.ID, out T val))
            {
                return val;
            }
            if (worldObject.comps == null)
            {
                return null;
            }
            for (int i = 0; i < worldObject.comps.Count; i++)
            {
                if (worldObject.comps[i].GetType() == typeof(T))
                {
                    var val2 = worldObject.comps[i] as T;
                    ICache_WorldObjectComp<T>.compsById[worldObject.ID] = val2;
                    return val2;
                }
            }

            for (int i = 0; i < worldObject.comps.Count; i++)
            {
                T val3 = worldObject.comps[i] as T;
                if (val3 != null)
                {
                    ICache_WorldObjectComp<T>.compsById[worldObject.ID] = val3;
                    return val3;
                }
            }
            return null;
        }
        public static class ICache_ThingDefProps<T> where T : CompProperties
        {
            public static Dictionary<ushort, T> compPropsById = new Dictionary<ushort, T>();
            public static void Clear()
            {
                compPropsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetThingDefPropsFast<T>(this ThingDef thingDef) where T : CompProperties
        {
            if (ICache_ThingDefProps<T>.compPropsById.TryGetValue(thingDef.shortHash, out T val))
            {
                return val;
            }

            if (thingDef.comps == null)
            {
                return null;
            }

            for (int i = 0; i < thingDef.comps.Count; i++)
            {
                T val3 = thingDef.comps[i] as T;
                if (val3 != null)
                {
                    ICache_ThingDefProps<T>.compPropsById[thingDef.shortHash] = val3;
                    return val3;
                }
            }
            return null;
        }
        public static class ICache_HediffDefProps<T> where T : HediffCompProperties
        {
            public static Dictionary<ushort, T> compPropsById = new Dictionary<ushort, T>();
            public static void Clear()
            {
                compPropsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetHediffDefPropsFast<T>(this HediffDef hediffDef) where T : HediffCompProperties
        {
            if (ICache_HediffDefProps<T>.compPropsById.TryGetValue(hediffDef.shortHash, out T val))
            {
                return val;
            }

            if (hediffDef.comps == null)
            {
                return null;
            }

            for (int i = 0; i < hediffDef.comps.Count; i++)
            {
                T val3 = hediffDef.comps[i] as T;
                if (val3 != null)
                {
                    ICache_HediffDefProps<T>.compPropsById[hediffDef.shortHash] = val3;
                    return val3;
                }
            }
            return null;
        }


        public static class ICache_DefModExtension<T> where T : DefModExtension
        {
            public static Dictionary<ushort, T> modExtensionsById = new Dictionary<ushort, T>();
            public static void Clear()
            {
                modExtensionsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetModExtensionFast<T>(this Def def) where T : DefModExtension
        {
            if (ICache_DefModExtension<T>.modExtensionsById.TryGetValue(def.shortHash, out T val))
            {
                return val;
            }

            if (def.modExtensions == null)
            {
                return null;
            }
            for (int i = 0; i < def.modExtensions.Count; i++)
            {
                var val2 = def.modExtensions[i] as T;
                if (val2 != null)
                {
                    ICache_DefModExtension<T>.modExtensionsById[def.shortHash] = val2;
                    return val2;
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasModExtensionFast<T>(this Def def) where T : DefModExtension
        {
            return GetModExtensionFast<T>(def) != null;
        }
        public static class ICache_MapComponent<T>
        {
            public static Dictionary<Map, T> compsByMap = new Dictionary<Map, T>();
            public static void Clear()
            {
                compsByMap.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetMapComponentFast<T>(this Map map) where T : MapComponent
        {
            if (!ICache_MapComponent<T>.compsByMap.TryGetValue(map, out T mapComp))
            {
                var comp = map.GetComponent<T>();
                if (comp != null)
                {
                    ICache_MapComponent<T>.compsByMap[map] = mapComp = comp;
                }
            }
            return mapComp;
        }
        public static class ICache_WorldComponent<T> where T : WorldComponent
        {
            public static World world;
            public static T component;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetComponent(World curWorld)
            {
                if (world != curWorld)
                {
                    var comp = curWorld.GetComponent<T>();
                    if (comp != null)
                    {
                        component = comp;
                        world = curWorld;
                    }
                }
                return component;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Clear()
            {
                world = null;
                component = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetWorldComponentFast<T>(this World world) where T : WorldComponent
        {
            return ICache_WorldComponent<T>.GetComponent(world);
        }
        public static class ICache_GameComponent<T> where T : GameComponent
        {
            public static Game game;
            public static T component;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T GetComponent(Game curGame)
            {
                if (game != curGame)
                {
                    var comp = curGame.GetComponent<T>();
                    if (comp != null)
                    {
                        component = comp;
                        game = curGame;
                    }
                }
                return component;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Clear()
            {
                game = null;
                component = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetGameComponentFast<T>(this Game game) where T : GameComponent
        {
            return ICache_GameComponent<T>.GetComponent(game);
        }
    }
}
