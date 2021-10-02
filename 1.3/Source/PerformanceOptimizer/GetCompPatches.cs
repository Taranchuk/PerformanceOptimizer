using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PerformanceOptimizer
{
    [StaticConstructorOnStartup]
    public static class GetCompPatches
    {
        public static HashSet<string> mostCalledComps = new HashSet<string>
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

        public static HashSet<string> assembliesToSkip = new HashSet<string>
        {
            "System", "Cecil", "Multiplayer", "Prepatcher", "HeavyMelee", "0Harmony", "UnityEngine", "mscorlib", "ICSharpCode", "Newtonsoft", "TranspilerExplorer"
        };

        public static HashSet<string> typesToSkip = new HashSet<string>
        {
            "ICSharpCode", "Newtonsoft", "TranspilerExplorer", "Ionic"
        };
        private static List<Type> types;
        public static List<Type> GetTypesToParse()
        {
            if (types is null)
            {
                types = GenTypes.AllTypes.Where(type => TypeValidator(type)).Distinct().ToList();
            }
            return types;
        }

        private static bool TypeValidator(Type type) => !assembliesToSkip.Any(asmName => type.Assembly?.FullName?.Contains(asmName) ?? false)
                                                     && !typesToSkip.Any(typeName => type.Name.Contains(typeName));

        public static IEnumerable<MethodInfo> GetValidMethods(this Type type)
        {
            return type.GetMethods().Where(predicate: mi => mi != null && !mi.IsAbstract && !mi.IsVirtual && !mi.IsGenericMethod && TypeValidator(mi.DeclaringType)).Distinct();
        }

        private static Stopwatch total = new Stopwatch();

        private static Harmony harmony;
        static GetCompPatches()
        {
            harmony = new Harmony("PerformanceOptimizer.Patches");
            harmony.PatchAll();
            total.Restart();
            var methodsCallingMapGetComp = new HashSet<MethodInfo>();
            var methodsCallingWorldGetComp = new HashSet<MethodInfo>();
            var methodsCallingGameGetComp = new HashSet<MethodInfo>();
            var methodsCallingThingGetComp = new HashSet<MethodInfo>();
            var methodsCallingThingTryGetComp = new HashSet<MethodInfo>();
            var types = GetTypesToParse();
            var methodsToParse = new HashSet<MethodInfo>();
            foreach (var type in types)
            {
                foreach (var method in type.GetValidMethods())
                {
                    methodsToParse.Add(method);
                }
            }

            //var mapGetComp = AccessTools.GetDeclaredMethods(typeof(Map)).FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "GetComponent" && mi.GetParameters().Length == 0);
            //var worldGetComp = AccessTools.GetDeclaredMethods(typeof(World)).FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "GetComponent" && mi.GetParameters().Length == 0);
            //var gameGetComp = AccessTools.GetDeclaredMethods(typeof(Game)).FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "GetComponent" && mi.GetParameters().Length == 0);
            //var thingGetComp = AccessTools.GetDeclaredMethods(typeof(ThingWithComps)).FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "GetComp" && mi.GetParameters().Length == 0);
            //var thingTryGetComp = AccessTools.GetDeclaredMethods(typeof(ThingCompUtility)).FirstOrDefault(mi => mi.IsGenericMethod && mi.Name == "TryGetComp" && mi.GetParameters().Length == 1);
            
            //Log.Message("mapGetComp: " + mapGetComp);
            //Log.Message("worldGetComp: " + mapGetComp);
            //Log.Message("gameGetComp: " + gameGetComp);
            //Log.Message("thingGetComp: " + thingGetComp);
            //Log.Message("thingTryGetComp: " + thingTryGetComp);

            foreach (var method in methodsToParse)
            {
                try
                {
                    var instructions = PatchProcessor.GetOriginalInstructions(method);
                    foreach (var instr in instructions)
                    {
                        // this following code doesn't work... instruments don't recognize methods, maybe because they are generic methods splitted by types
                        //if (instr.Calls(mapGetComp))
                        //{
                        //    methodsCallingMapGetComp.Add(method);
                        //}
                        //else if (instr.Calls(worldGetComp))
                        //{
                        //    methodsCallingWorldGetComp.Add(method);
                        //}
                        //else if (instr.Calls(gameGetComp))
                        //{
                        //    methodsCallingGameGetComp.Add(method);
                        //}
                        //else if (instr.Calls(thingGetComp))
                        //{
                        //    methodsCallingThingGetComp.Add(method);
                        //}
                        //else if (instr.Calls(thingTryGetComp))
                        //{
                        //    methodsCallingThingTryGetComp.Add(method);
                        //}

                        // this following code works, although it's a bit clustefuck
                        if (instr.operand is MethodInfo mi)
                        {
                            if (mi.IsGenericMethod && mi.GetParameters().Length <= 0)
                            {
                                if (instr.opcode == OpCodes.Callvirt)
                                {
                                    if (mi.Name == "GetComponent")
                                    {
                                        var underlyingType = mi.GetUnderlyingType();
                                        if (typeof(MapComponent).IsAssignableFrom(underlyingType))
                                        {
                                            methodsCallingMapGetComp.Add(method);
                                        }
                                        else if (typeof(GameComponent).IsAssignableFrom(underlyingType))
                                        {
                                            methodsCallingGameGetComp.Add(method);
                                        }
                                        else if (typeof(WorldComponent).IsAssignableFrom(underlyingType))
                                        {
                                            methodsCallingWorldGetComp.Add(method);
                                        }
                                    }
                                    else if (mi.Name == "GetComp")
                                    {
                                        var underlyingType = mi.GetUnderlyingType();
                                        if (typeof(ThingComp).IsAssignableFrom(underlyingType))
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
                                        if (typeof(ThingComp).IsAssignableFrom(underlyingType))
                                        {
                                            methodsCallingThingTryGetComp.Add(method);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            Patch(methodsCallingMapGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetMapCompTranspiler))));
            Patch(methodsCallingWorldGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetWorldCompTranspiler))));
            Patch(methodsCallingGameGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetGameCompTranspiler))));
            Patch(methodsCallingThingGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetThingCompTranspiler))));
            Patch(methodsCallingThingTryGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.TryGetThingCompTranspiler))));

            var hooks = new List<MethodInfo>
            {
                AccessTools.Method(typeof(MapDeiniter), "Deinit"),
                AccessTools.Method(typeof(Game), "AddMap"),
                AccessTools.Method(typeof(World), "FillComponents"),
                AccessTools.Method(typeof(Game), "FillComponents"),
                AccessTools.Method(typeof(MapComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(WorldComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(GameComponentUtility), "FinalizeInit"),
                AccessTools.Method(typeof(Game), "InitNewGame"),
                AccessTools.Method(typeof(Game), "LoadGame"),
            };
            foreach (var hook in hooks)
            {
                harmony.Patch(hook, null, new HarmonyMethod(typeof(ComponentCache), "ResetComps"));
            }

            var elapsed = (float)total.ElapsedTicks / Stopwatch.Frequency;
            Log.Message("Parsed and Transpiled for get comp replacement: " + elapsed);
            total.Stop();
        }

        [HarmonyPatch(typeof(MapComponentUtility), "FinalizeInit")]
        public class FinalizeInit_Patch
        {
            public static void Postfix(Map map)
            {
                Log.Message("Loaded map: " + map + ", it has " + map.components.Count + " comps");
            }
        }
        [HarmonyPatch(typeof(WorldComponentUtility), "FinalizeInit")]
        public class World_Patch
        {
            public static void Postfix(World world)
            {
                Log.Message("Loaded world: " + world + ", it has " + world.components.Count + " comps");
            }
        }

        [HarmonyPatch(typeof(GameComponentUtility), "FinalizeInit")]
        public class GameComponentUtility_Patch
        {
            public static void Postfix()
            {
                Log.Message("Loaded game: " + Current.Game + ", it has " + Current.Game.components.Count + " comps");
            }
        }

        [HarmonyPatch(typeof(ThingWithComps), "SpawnSetup")]
        public static class Thing_SpawnSetup_Patch
        {
            public static HashSet<ThingDef> reportedDefs = new HashSet<ThingDef>();
            public static void Postfix(ThingWithComps __instance)
            {
                if (__instance.comps?.Count > 0 && !reportedDefs.Contains(__instance.def))
                {
                    reportedDefs.Add(__instance.def);
                    Log.Message("Loaded ThingWithComps: " + __instance.def + ", it has " + __instance.comps?.Count + " comps");
                }
            }
        }

        [HarmonyPatch(typeof(ThingWithComps), "InitializeComps")]
        public static class Thing_InitializeComps_Patch
        {
            public static void Postfix(ThingWithComps __instance)
            {
                if (__instance.comps != null)
                {
                    __instance.comps = __instance.comps.OrderBy(delegate(ThingComp x) 
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
        }

        private static void Patch(HashSet<MethodInfo> methodsToPatch, HarmonyMethod transpiler)
        {
            foreach (var method in methodsToPatch)
            {
                //Log.Message("Patching " + method.DeclaringType.ToString() + " - " + method.ToString() + " - is generic: " + method.IsGenericMethod + ", is virtual: " + method.IsVirtual
                //    + " - is type generic: " + method.DeclaringType.IsGenericType + ", is type abstract: " + method.DeclaringType.IsAbstract);
                try
                {
                    harmony.Patch(method, transpiler: transpiler);
                }
                catch (Exception ex)
                {
                    //Log.Error("Error in transpiling: " + method + ", exception: " + ex);
                }
            }
        }

        public static MethodInfo genericMapGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetMapComponent));
        private static IEnumerable<CodeInstruction> GetMapCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComponent", typeof(MapComponent), genericMapGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        public static MethodInfo genericWorldGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldComponent));
        private static IEnumerable<CodeInstruction> GetWorldCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComponent", typeof(WorldComponent), genericWorldGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        public static MethodInfo genericGameGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetGameComponent));
        private static IEnumerable<CodeInstruction> GetGameCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComponent", typeof(GameComponent), genericGameGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        //public static MethodInfo genericThingGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetCompVanilla));
        public static MethodInfo genericThingGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetThingCompDict));
        private static IEnumerable<CodeInstruction> GetThingCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComp", typeof(ThingComp), genericThingGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        //public static MethodInfo genericThingTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetCompVanilla));
        public static MethodInfo genericThingTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetCompDict));
        private static IEnumerable<CodeInstruction> TryGetThingCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("TryGetComp", typeof(ThingComp), genericThingTryGetComp, OpCodes.Call, 1, instructions, source, il);
        }
        public static bool CallsComponent(this CodeInstruction codeInstruction, OpCode opcode, string methodName, Type baseCompType, int parameterLength, out Type curMapCompType)
        {
            if (codeInstruction.opcode == opcode && codeInstruction.operand is MethodInfo mi && mi.Name == methodName && mi.IsGenericMethod && mi.GetParameters().Length == parameterLength)
            {
                curMapCompType = mi.GetUnderlyingType();
                if (baseCompType.IsAssignableFrom(curMapCompType))
                {
                    return true;
                }
            }
            curMapCompType = null;
            return false;
        }

        private static IEnumerable<CodeInstruction> PerformTranspiler(string methodName, Type baseType, MethodInfo genericMethod, OpCode opcode, int parameterLength, IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (instr.CallsComponent(opcode, methodName, baseType, parameterLength, out Type curMapCompType))
                {
                    var methodToReplace = genericMethod.MakeGenericMethod(new Type[] { curMapCompType });
                    instr.opcode = OpCodes.Call;
                    instr.operand = methodToReplace;
                }
                yield return instr;
            }
        }
        public static void StartLog(this Stopwatch stopwatch)
        {
            stopwatch.Restart();
        }

        public class StopwatchData
        {
            public float total;
            public float count;
        }

        private static Dictionary<Stopwatch, StopwatchData> stopwatches = new Dictionary<Stopwatch, StopwatchData>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogTime(this Stopwatch stopwatch, string log)
        {
            if (!stopwatches.TryGetValue(stopwatch, out var stats))
            {
                stopwatches[stopwatch] = stats = new StopwatchData();
            }

            var elapsed = (float)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            stats.count++;
            stats.total += elapsed;

            if (stats.count > 999999)
            {
                Log.Message(log + "it took (" + stats.count + "): " + stats.total);
                foreach (var data in ComponentCache.calledStats.OrderByDescending(x => x.Value))
                {
                    Log.Message("Called: " + data.Key + " - " + data.Value);
                }
                stats.total = 0;
                stats.count = 0;
            }
        }
    }
}
