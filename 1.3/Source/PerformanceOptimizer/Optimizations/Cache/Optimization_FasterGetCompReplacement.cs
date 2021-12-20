using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using static PerformanceOptimizer.ComponentCache;

namespace PerformanceOptimizer
{
    public class Optimization_FasterGetCompReplacement : Optimization
    {
        public override int DrawOrder => -99999;
        public List<MethodInfo> methodsCallingMapGetComp;
        public List<MethodInfo> methodsCallingWorldGetComp;
        public List<MethodInfo> methodsCallingGameGetComp;
        public List<MethodInfo> methodsCallingThingGetComp;
        public List<MethodInfo> methodsCallingThingTryGetComp;
        public List<MethodInfo> methodsCallingHediffTryGetComp;
        public List<MethodInfo> methodsCallingWorldObjectGetComp;

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
        public static void ParseMethod(MethodInfo method, List<MethodInfo> methodsCallingMapGetComp, List<MethodInfo> methodsCallingWorldGetComp, List<MethodInfo> methodsCallingGameGetComp,
            List<MethodInfo> methodsCallingThingGetComp, List<MethodInfo> methodsCallingThingTryGetComp, List<MethodInfo> methodsCallingHediffTryGetComp
            , List<MethodInfo> methodsCallingWorldObjectGetComp)
        {
            try
            {
                var instructions = PatchProcessor.GetCurrentInstructions(method);
                foreach (var instr in instructions)
                {
                    if (instr.operand is MethodInfo mi)
                    {
                        if (mi.IsGenericMethod && mi.GetParameters().Length <= 0)
                        {
                            if (instr.opcode == OpCodes.Callvirt)
                            {
                                if (mi.Name == "GetComponent")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (!methodsCallingMapGetComp.Contains(method) && typeof(MapComponent).IsAssignableFrom(underlyingType))
                                    {
                                        methodsCallingMapGetComp.Add(method);
                                    }
                                    else if (!methodsCallingGameGetComp.Contains(method) && typeof(GameComponent).IsAssignableFrom(underlyingType))
                                    {
                                        methodsCallingGameGetComp.Add(method);
                                    }
                                    else if (!methodsCallingWorldGetComp.Contains(method) && typeof(WorldComponent).IsAssignableFrom(underlyingType))
                                    {
                                        methodsCallingWorldGetComp.Add(method);
                                    }
                                    else if (!methodsCallingWorldObjectGetComp.Contains(method) && typeof(WorldObjectComp).IsAssignableFrom(underlyingType))
                                    {
                                        methodsCallingWorldObjectGetComp.Add(method);
                                    }
                                }
                                else if (mi.Name == "GetComp")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (!methodsCallingThingGetComp.Contains(method) && typeof(ThingComp).IsAssignableFrom(underlyingType))
                                    {
                                        methodsCallingThingGetComp.Add(method);
                                    }
                                }
                            }
                        }
                        else if (mi.IsGenericMethod && mi.GetParameters().Length == 1)
                        {
                            if (instr.opcode == OpCodes.Call)
                            {
                                if (mi.Name == "TryGetComp")
                                {
                                    var underlyingType = mi.GetUnderlyingType();
                                    if (!methodsCallingThingTryGetComp.Contains(method) && typeof(ThingComp).IsAssignableFrom(underlyingType))
                                    {
                                        methodsCallingThingTryGetComp.Add(method);
                                    }
                                    else if (!methodsCallingHediffTryGetComp.Contains(method) && typeof(HediffComp).IsAssignableFrom(underlyingType))
                                    {
                                        methodsCallingHediffTryGetComp.Add(method);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        public static MethodInfo genericMapGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetMapComponentDict));
        private static IEnumerable<CodeInstruction> GetMapCompTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return PerformTranspiler("GetComponent", typeof(MapComponent), genericMapGetComp, OpCodes.Callvirt, 0, instructions);
        }

        public static MethodInfo genericWorldGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldComponentDict));
        private static IEnumerable<CodeInstruction> GetWorldCompTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return PerformTranspiler("GetComponent", typeof(WorldComponent), genericWorldGetComp, OpCodes.Callvirt, 0, instructions);
        }

