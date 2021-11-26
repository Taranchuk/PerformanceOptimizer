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

        public static HashSet<string> typesToSkip = new HashSet<string>
        {
            "AnimalGenetics.ColonyManager+JobsWrapper", "AutoMachineTool", "GearUpAndGo.SetBetterPawnControl", "AlteredCarbon.ModCompatibility", "TorannMagic.ModCheck"
        };

        public static HashSet<string> methodsToSkip = new HashSet<string>
        {
            "Numbers.MainTabWindow_Numbers", "Numbers.OptionsMaker", "<PawnSelector>g__Action", "<AllBuildingsColonistWithComp>", "<FailOnOwnerStatus>", "Transpiler"
        };

        private static bool TypeValidator(Type type) => !assembliesToSkip.Any(asmName => type.Assembly?.FullName?.Contains(asmName) ?? false) && !typesToSkip.Any(x => type.FullName.Contains(x));

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
        public static async void DoPatchesAsync()
        {
            var methodsCallingMapGetComp = new List<MethodInfo>();
            var methodsCallingWorldGetComp = new List<MethodInfo>();
            var methodsCallingGameGetComp = new List<MethodInfo>();
            var methodsCallingThingGetComp = new List<MethodInfo>();
            var methodsCallingThingTryGetComp = new List<MethodInfo>();
            var methodsCallingHediffTryGetComp = new List<MethodInfo>();
            var methodsCallingWorldObjectGetComp = new List<MethodInfo>();

            var methodsToParse = new List<MethodInfo>();
            await Task.Run(() => 
            {
                try
                {
                    var types = GetTypesToParse();
                    foreach (var type in types)
                    {
                        //Log.Message("Type: " + type);
                        //Log.ResetMessageCount();
                        foreach (var method in type.GetMethodsToParse())
                        {
                            methodsToParse.Add(method);
                        }
                    }

                    for (var i = 0; i < methodsToParse.Count; i++)
                    {
                        ParseMethod(methodsToParse[i], methodsCallingMapGetComp, methodsCallingWorldGetComp, methodsCallingGameGetComp, methodsCallingThingGetComp,
                            methodsCallingThingTryGetComp, methodsCallingHediffTryGetComp, methodsCallingWorldObjectGetComp);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Exception in Performance Optimizer: " + ex);
                }
            });
            Patch(methodsCallingMapGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetMapCompTranspiler))));
            Patch(methodsCallingWorldGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetWorldCompTranspiler))));
            Patch(methodsCallingGameGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetGameCompTranspiler))));
            Patch(methodsCallingWorldObjectGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetWorldObjectCompTranspiler))));
            Patch(methodsCallingThingGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.GetThingCompTranspiler))));
            Patch(methodsCallingThingTryGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.TryGetThingCompTranspiler))));
            Patch(methodsCallingHediffTryGetComp, new HarmonyMethod(AccessTools.Method(typeof(GetCompPatches), nameof(GetCompPatches.TryGetHediffCompTranspiler))));
        }

        private static int patchedMethodsCount = 0;
        private static void Patch(List<MethodInfo> methodsToPatch, HarmonyMethod transpiler)
        {
            foreach (var method in methodsToPatch)
            {
                try
                {
                    PerformanceOptimizerMod.harmony.Patch(method, transpiler: transpiler);
                    patchedMethodsCount++;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.GetType() + " - Error in transpiling: " + method.FullDescription() + ", type: " + method.DeclaringType + ", exception: " + ex + " - InnerException: " + ex.InnerException);
                }
            }
        }

        public static MethodInfo genericMapGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetMapComponentDict));
        private static IEnumerable<CodeInstruction> GetMapCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComponent", typeof(MapComponent), genericMapGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        public static MethodInfo genericWorldGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldComponentDict));
        private static IEnumerable<CodeInstruction> GetWorldCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComponent", typeof(WorldComponent), genericWorldGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        public static MethodInfo genericGameGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetGameComponentDict));
        private static IEnumerable<CodeInstruction> GetGameCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComponent", typeof(GameComponent), genericGameGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        public static MethodInfo genericThingGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetThingCompFast));
        private static IEnumerable<CodeInstruction> GetThingCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComp", typeof(ThingComp), genericThingGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }

        public static MethodInfo genericThingTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetThingCompFast));
        private static IEnumerable<CodeInstruction> TryGetThingCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("TryGetComp", typeof(ThingComp), genericThingTryGetComp, OpCodes.Call, 1, instructions, source, il);
        }

        public static MethodInfo genericHediffTryGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.TryGetHediffCompFast));
        private static IEnumerable<CodeInstruction> TryGetHediffCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("TryGetComp", typeof(HediffComp), genericHediffTryGetComp, OpCodes.Call, 1, instructions, source, il);
        }

        public static MethodInfo genericWorldObjectGetComp = AccessTools.Method(typeof(ComponentCache), nameof(ComponentCache.GetWorldObjectCompFast));
        private static IEnumerable<CodeInstruction> GetWorldObjectCompTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            return PerformTranspiler("GetComponent", typeof(WorldObjectComp), genericWorldObjectGetComp, OpCodes.Callvirt, 0, instructions, source, il);
        }
        public static bool CallsComponent(this CodeInstruction codeInstruction, OpCode opcode, string methodName, Type baseCompType, int parameterLength, out Type curType)
        {
            if (codeInstruction.opcode == opcode && codeInstruction.operand is MethodInfo mi && mi.Name == methodName && mi.IsGenericMethod && mi.GetParameters().Length == parameterLength)
            {
                curType = mi.GetUnderlyingType();
                if (baseCompType.IsAssignableFrom(curType))
                {
                    return true;
                }
            }
            curType = null;
            return false;
        }

        private static IEnumerable<CodeInstruction> PerformTranspiler(string methodName, Type baseType, MethodInfo genericMethod, OpCode opcode, int parameterLength, 
            IEnumerable<CodeInstruction> instructions, MethodBase source, ILGenerator il)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (instr.CallsComponent(opcode, methodName, baseType, parameterLength, out Type type))
                {
                    var methodToReplace = genericMethod.MakeGenericMethod(new Type[] { type });
                    instr.opcode = OpCodes.Call;
                    instr.operand = methodToReplace;
                }
                yield return instr;
            }
        }

        [HarmonyPatch(typeof(ThingWithComps), "InitializeComps")]
        public static class Thing_InitializeComps_Patch
        {
            public static void Postfix(ThingWithComps __instance)
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
