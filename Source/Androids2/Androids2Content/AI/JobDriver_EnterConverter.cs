using Androids2;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Androids2
{
    // Token: 0x02000009 RID: 9
    public class JobDriver_A2EnterConverter : JobDriver
    {
        // Token: 0x06000018 RID: 24 RVA: 0x000033AE File Offset: 0x000015AE
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return ReservationUtility.Reserve(this.pawn, this.job.targetA, this.job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x06000019 RID: 25 RVA: 0x000033D0 File Offset: 0x000015D0
        public override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedOrNull<JobDriver_A2EnterConverter>(this, TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil toil = Toils_General.Wait(20, 0);
            ToilFailConditions.FailOnCannotTouch<Toil>(toil, TargetIndex.A, PathEndMode.InteractionCell);
            ToilEffects.WithProgressBarToilDelay(toil, TargetIndex.A, false, -0.5f);
            yield return toil;
            Toil enter = new Toil();
            enter.initAction = delegate ()
            {
                Pawn actor = enter.actor;
                Building_Converter conversionChamber = (Building_Converter)actor.CurJob.targetA.Thing;
                Action action = delegate ()
                {
                    actor.DeSpawn(0);
                    conversionChamber.TryAcceptThing(actor, true);
                    conversionChamber.Notify_PawnEntered();
                };
                if (conversionChamber.def.building.isPlayerEjectable)
                {
                    action();
                    return;
                }
                if (this.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                {
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(TranslatorFormattedStringExtensions.Translate("CasketWarning", NamedArgumentUtility.Named(actor, "PAWN")).AdjustedFor(actor, "PAWN", true), action, false, null, WindowLayer.Dialog));
                    return;
                }
                action();
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
            yield break;
        }
    }
}