        public static MethodInfo genericGameGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetGameComponentDict));
        private static IEnumerable<CodeInstruction> GetGameCompTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return PerformTranspiler("GetComponent", typeof(GameComponent), genericGameGetComp, OpCodes.Callvirt, 0, instructions);
        }

        public static MethodInfo genericThingGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetThingCompFast));
        private static IEnumerable<CodeInstruction> GetThingCompTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return PerformTranspiler("GetComp", typeof(ThingComp), genericThingGetComp, OpCodes.Callvirt, 0, instructions);
        }

        public static MethodInfo genericThingTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetThingCompFast));
        private static IEnumerable<CodeInstruction> TryGetThingCompTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return PerformTranspiler("TryGetComp", typeof(ThingComp), genericThingTryGetComp, OpCodes.Call, 1, instructions);
        }

        public static MethodInfo genericHediffTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetHediffCompFast));
        private static IEnumerable<CodeInstruction> TryGetHediffCompTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return PerformTranspiler("TryGetComp", typeof(HediffComp), genericHediffTryGetComp, OpCodes.Call, 1, instructions);
        }

        public static MethodInfo genericWorldObjectGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldObjectCompFast));
        private static IEnumerable<CodeInstruction> GetWorldObjectCompTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return PerformTranspiler("GetComponent", typeof(WorldObjectComp), genericWorldObjectGetComp, OpCodes.Callvirt, 0, instructions);
        }
        public override OptimizationType OptimizationType => OptimizationType.Optimization;

        public override string Label => "PO.FasterGetCompReplacement".Translate();

        public override void DoPatches()
        {
            base.DoPatches();
            Patch(typeof(ThingWithComps), "InitializeComps", postfix: GetMethod(nameof(InitializeCompsPostfix)));
            bool parse = false;
            if (methodsCallingMapGetComp is null)
            {
                methodsCallingMapGetComp = new List<MethodInfo>();
                methodsCallingWorldGetComp = new List<MethodInfo>();
                methodsCallingGameGetComp = new List<MethodInfo>();
                methodsCallingThingGetComp = new List<MethodInfo>();
                methodsCallingThingTryGetComp = new List<MethodInfo>();
                methodsCallingHediffTryGetComp = new List<MethodInfo>();
                methodsCallingWorldObjectGetComp = new List<MethodInfo>();
                parse = true;
            }
            DoPatchesAsync(parse);
        }
        public async void DoPatchesAsync(bool parse)
        {
            if (parse)
            {
                var methodsToParse = new HashSet<MethodInfo>();
                await Task.Run(() =>
                {
                    try
                    {
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
                            ParseMethod(method, methodsCallingMapGetComp, methodsCallingWorldGetComp, methodsCallingGameGetComp, methodsCallingThingGetComp,
                                methodsCallingThingTryGetComp, methodsCallingHediffTryGetComp, methodsCallingWorldObjectGetComp);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Exception in Performance Optimizer: " + ex);
                    }
                });
            }

            foreach (var method in methodsCallingGameGetComp)
            {
                Patch(method, transpiler: GetMethod(nameof(Optimization_FasterGetCompReplacement.GetGameCompTranspiler)));
            }
            foreach (var method in methodsCallingWorldGetComp)
            {
                Patch(method, transpiler: GetMethod(nameof(Optimization_FasterGetCompReplacement.GetWorldCompTranspiler)));
            }
            foreach (var method in methodsCallingWorldObjectGetComp)
            {
                Patch(method, transpiler: GetMethod(nameof(Optimization_FasterGetCompReplacement.GetWorldObjectCompTranspiler)));
            }
            foreach (var method in methodsCallingMapGetComp)
            {
                Patch(method, transpiler: GetMethod(nameof(Optimization_FasterGetCompReplacement.GetMapCompTranspiler)));
            }
            foreach (var method in methodsCallingThingGetComp)
            {
                Patch(method, transpiler: GetMethod(nameof(Optimization_FasterGetCompReplacement.GetThingCompTranspiler)));
            }
            foreach (var method in methodsCallingThingTryGetComp)
            {
                Patch(method, transpiler: GetMethod(nameof(Optimization_FasterGetCompReplacement.TryGetThingCompTranspiler)));
            }
            foreach (var method in methodsCallingHediffTryGetComp)
            {
                Patch(method, transpiler: GetMethod(nameof(Optimization_FasterGetCompReplacement.TryGetHediffCompTranspiler)));
            }
        }

        public static bool CallsComponent(CodeInstruction codeInstruction, OpCode opcode, string methodName, Type baseCompType, int parameterLength, out Type curType)
        {
            if (codeInstruction.opcode == opcode && codeInstruction.operand is MethodInfo mi && mi.Name == methodName)
            {
                if (mi.IsGenericMethod && mi.GetParameters().Length == parameterLength)
                {
                    curType = mi.GetUnderlyingType();
                    if (baseCompType.IsAssignableFrom(curType))
                    {
                        return true;
                    }
                }
            }
            curType = null;
            return false;
        }

        private static IEnumerable<CodeInstruction> PerformTranspiler(string methodName, Type baseType, MethodInfo genericMethod, OpCode opcode, int parameterLength,
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (CallsComponent(instr, opcode, methodName, baseType, parameterLength, out Type type))
                {
                    var methodToReplace = genericMethod.MakeGenericMethod(new Type[] { type });
                    instr.opcode = OpCodes.Call;
                    instr.operand = methodToReplace;
                }
                yield return instr;
            }
        }

        public static void InitializeCompsPostfix(ThingWithComps __instance)
        {
            if (__instance.comps != null && __instance.def.plant is null && !__instance.def.saveCompressible && __instance.comps.Count > 1)
            {
                __instance.comps = __instance.comps.OrderBy(delegate (ThingComp x)
                {
                    var index = mostCalledComps.FirstIndexOf(y => y == x.GetType().ToString());
                    if (index == -1)
                    {
                        return 99999;
                    }
                    return index;
                }).ToList();
            }
        }

        public override void Clear()
        {
            ComponentCache.cachedWorldComps.Clear();
            ComponentCache.cachedGameComps.Clear();
            CompsOfType<Map>.mapCompsByMap.Clear();
            foreach (var type in typeof(ThingComp).AllSubclasses())
            {
                GenGeneric.InvokeStaticMethodOnGenericType(typeof(ICache_ThingComp<>), type, "Clear");
            }
        }

        public static List<string> mostCalledComps = new List<string>
            {
                "RimWorld.CompQuality",
                "Verse.CompAttachBase",
                "Verse.CompEquippable",
                "AlienRace.AlienPartGenerator+AlienComp",
                "RimWorld.CompForbiddable",
                "RimWorld.CompRottable",
                "aRandomKiwi.GFM.Comp_Guard",
                "RimWorld.CompBreakdownable",
                "CaravanAdventures.CaravanStory.CompTalk",
                "RimWorld.CompStyleable",
                "LWM.DeepStorage.CompDeepStorage",
                "RimWorld.CompPowerTrader",
                "RimWorld.CompFlickable",
                "VFECore.Abilities.CompAbilities",
                "RimWorld.CompFacility",
                "BestMix.CompBestMix",
                "RimWorld.CompBiocodable",
                "CombatExtended.CompAmmoUser",
                "VFESecurity.CompPawnTracker",
                "RimWorld.CompCanBeDormant",
                "CombatExtended.CompInventory",
                "RimWorld.CompIngredients",
                "VFEV.Facepaint.CompFacepaint",
                "RimWorld.CompDrug",
                "RimWorld.CompEggLayer",
                "RimWorld.CompAffectedByFacilities",
                "RimWorld.CompSpawnSubplant",
                "Verse.CompColorable",
                "RimWorld.CompArt",
                "PickUpAndHaul.CompHauledToInventory",
                "CommonSense.CompUnloadChecker",
                "Dark.Signs.Comp_Sign",
                "Locks2.Core.LockComp",
                "RimWorld.CompBecomeBuilding",
                "Hospitality.CompGuest",
                "CombatExtended.CompTacticalManager",
                "RimWorld.CompReloadable",
                "RimWorld.CompTransporter",
                "DubsBadHygiene.CompBlockage",
            };
    }

    public static class CompsOfType<T>
    {
        public static Dictionary<Map, T> mapCompsByMap = new Dictionary<Map, T>();
    }

    [StaticConstructorOnStartup]
    public static class ComponentCache
    {
        private static Stopwatch dictStopwatch = new Stopwatch();
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
                ICache_ThingComp<T>.compsById[thingWithComps.thingIDNumber] = null;
                return null;
            }
            for (int i = 0; i < thingWithComps.comps.Count; i++)
            {
                if (thingWithComps.comps[i].GetType() == typeof(T))
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
            ICache_ThingComp<T>.compsById[thingWithComps.thingIDNumber] = null;
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetWorldObjectCompFast<T>(this WorldObject worldObject) where T : WorldObjectComp
        {
            //dictStopwatch.Restart();
            if (worldObject.comps == null)
            {
                //dictStopwatch.LogTime("Dict approach: ");
                return null;
            }
            for (int i = 0; i < worldObject.comps.Count; i++)
            {
                if (worldObject.comps[i].GetType() == typeof(T))
                {
                    //RegisterComp(thingWithComps.comps[i].GetType());
                    //dictStopwatch.LogTime("Dict approach: ");
                    return worldObject.comps[i] as T;
                }
            }

            for (int i = 0; i < worldObject.comps.Count; i++)
            {
                T val = worldObject.comps[i] as T;
                if (val != null)
                {
                    return val;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TryGetHediffCompFast<T>(this Hediff hd) where T : HediffComp
        {
            HediffWithComps hediffWithComps = hd as HediffWithComps;
            if (hediffWithComps == null)
            {
                return null;
            }
            //dictStopwatch.Restart();
            if (hediffWithComps.comps == null)
            {
                //dictStopwatch.LogTime("Dict approach: ");
                return null;
            }

            for (int i = 0; i < hediffWithComps.comps.Count; i++)
            {
                if (hediffWithComps.comps[i].GetType() == typeof(T))
                {
                    //RegisterComp(thingWithComps.comps[i].GetType());
                    //dictStopwatch.LogTime("Dict approach: ");
                    return hediffWithComps.comps[i] as T;
                }
            }

            for (int i = 0; i < hediffWithComps.comps.Count; i++)
            {
                T val = hediffWithComps.comps[i] as T;
                if (val != null)
                {
                    return val;
                }
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetMapComponentDict<T>(this Map map) where T : MapComponent
        {
            if (!CompsOfType<T>.mapCompsByMap.TryGetValue(map, out T mapComp) || mapComp is null)
            {
                CompsOfType<T>.mapCompsByMap[map] = mapComp = map.GetComponent<T>();
            }
            //Log.Message("Returning map comp: " + mapComp + ", total count of map comps is " + map.components.Count);
            return mapComp;
        }

        public static Dictionary<Type, WorldComponent> cachedWorldComps = new Dictionary<Type, WorldComponent>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetWorldComponentDict<T>(this World world) where T : WorldComponent
        {
            var type = typeof(T);
            if (!cachedWorldComps.TryGetValue(type, out var worldComp) || worldComp is null)
            {
                cachedWorldComps[type] = worldComp = world.GetComponent<T>();
            }
            //Log.Message("Returning world comp: " + worldComp + ", total count of world comps is " + world.components.Count);
            return worldComp as T;
        }

        public static Dictionary<Type, GameComponent> cachedGameComps = new Dictionary<Type, GameComponent>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetGameComponentDict<T>(this Game game) where T : GameComponent
        {
            var type = typeof(T);
            if (!cachedGameComps.TryGetValue(type, out var gameComp) || gameComp is null)
            {
                cachedGameComps[type] = gameComp = game.GetComponent<T>();
            }
            //Log.Message("Returning game comp: " + gameComp + ", total count of game comps is " + game.components.Count);
            return gameComp as T;
        }
    }
}
