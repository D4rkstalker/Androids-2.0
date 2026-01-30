using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2.VREPatches
{
    [HarmonyPatch(typeof(Need_MemorySpace), nameof(Need_MemorySpace.NeedInterval))]
    public static class Patch_Need_MemorySpace_NeedInterval
    {
        public static bool Prefix(Need_MemorySpace __instance)
        {
            __instance.curLevelInt = Mathf.Max(0, __instance.curLevelInt - ((1f / GenDate.TicksPerDay) * 150f * __instance.pawn.GetStatValue(VREA_DefOf.VREA_MemorySpaceDrainMultiplier)));
            if (__instance.curLevelInt == 0f)
            {
                if (!__instance.pawn.Drafted)
                {
                    if (__instance.pawn.MentalStateDef != VREA_DefOf.VREA_Reformatting)
                    {
                        if (__instance.pawn.Spawned)
                        {
                            if (__instance.pawn.InMentalState)
                            {
                                __instance.pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                            }
                            __instance.pawn.mindState.mentalStateHandler.TryStartMentalState(VREA_DefOf.VREA_Reformatting);

                        }
                        else
                        {
                            __instance.curLevelInt = 1f;
                        }
                    }
                }
                else
                {
                    var frag = __instance.pawn.health.hediffSet.GetFirstHediffOfDef(A2_Defof.A2_DataFragmentation);
                    if (frag != null)
                    {
                        frag.Severity += 0.01f;

                    }
                    else
                    {
                        frag = HediffMaker.MakeHediff(A2_Defof.A2_DataFragmentation, __instance.pawn);
                        frag.Severity = 0.01f;
                    }

                }

            }
            return false; // skip original NeedInterval()
        }
    }
}
