using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Androids2
{
    // Token: 0x020032E9 RID: 13033
    public class FloatMenuOptionProvider_CarryAndroidToCharger : FloatMenuOptionProvider
    {
        // Token: 0x17002F7A RID: 12154
        // (get) Token: 0x060125A0 RID: 75168 RVA: 0x000028E7 File Offset: 0x00000AE7
        public override bool Drafted
        {
            get
            {
                return true;
            }
        }

        // Token: 0x17002F7B RID: 12155
        // (get) Token: 0x060125A1 RID: 75169 RVA: 0x000028E7 File Offset: 0x00000AE7
        public override bool Undrafted
        {
            get
            {
                return true;
            }
        }

        // Token: 0x17002F7C RID: 12156
        // (get) Token: 0x060125A2 RID: 75170 RVA: 0x00002501 File Offset: 0x00000701
        public override bool Multiselect
        {
            get
            {
                return false;
            }
        }

        // Token: 0x17002F7D RID: 12157
        // (get) Token: 0x060125A3 RID: 75171 RVA: 0x000028E7 File Offset: 0x00000AE7
        public override bool RequiresManipulation
        {
            get
            {
                return true;
            }
        }

        // Token: 0x060125A4 RID: 75172 RVA: 0x00566B79 File Offset: 0x00564D79
        public override bool AppliesInt(FloatMenuContext context)
        {
            return ModsConfig.BiotechActive;
        }

        // Token: 0x060125A5 RID: 75173 RVA: 0x00568598 File Offset: 0x00566798
        public override FloatMenuOption GetSingleOptionFor(Pawn clickedPawn, FloatMenuContext context)
        {
            if (clickedPawn == context.FirstSelectedPawn)
            {
                return null;
            }
            //if (!clickedPawn.IsSelfShutdown())
            //{
            //    return null;
            //}
            Building_AndroidCharger charger = JobGiver_GetAndroid_Charger.GetClosestCharger(clickedPawn, context.FirstSelectedPawn, false);
            if (charger == null)
            {
                charger = JobGiver_GetAndroid_Charger.GetClosestCharger(clickedPawn, context.FirstSelectedPawn, true);
            }
            if (charger == null)
            {
                return new FloatMenuOption("CannotCarryToRecharger".Translate(clickedPawn.Named("PAWN")) + ": " + "CannotCarryToRechargerNoneAvailable".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            if (!context.FirstSelectedPawn.CanReach(charger, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                return new FloatMenuOption("CannotCarryToRecharger".Translate(clickedPawn.Named("PAWN")) + ": " + "NoPath".Translate().CapitalizeFirst(), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
            }
            return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToRechargerOrdered".Translate(clickedPawn.Named("PAWN")), delegate
            {
                Job job = JobMaker.MakeJob(A2_Defof.A2_HaulAndroidToCharger, clickedPawn, charger, charger.InteractionCell);
                job.count = 1;
                context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), context.FirstSelectedPawn, new LocalTargetInfo(clickedPawn), "ReservedBy", null);
        }
    }
}
