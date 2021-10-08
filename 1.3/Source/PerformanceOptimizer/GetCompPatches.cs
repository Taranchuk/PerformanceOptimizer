using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace PerformanceOptimizer
{
    public static class GetCompPatches
    {
        public static HashSet<string> assembliesToSkip = new HashSet<string>
        {
            "System", "Cecil", "Multiplayer", "Prepatcher", "HeavyMelee", "0Harmony", "UnityEngine", "mscorlib", "ICSharpCode", "Newtonsoft", "TranspilerExplorer"
        };

        public static HashSet<string> methodsToSkip = new HashSet<string>
        {
            "Numbers.MainTabWindow_Numbers", "Numbers.OptionsMaker", "<PawnSelector>g__Action", "<AllBuildingsColonistWithComp>", "<FailOnOwnerStatus>"
        };
        private static bool TypeValidator(Type type) => !assembliesToSkip.Any(asmName => type.Assembly?.FullName?.Contains(asmName) ?? false);

        private static List<Type> types;
        public static List<Type> GetTypesToParse()
        {
            if (types is null)
            {
                types = GenTypes.AllTypes.Where(type => TypeValidator(type)).Distinct().ToList();
            }
            return types;
        }

        public static IEnumerable<MethodInfo> GetMethodsToParse(this Type type)
        {
            return AccessTools.GetDeclaredMethods(type).Where(predicate: mi => mi != null && !mi.IsAbstract && !mi.IsGenericMethod && !methodsToSkip.Any(x => mi.FullDescription().Contains(x)));
        }

        public static List<MethodInfo> methodsCallingMapGetComp = new List<MethodInfo>();
        public static List<MethodInfo> methodsCallingWorldGetComp = new List<MethodInfo>();
        public static List<MethodInfo> methodsCallingGameGetComp = new List<MethodInfo>();
        public static List<MethodInfo> methodsCallingThingGetComp = new List<MethodInfo>();
        public static List<MethodInfo> methodsCallingThingTryGetComp = new List<MethodInfo>();
        public static void ParseMethod(MethodInfo method)
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
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private static Stopwatch totalSW = new Stopwatch();
        private static Stopwatch curSW = new Stopwatch();
        public static List<MethodInfo> methodsToParse = new List<MethodInfo>();
        public static async void DoPatchesAsync()
        {
            totalSW.Restart();
            await Task.Run(() => 
            {
                curSW.Restart();
                var types = GetTypesToParse();
                curSW.LogTime("Collected types: ", 0);
                curSW.Restart();
                foreach (var type in types)
                {
                    foreach (var method in type.GetMethodsToParse())
                    {
                        methodsToParse.Add(method);
                    }
                }
                curSW.LogTime("Methods added to be parsed: ", 0);
                curSW.Restart();

                for (var i = 0; i < methodsToParse.Count; i++)
                {
                    ParseMethod(methodsToParse[i]);
                }
                curSW.LogTime("Methods parsed: ", 0);
            });

            curSW.Restart();
            Patch(methodsCallingMapGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetMapCompTranspiler))));
            Patch(methodsCallingWorldGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetWorldCompTranspiler))));
            Patch(methodsCallingGameGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetGameCompTranspiler))));
            Patch(methodsCallingThingGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetThingCompTranspiler))));
            Patch(methodsCallingThingTryGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.TryGetThingCompTranspiler))));
            curSW.LogTime("Patched methods: ", 0);
            curSW.Restart();
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
                PerformanceOptimizerMain.harmony.Patch(hook, null, new HarmonyMethod(typeof(ComponentCache), "ResetComps"));
            }

            curSW.LogTime("Patched hooks: ", 0);
            curSW.Stop();

            totalSW.LogTime("Parsed and Transpiled for get comp replacement, total patched: " + patchedMethodsCount + ", parsed methods: " + methodsToParse.Count + ", total time: ", 0);
            totalSW.Stop();
        }

        private static int patchedMethodsCount = 0;
        private static void Patch(List<MethodInfo> methodsToPatch, HarmonyMethod transpiler)
        {
            foreach (var method in methodsToPatch)
            {
                try
                {
                    PerformanceOptimizerMain.harmony.Patch(method, transpiler: transpiler);
                    patchedMethodsCount++;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.GetType() + " - Error in transpiling: " + method.FullDescription() + ", type: " + method.DeclaringType + ", exception: " + ex + " - InnerException: " + ex.InnerException);
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
            public static List<ThingDef> reportedDefs = new List<ThingDef>();
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
    }
}
