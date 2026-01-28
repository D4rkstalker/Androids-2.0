using Androids2;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using VREAndroids;

namespace Androids2
{

    public class JobDriver_A2AndroidCharge : JobDriver
    {
        public Building_AndroidCharger AndroidCharger => job.targetA.Thing as Building_AndroidCharger;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        private Mote moteCharging;

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOn(() => AndroidCharger.compPower != null && AndroidCharger.compPower.PowerOn is false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                toil.actor.pather.StopDead();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.handlingFacing = true;
            toil.tickAction = delegate
            {
                toil.actor.Rotation = Rot4.South;
                var memorySpace = this.pawn.needs.TryGetNeed<Need_MemorySpace>();
                var memorySpaceGain = memorySpace.curLevelInt + (1f /
                    (float)MentalState_Reformatting.TicksToRecoverFromReformatting(pawn, null) * 0.2f);
                memorySpace.curLevelInt = Mathf.Min(1f, memorySpaceGain);
                var power = pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor) as Hediff_AndroidReactor;

                //var powerGain = power.curLevelInt + AndroidCharger.chargeRate;
                //power.curLevelInt = Mathf.Min(1f, powerGain);
                //Debug.LogWarning("power gain rate: " + AndroidCharger.chargeRate);
                if (memorySpace.curLevelInt == 1f && power.Energy == 1f)
                {
                    this.EndJobWith(JobCondition.Succeeded);
                }

                if (moteCharging == null || moteCharging.Destroyed)
                {
                    moteCharging = MoteMaker.MakeAttachedOverlay(pawn, VREA_DefOf.VREA_Mote_AndroidReformatting, Vector3.zero);
                }
                moteCharging?.Maintain();
            };
            yield return toil;
        }

    }
}
