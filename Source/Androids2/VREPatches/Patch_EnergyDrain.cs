using HarmonyLib;
using RimWorld;     // if you need Log; remove if unused
using System;
using VEF;
using VEF.Graphics;
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
        public static bool Prefix(Hediff_AndroidReactor __instance, ref float __result)
        {
            int efficiency = 0;
            float efficiencyFactor = 1f;
            foreach (Gene item in __instance.pawn.genes.GenesListForReading)
            {
                if (!item.Overridden)
                {
                    efficiency += item.def.biostatMet;
                    if (item.def == A2_Defof.VREA_A2_BatteryPower)
                    {
                        efficiencyFactor *= 5;
                    }
                    else if (item.def == A2_Defof.VREA_A2_SuperCapacitor)
                    {
                        efficiencyFactor *= 10;
                    }
                    else if (item.def == A2_Defof.VREA_A2_AuxBattery)
                    {
                        efficiencyFactor *= 0.5f;
                    }
                    else if ((item.def == A2_Defof.VREA_A2_Hardware_Integration_I || item.def == A2_Defof.VREA_A2_Hardware_Integration_II || item.def == A2_Defof.VREA_A2_Hardware_Integration_III))
                    {
                        efficiencyFactor *= item.def.GetModExtension<HardwareIntegration>().complexityMult;
                    }
                    else if (item.def == A2_Defof.VREA_A2_SafetyOverrides)
                    {
                        efficiencyFactor *= 3f;
                    }
                }
            }
            if (efficiency > 0)
            {
                efficiency /= (int) efficiencyFactor; // cap at 20 for now
            }
            else
            {
                efficiency *= (int)efficiencyFactor; // cap at -20 for now
            }
            __result = AndroidStatsTable.PowerEfficiencyToPowerDrainFactorCurve.Evaluate(efficiency);
            return false;
        }
    }
}
