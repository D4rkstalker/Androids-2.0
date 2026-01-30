using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VREAndroids;
using static UnityEngine.TouchScreenKeyboard;

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
        [HarmonyPatch(typeof(HediffUtility), nameof(HediffUtility.CanHealNaturally))]
        public static class HediffUtility_CanHealNaturally_A2Patch
        {
            public static bool Prefix(Hediff_Injury hd, ref bool __result)
            {
                __result = CanNaturallyRepair(hd.pawn);

                return __result;
            }
        }
        [HarmonyPatch(typeof(HediffUtility), "CanHealFromTending")]
        public static class HediffUtility_CanHealFromTending_A2Patch
        {
            [HarmonyPriority(int.MinValue)]
            public static bool Prefix(Hediff_Injury hd, ref bool __result)
            {
                __result = CanNaturallyRepair(hd.pawn);

                return __result;
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
        static bool CanNaturallyRepair(Pawn android)
        {
            if (!android.IsAndroid()) return true;
            if (android.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh)) return true;
            if (android.HasActiveGene(A2_Defof.VREA_A2_ReconstructionMechanite)) return true;
            return false;
        }
    }

}

