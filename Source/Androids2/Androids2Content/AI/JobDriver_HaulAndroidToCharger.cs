using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Androids2
{
    // Token: 0x02000722 RID: 1826
    public class JobDriver_HaulAndroidToCharger : JobDriver
    {
        // Token: 0x060036B1 RID: 14001 RVA: 0x00150794 File Offset: 0x0014E994
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 1, -1, null, errorOnFailed, false) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed, false);
        }

        // Token: 0x060036B2 RID: 14002 RVA: 0x001507E9 File Offset: 0x0014E9E9
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, false).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return toil;
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false, true, false);
            Toil toil2 = Toils_Haul.CarryHauledThingToCell(TargetIndex.C, PathEndMode.OnCell);
            yield return toil2;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, false, false);
            yield return Toils_General.Do(delegate
            {
                this.pawn.Map.reservationManager.Release(this.job.targetB, this.pawn, this.job);
                Pawn pawn = (Pawn)this.job.targetA.Thing;
                Building_AndroidCharger building_AndroidCharger = (Building_AndroidCharger)this.job.targetB.Thing;
                Job job = JobMaker.MakeJob(A2_Defof.A2_AndroidCharge, building_AndroidCharger);
                pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, false, true, null, null, false, false, null, false, true, false);
            });
            yield break;
        }

        // Token: 0x04002339 RID: 9017
        private const TargetIndex MechInd = TargetIndex.A;

        // Token: 0x0400233A RID: 9018
        private const TargetIndex ChargerInd = TargetIndex.B;

        // Token: 0x0400233B RID: 9019
        private const TargetIndex ChargerCellInd = TargetIndex.C;
    }
}
