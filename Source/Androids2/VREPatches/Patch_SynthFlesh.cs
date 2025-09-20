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
    [StaticConstructorOnStartup]

    public class Patch_SynthFlesh
    {
        static Patch_SynthFlesh()
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
        [HarmonyPatch(typeof(HediffUtility), nameof(HediffUtility.CanHealNaturally))]
        public static class HediffUtility_CanHealNaturally_A2Patch
        {
            public static bool Prefix(Hediff_Injury hd, ref bool __result)
            {
                if (hd.pawn.IsAndroid() && !hd.pawn.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(HediffUtility), "CanHealFromTending")]
        public static class HediffUtility_CanHealFromTending_A2Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(Hediff_Injury hd, ref bool __result)
            {
                if (hd.pawn.IsAndroid() && !hd.pawn.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        // Target: public static bool JobDriver_RepairAndroid.CanRepairAndroid(Pawn android)
        [HarmonyPatch(typeof(JobDriver_RepairAndroid), nameof(JobDriver_RepairAndroid.CanRepairAndroid))]
        public static class Patch_CanRepairAndroid_Prefix
        {

            [HarmonyPriority(Priority.Last)]
            public static bool Prefix(Pawn android, ref bool __result)
            {
                if(android.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }

}

