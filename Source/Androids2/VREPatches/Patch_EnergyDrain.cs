using HarmonyLib;
using RimWorld;     // if you need Log; remove if unused
using System;
using Verse;       // for Pawn, etc.
using VREAndroids; // for Hediff_AndroidReactor
namespace Androids2
{
    // Patch the getter: Hediff_AndroidReactor.PowerEfficiencyDrainMultiplier
    [HarmonyPatch(typeof(Hediff_AndroidReactor))]
    [HarmonyPatch(nameof(Hediff_AndroidReactor.PowerEfficiencyDrainMultiplier), MethodType.Getter)]
    public static class Patch_Hediff_AndroidReactor_PowerEfficiencyDrainMultiplier
    {
        // Postfix runs after the original; we can override or adjust __result.
        public static void Postfix(Hediff_AndroidReactor __instance, ref float __result)
        {
            if (__instance.pawn.HasActiveGene(A2_Defof.A2_BatteryPower) && !__instance.pawn.HasActiveGene(A2_Defof.A2_AuxBattery))
                __result *= 2;
        }
    }
}
