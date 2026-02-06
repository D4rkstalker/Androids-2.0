using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using VREAndroids;
using Verse;
using Verse.AI;

namespace Androids2.VREPatches
{
    internal class Patch_RepairAndroid
    {
        [HarmonyPatch(typeof(JobDriver_RepairAndroid), nameof(JobDriver_RepairAndroid.MakeNewToils))]
        public static class Patch_RepairAndroid_MakeNewToils
        {
            [HarmonyPriority(Priority.Last)]
            public static void Prefix(JobDriver_RepairAndroid __instance)
            {
                if (__instance.pawn.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh))
                {
                    __instance.job.endAfterTendedOnce = true;
                }
            }
        }
    }
}
