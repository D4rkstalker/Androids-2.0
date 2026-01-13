using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VREAndroids;

namespace Androids2.VREPatches
{
    [HarmonyPatch(typeof(MeditationFocusTypeAvailabilityCache_PawnCanUseInt_Patch), "Postfix")]
    public static class MeditationTypeAvailabilityUnPreventionPatch
    {
        public static bool Prefix(Pawn p)
        {
            if (p.HasActiveGene(VREA_DefOf.VREA_JoyDisabled) && p.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MeditationUtility_CanMeditateNow_Patch), "Postfix")]
    public static class CanMeditateNowFixer
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.HasActiveGene(VREA_DefOf.VREA_JoyDisabled) && pawn.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip))
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MeditationUtility), "CanMeditateNow")]
    public static class MeditationJobgiverUnPreventionPatch
    {
        [HarmonyPriority(Priority.High)]
        public static void PostFix(ref bool __result, Pawn pawn)
        {
            if (pawn.HasActiveGene(VREAndroids.VREA_DefOf.VREA_JoyDisabled) && !(pawn.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip)))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(JoyGiver_Meditate), "TryGiveJob")]
    public static class MeditationJobGiverFixer
    {
        [HarmonyPriority(Priority.High)]
        public static void PostFix(ref Job __result, Pawn pawn)
        {
            if (pawn.HasActiveGene(VREAndroids.VREA_DefOf.VREA_JoyDisabled) && pawn.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip))
            {
                __result = MeditationUtility.GetMeditationJob(pawn, forJoy: false);
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), "ShouldShowFor")]
    public static class ShowMeditationFocusGainPatch
    {
        [HarmonyPriority(Priority.High)]
        public static void Postfix(StatWorker __instance, ref bool __result, StatRequest req, StatDef ___stat)
        {
            if (__result && req.Thing is Pawn pawn && pawn.IsAndroid())
            {
                if (___stat == StatDefOf.MeditationFocusGain && pawn.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip))
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class CreateInitialComponentsPatch
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.RaceProps.IsFlesh)
            {
                return true;
            }
            if (pawn.IsAndroid() && pawn.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip) && pawn.psychicEntropy == null)
            {
                pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddComponentsForSpawn")]
    public static class AddComponentsForSpawnPatch
    {
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.RaceProps.IsFlesh)
            {
                return true;
            }
            if (pawn.IsAndroid() && pawn.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip) && pawn.psychicEntropy == null)
            {
                pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Hediff_Psylink_ChangeLevel_Patch), "Prefix")]
    public static class Psylink_Unprevention_patch
    {
        public static void Postfix(ref bool __result, object[] __args)
        {
            Hediff_Psylink instance = (Hediff_Psylink)__args[0];
            if (instance.pawn.IsAndroid() && instance.pawn.HasActiveGene(A2_Defof.VREA_A2_EltexPsyChip))
            {
                __result = true;
            }
        }
    }
    //[StaticConstructorOnStartup]
    //public static class PuppeteerCompatBootstrap
    //{
    //    static PuppeteerCompatBootstrap()
    //    {
    //        if (!ModsConfig.IsActive("VanillaExpanded.VPE.Puppeteer"))
    //            return; // Puppeteer not loaded → do nothing

    //        Androids2.harmony.Patch(
    //            AccessTools.Method(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState),
    //                new[]
    //                {
    //                        typeof(MentalStateDef), typeof(string), typeof(bool), typeof(bool), typeof(bool),
    //                        typeof(Pawn), typeof(bool), typeof(bool), typeof(bool)
    //                }),
    //            prefix: new HarmonyMethod(typeof(AllowVREAStatesOnPuppets_Patch), nameof(AllowVREAStatesOnPuppets_Patch.Prefix)),
    //            finalizer: new HarmonyMethod(typeof(AllowVREAStatesOnPuppets_Patch), nameof(AllowVREAStatesOnPuppets_Patch.Finalizer))
    //        );
    //    }
    //}

    //[HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    //public static class AllowVREAStatesOnPuppets_Patch
    //{
    //    private static FieldInfo _shouldStartField;
    //    private static MethodInfo _isPuppetMI;
    //    private static bool _resolved;

    //    // Resolve Puppeteer reflection targets once; only if the mod is active
    //    private static void EnsureResolved()
    //    {
    //        if (_resolved) return;
    //        _resolved = true;

    //        if (!ModsConfig.IsActive("VanillaExpanded.VPE.Puppeteer"))
    //            return;

    //        var asm = AppDomain.CurrentDomain
    //            .GetAssemblies()
    //            .FirstOrDefault(a => a.GetName().Name == "VPEPuppeteer");
    //        if (asm == null) return;

    //        var patchType = asm.GetType("VPEPuppeteer.MentalStateHandler_TryStartMentalState_Patch");
    //        _shouldStartField = AccessTools.Field(patchType, "shouldStartMentalState");

    //        var utilsType = asm.GetType("VPEPuppeteer.VPEPUtils");
    //        _isPuppetMI = AccessTools.Method(utilsType, "IsPuppet", new[] { typeof(Pawn) });
    //    }

    //    // Runs as early as possible; we don’t hard-order vs Puppeteer but match its max priority
    //    [HarmonyPriority(int.MaxValue)]
    //    public static void Prefix(
    //        MentalStateDef stateDef,
    //        Pawn ___pawn,
    //        out bool __state // did we flip the flag?
    //    )
    //    {
    //        __state = false;

    //        if (!ModsConfig.IsActive("VanillaExpanded.VPE.Puppeteer"))
    //            return;

    //        EnsureResolved();
    //        if (_shouldStartField == null || _isPuppetMI == null || stateDef == null || ___pawn == null)
    //            return;

    //        // Only for these two VREA states, and only if the pawn is a puppet per Puppeteer
    //        if ((stateDef.defName == "VREA_Reformatting" || stateDef.defName == "VREA_SolarFlared"))
    //        {
    //            var isPuppet = _isPuppetMI.Invoke(null, new object[] { ___pawn }) as bool? ?? false;
    //            if (isPuppet)
    //            {
    //                _shouldStartField.SetValue(null, true);
    //                __state = true; // mark that we changed it
    //            }
    //        }
    //    }

    //    public static void Finalizer(
    //        Exception __exception,
    //        bool __state // restore only if we flipped it
    //    )
    //    {
    //        if (!__state) return;
    //            _shouldStartField?.SetValue(null, false);
    //    }
    //}
}
