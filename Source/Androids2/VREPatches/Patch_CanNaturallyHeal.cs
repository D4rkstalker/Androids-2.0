using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VREAndroids;

namespace Androids2.VREPatches
{

    public class Patch_CanNaturallyHeal
    {
        static Patch_CanNaturallyHeal()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                TryUnpatchVREAndroids();
            });
        }

        private static void TryUnpatchVREAndroids()
        {
            var original = AccessTools.Method(typeof(HediffUtility), nameof(HediffUtility.CanHealNaturally));
            Helpers.Unpatch(original, "VREAndroids.HediffUtility_CanHealNaturally_Patch");

            original = AccessTools.Method(typeof(HediffUtility), nameof(HediffUtility.CanHealFromTending));
            Helpers.Unpatch(original, "VREAndroids.HediffUtility_CanHealFromTending_Patch");

        }
        //[HarmonyPatch(typeof(HediffUtility), nameof(HediffUtility.CanHealNaturally))]
        //public static class HediffUtility_CanHealNaturally_A2Patch
        //{
        //    public static void Postfix(Hediff_Injury hd, ref bool __result)
        //    {
        //        if (hd.pawn.IsAndroid())
        //        {
        //            __result = Helpers.CanNaturallyRepair(hd.pawn);
        //        }
        //    }
        //}
        //[HarmonyPatch(typeof(HediffUtility), "CanHealFromTending")]
        //public static class HediffUtility_CanHealFromTending_A2Patch
        //{
        //    [HarmonyPriority(int.MinValue)]
        //    public static void Postfix(Hediff_Injury hd, ref bool __result)
        //    {
        //        if (hd.pawn.IsAndroid())
        //        {
        //            __result = Helpers.CanNaturallyRepair(hd.pawn);
        //        }
        //    }
        //}
        // Target: public static bool JobDriver_RepairAndroid.CanRepairAndroid(Pawn android)
        //[HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HasHediffsNeedingTend))]
        //public static class Patch_HasHediffsNeedingTend_Prefix
        //{
        //    [HarmonyPriority(Priority.Last)]
        //    public static bool Prefix(Pawn ___pawn, ref bool __result)
        //    {
        //        Log.Warning("checking if tending needed:");
        //        if (___pawn.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh))
        //        {
        //            foreach (Hediff hediff in ___pawn.health.hediffSet.hediffs)
        //            {
        //                if (hediff.Bleeding)
        //                {
        //                    Log.Warning("yes");

        //                    __result = true;
        //                    return false;
        //                }
        //            }
        //            Log.Warning("no");
        //            __result = false;
        //            return false;
        //        }
        //        return true;
        //    }
        //}
        [HarmonyPatch(typeof(JobDriver_RepairAndroid), nameof(JobDriver_RepairAndroid.GetHediffToHeal))]
        public static class Patch_GetHediffToHeal_Prefix
        {

            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(Pawn android, ref Hediff __result)
            {
                if (android.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh))
                {
                    float bleedrate = 0;
                    Hediff injury = null;
                    foreach (Hediff hediff in android.health.hediffSet.hediffs)
                    {
                        if (hediff.Bleeding && hediff.BleedRate > bleedrate)
                        {
                            bleedrate = hediff.BleedRate;
                            injury = hediff;
                        }
                    }
                    __result = injury;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(JobDriver_RepairAndroid), nameof(JobDriver_RepairAndroid.CanRepairAndroid))]
        public static class Patch_CanRepairAndroid_Prefix
        {

            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(Pawn android, ref bool __result)
            {
                if (android.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh))
                {
                    foreach (Hediff hediff in android.health.hediffSet.hediffs)
                    {
                        if (hediff.Bleeding)
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }

}

