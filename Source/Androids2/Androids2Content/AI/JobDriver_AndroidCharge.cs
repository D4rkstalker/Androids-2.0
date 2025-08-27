using Androids2;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using VREAndroids;

namespace Androids2
{
    // Token: 0x020013A1 RID: 5025
    public class JobDriver_A2AndroidCharge : JobDriver
    {
        // Token: 0x17001365 RID: 4965
        // (get) Token: 0x06007B06 RID: 31494 RVA: 0x00250AD3 File Offset: 0x0024ECD3
        public Building_AndroidCharger Charger
        {
            get
            {
                return (Building_AndroidCharger)this.job.targetA.Thing;
            }
        }

        // Token: 0x06007B07 RID: 31495 RVA: 0x00250AEA File Offset: 0x0024ECEA
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Charger, this.job, 1, -1, null, errorOnFailed, false);
        }

        // Token: 0x06007B08 RID: 31496 RVA: 0x00250B12 File Offset: 0x0024ED12
        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => !this.Charger.CanPawnChargeCurrently(this.pawn));
            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate ()
            {
                this.Charger.StartCharging(this.pawn);
            };
            toil.AddFinishAction(delegate
            {
                if (this.Charger.CurrentlyChargingMech == this.pawn)
                {
                    this.Charger.StopCharging();
                }
            });
            toil.handlingFacing = true;
            Toil toil2 = toil;
            toil2.tickIntervalAction = (Action<int>)Delegate.Combine(toil2.tickIntervalAction, new Action<int>(delegate (int delta)
            {
                this.pawn.rotationTracker.FaceTarget(this.Charger.Position);
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor) as Hediff_AndroidReactor;
                if (hediff != null)
                {
                    if (hediff.Energy >= 0.99f)
                    {
                        base.ReadyForNextToil();
                    }
                }
            }));
            yield return toil;
            yield break;
        }

        // Token: 0x0400532D RID: 21293
        private const TargetIndex ChargerInd = TargetIndex.A;
    }
}
