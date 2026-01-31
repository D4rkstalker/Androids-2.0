using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using VREAndroids;

namespace Androids2.VREPatches
{
    [StaticConstructorOnStartup]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Patch_GizmoEndMemoryFreeing
    {
        private static readonly Texture2D icon = ContentFinder<Texture2D>.Get("UI/BiostatIcon/BiostatEfficiency");
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            if (__instance.MentalState != null && __instance.MentalState.def == VREA_DefOf.VREA_Reformatting)
            {
                yield return new Command_Action
                {
                    defaultLabel = "A2_StopReformattingLabel".Translate(),
                    defaultDesc = "A2_StopReformattingDesc".Translate(),
                    icon = icon,
                    action = () =>
                    {
                        __instance.MentalState.RecoverFromState();
                        var memoryNeed = __instance.needs.TryGetNeed<Need_MemorySpace>();
                        if (memoryNeed != null)
                        {
                            memoryNeed.CurLevel += 0.01f;
                        }
                        var frag = __instance.health.hediffSet.GetFirstHediffOfDef(A2_Defof.A2_DataFragmentation);
                        if (frag != null)
                        {
                            frag.Severity += 0.3f;

                        }
                        else
                        {
                            frag = HediffMaker.MakeHediff(A2_Defof.A2_DataFragmentation, __instance);
                            frag.Severity = 0.3f;
                            __instance.health.AddHediff(frag);

                        }

                    }

                };
            }
            foreach (var gizmo in __result)
                yield return gizmo;            
        }
    }
}
