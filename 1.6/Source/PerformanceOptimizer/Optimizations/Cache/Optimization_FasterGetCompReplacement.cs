﻿using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static PerformanceOptimizer.ComponentCache;

namespace PerformanceOptimizer
{
    public class PerformPatchesPerFrames : MonoBehaviour
    {
        public Optimization_FasterGetCompReplacement optimization;
        public const float MaxTimeToLoadThisFrame = 0.001f;
        public IEnumerator PerformPatches()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            var totalElapsed = 0f;
            Log.Message("Starting patches: " + optimization.patchesToPerform.Count + " - " + DateTime.Now.ToString());
            while (optimization.patchesToPerform.Any())
            {
                var patch = optimization.patchesToPerform.Pop();
                optimization.Patch(patch.Key, transpiler: Optimization_FasterGetCompReplacement.transpiler);
                float elapsed = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
                totalElapsed += elapsed;
                if (elapsed >= MaxTimeToLoadThisFrame)
                {
                    yield return 0;
                    stopwatch.Restart();
                }
            }
            stopwatch.Stop();
            Log.Message("Finished transpiling " + (optimization.patchedMethods.Count - 2) + " methods - " + DateTime.Now.ToString());
            yield return null;
        }
    }

    public class Optimization_FasterGetCompReplacement : Optimization
    {
        public override OptimizationType OptimizationType => OptimizationType.Optimization;
        public override string Label => "PO.FasterGetCompReplacement".Translate();
        public override int DrawOrder => -99999;

        public static MethodInfo genericMapGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetMapComponentFast));
        public static MethodInfo genericWorldGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldComponentFast));
        public static MethodInfo genericGameGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetGameComponentFast));
        public static MethodInfo genericHediffTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetHediffCompFast));
        public static MethodInfo genericWorldObjectGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldObjectCompFast));
        public static MethodInfo genericThingDefCompProps = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetThingDefPropsFast));
        public static MethodInfo genericHediffDefCompProps = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetHediffDefPropsFast));

        public static HashSet<string> assembliesToSkip = new()
        {
            "System", "Cecil", "Multiplayer", "Prepatcher", "HeavyMelee", "0Harmony", "UnityEngine", "mscorlib",
            "ICSharpCode", "Newtonsoft", "ISharpZipLib", "NAudio", "Unity.TextMeshPro", "PerformanceOptimizer", "NVorbis",
            "com.rlabrecque.steamworks.net", "Assembly-CSharp-firstpass", "CombatAI", "MonoMod", "SmashTools"
        };

        public static HashSet<string> typesToSkip = new()
        {
            "AnimalGenetics.ColonyManager+JobsWrapper", "AutoMachineTool", "NightVision.CombatHelpers",
            "RJWSexperience.UI.SexStatusWindow", "AntimatterAnnihilation.Buildings.Building_MultiRefuelable",
            "AntimatterAnnihilation.Buildings.Building_AlloyFusionMachine",
            "AntimatterAnnihilation.Buildings.Building_CompositeRefiner"
        };

        public static HashSet<string> methodsToSkip = new()
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
            types ??= GenTypes.AllTypes.Where(type => TypeValidator(type)).Distinct().ToList();
            return types;
        }

        public static List<MethodInfo> GetMethodsToParse(Type type)
        {
            List<MethodInfo> methods = new();
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(type))
            {
                if (method != null && !method.IsAbstract)
                {
                    try
                    {
                        string description = method.FullDescription();
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
                List<CodeInstruction> instructions = PatchProcessor.GetOriginalInstructions(method);
                foreach (CodeInstruction instr in instructions)
                {
                    if (instr.operand is MethodInfo mi && mi.IsGenericMethod)
                    {
                        int parameterLength = mi.GetParameters().Length;
                        if (parameterLength == 0)
                        {
                            if (instr.opcode == OpCodes.Callvirt || instr.opcode == OpCodes.Call)
                            {
                                string miName = mi.Name;
                                if (miName == "GetComponent")
                                {
                                    Type underlyingType = mi.GetUnderlyingType();
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
                                else if (miName == "GetCompProperties")
                                {
                                    Type underlyingType = mi.GetUnderlyingType();
                                    if (typeof(CompProperties).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericThingDefCompProps);
                                    }
                                }
                                else if (miName == "CompProps")
                                {
                                    Type underlyingType = mi.GetUnderlyingType();
                                    if (typeof(HediffCompProperties).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericHediffDefCompProps);
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
                                    Type underlyingType = mi.GetUnderlyingType();
                                    if (typeof(HediffComp).IsAssignableFrom(underlyingType))
                                    {
                                        AddPatchInfo(method, instr, underlyingType, genericHediffTryGetComp);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Log.Error("Error when parsing " + method.FullDescription() + " - exception: " + ex);
            }

            void AddPatchInfo(MethodInfo targetMethod, CodeInstruction instr, Type genericType, MethodInfo genericMethod)
            {
                //Log.Message("Patched " + method.FullDescription() + " - instr: " + instr);
                //Log.ResetMessageCount();
                PatchInfo patchInfo = new()
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
            var hugslibHarmonyUtility = AccessTools.TypeByName("HugsLib.Utils.HarmonyUtility");
            if (hugslibHarmonyUtility != null)
            {
                var AppendPatchList = AccessTools.Method(hugslibHarmonyUtility, "HasActivePatches");
                Patch(AppendPatchList, prefix: GetMethod(nameof(StripPOPatchesInfo)));
            }
        }

        public static void StripPOPatchesInfo(ref Patches patches)
        {
            patches = StripPatches(patches);
        }

        private static Patches StripPatches(Patches patches)
        {
            if (patches != null)
            {
                var filtered = patches.Transpilers.Where(x => x.PatchMethod != transpiler).ToList();
                var transpilerList = new ReadOnlyCollection<Patch>(filtered);
                if (transpilerList.Count != patches.Transpilers.Count)
                {
                    Traverse.Create(patches).Field("Transpilers").SetValue(transpilerList);
                }
            }
            return patches;
        }

        public static readonly MethodInfo transpiler = AccessTools.Method(typeof(Optimization_FasterGetCompReplacement), nameof(Optimization_FasterGetCompReplacement.Transpiler));

        public List<KeyValuePair<MethodBase, List<PatchInfo>>> patchesToPerform;
        public async void DoPatchesAsync(bool parse)
        {
            await Task.Run(() =>
            {
                if (parse)
                {
                    ParseEverything();
                }
                PerformanceOptimizerMod.performPatchesPerFrames.optimization = this;
                patchesToPerform = patchInfos.ToList();
            });
            PerformanceOptimizerMod.performPatchesPerFrames.StartCoroutine(PerformanceOptimizerMod.performPatchesPerFrames.PerformPatches());
        }

        private void ParseEverything()
        {
            try
            {
                HashSet<MethodInfo> methodsToParse = new();
                List<Type> types = GetTypesToParse();
                foreach (Type type in types)
                {
                    foreach (MethodInfo method in GetMethodsToParse(type))
                    {
                        methodsToParse.Add(method);
                    }
                }
                foreach (MethodInfo method in methodsToParse)
                {
                    ParseMethod(method);
                }
            }
            catch (Exception)
            {
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
        {
            List<PatchInfo> curPatchInfos = patchInfos[method];
            foreach (CodeInstruction instr in instructions)
            {
                for (int j = 0; j < curPatchInfos.Count; j++)
                {
                    PatchInfo patchInfo = curPatchInfos[j];
                    if (patchInfo.targetInstruction.opcode == instr.opcode && patchInfo.targetInstruction.operand == instr.operand)
                    {
                        instr.opcode = OpCodes.Call;
                        instr.operand = patchInfo.methodToReplace;
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
            clearMethods ??= GetClearMethods().ToList();
            foreach (MethodInfo method in clearMethods)
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
            foreach (Type type in typeof(HediffComp).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_HediffComp<>), type);
            }
            foreach (Type type in typeof(WorldObjectComp).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_WorldObjectComp<>), type);
            }
            foreach (Type type in typeof(GameComponent).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_GameComponent<>), type);
            }
            foreach (Type type in typeof(WorldComponent).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_WorldComponent<>), type);
            }
            foreach (Type type in typeof(MapComponent).AllSubclasses())
            {
                yield return ClearMethod(typeof(ICache_MapComponent<>), type);
            }

            static MethodInfo ClearMethod(Type genericType, Type genericParam)
            {
                return genericType.MakeGenericType(genericParam).GetMethod("Clear", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class ComponentCache
    {

        //Attentions modders! If you remove or add comps from things manually, you can reset the comp cache by calling this method below. Example:
        //public static MethodInfo ComponentCache_ResetCompCache_Info = AccessTools.Method("PerformanceOptimizer.ComponentCache:ResetCompCache");
        //ComponentCache_ResetCompCache_Info?.Invoke(null, new object[] { thing });

        private static readonly Dictionary<Type, MethodInfo> cachedMethods = new();

        public static class ICache_HediffComp<T> where T : HediffComp
        {
            public static Dictionary<int, T> compsById = new();
            public static void Clear()
            {
                compsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TryGetHediffCompFast<T>(this Hediff hd) where T : HediffComp
        {
            if (hd is null) return null;
            if (ICache_HediffComp<T>.compsById.TryGetValue(hd.loadID, out T val))
            {
                return val;
            }
            if (hd is not HediffWithComps hediffWithComps)
            {
                return null;
            }
            if (hediffWithComps.comps == null)
            {
                return null;
            }

            for (int i = 0; i < hediffWithComps.comps.Count; i++)
            {
                HediffCompProperties props = hediffWithComps.comps[i].props;
                if (props != null && props.compClass == typeof(T))
                {
                    T val2 = hediffWithComps.comps[i] as T;
                    ICache_HediffComp<T>.compsById[hd.loadID] = val2;
                    return val2;
                }
            }

            for (int i = 0; i < hediffWithComps.comps.Count; i++)
            {
                if (hediffWithComps.comps[i] is T val3)
                {
                    ICache_HediffComp<T>.compsById[hd.loadID] = val3;
                    return val3;
                }
            }
            return null;
        }
        public static class ICache_WorldObjectComp<T> where T : WorldObjectComp
        {
            public static Dictionary<int, T> compsById = new();
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
                    T val2 = worldObject.comps[i] as T;
                    ICache_WorldObjectComp<T>.compsById[worldObject.ID] = val2;
                    return val2;
                }
            }

            for (int i = 0; i < worldObject.comps.Count; i++)
            {
                if (worldObject.comps[i] is T val3)
                {
                    ICache_WorldObjectComp<T>.compsById[worldObject.ID] = val3;
                    return val3;
                }
            }
            return null;
        }
        public static class ICache_ThingDefProps<T> where T : CompProperties
        {
            public static Dictionary<ushort, T> compPropsById = new();
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
                if (thingDef.comps[i] is T val3)
                {
                    ICache_ThingDefProps<T>.compPropsById[thingDef.shortHash] = val3;
                    return val3;
                }
            }
            return null;
        }
        public static class ICache_HediffDefProps<T> where T : HediffCompProperties
        {
            public static Dictionary<ushort, T> compPropsById = new();
            public static void Clear()
            {
                compPropsById.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetHediffDefPropsFast<T>(this HediffDef hediffDef) where T : HediffCompProperties
        {
            if (hediffDef.comps == null)
            {
                return null;
            }

            if (ICache_HediffDefProps<T>.compPropsById.TryGetValue(hediffDef.shortHash, out T val))
            {
                return val;
            }

            for (int i = 0; i < hediffDef.comps.Count; i++)
            {
                if (hediffDef.comps[i] is T val3)
                {
                    ICache_HediffDefProps<T>.compPropsById[hediffDef.shortHash] = val3;
                    return val3;
                }
            }
            return null;
        }


        public static class ICache_DefModExtension<T> where T : DefModExtension
        {
            public static Dictionary<ushort, T> modExtensionsById = new();
            public static void Clear()
            {
                modExtensionsById.Clear();
            }
        }

        public static class ICache_MapComponent<T>
        {
            public static Dictionary<Map, T> compsByMap = new();
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
                T comp = map.GetComponent<T>();
                if (comp != null)
                {
                    ICache_MapComponent<T>.compsByMap[map] = mapComp = comp;
                }
            }
            //Log.Message("Fresh Getting: " + map.GetComponent<T>() + " - " + map.GetComponent<T>()?.GetHashCode());
            //Log.Message("Saved Getting: " + mapComp + " - " + mapComp?.GetHashCode());
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
                    T comp = curWorld.GetComponent<T>();
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
                    T comp = curGame.GetComponent<T>();
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
