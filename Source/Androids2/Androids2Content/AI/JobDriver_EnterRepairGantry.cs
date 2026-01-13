using Androids2.Androids2Content.Comps;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Androids2.Androids2Content.AI
{

    public class JobDriver_EnterRepairGantry : JobDriver
    {
        public const int EnterPodDelay = 70;

        public List<Thing> pickedUpIngredients = new List<Thing>();

        public const TargetIndex BiosculpterIndex = TargetIndex.A;

        public const TargetIndex IngredientInd = TargetIndex.B;

        public CompRepairGantry BiosculpterPod => job.targetA.Thing.TryGetComp<CompRepairGantry>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }

            List<LocalTargetInfo> targetQueue = job.GetTargetQueue(TargetIndex.B);
            for (int i = 0; i < targetQueue.Count; i++)
            {
                if (!pawn.Reserve(targetQueue[i], job, 1, -1, null, errorOnFailed))
                {
                    return false;
                }
            }

            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            if (!ModLister.CheckIdeology("Biosculpting"))
            {
                yield break;
            }

            AddFinishAction(delegate
            {
                if (BiosculpterPod != null)
                {
                    if (BiosculpterPod.queuedEnterJob == job)
                    {
                        BiosculpterPod.ClearQueuedInformation();
                    }

                    if (BiosculpterPod.Occupant != GetActor())
                    {
                        foreach (Thing pickedUpIngredient in pickedUpIngredients)
                        {
                            if (pawn.inventory.Contains(pickedUpIngredient))
                            {
                                pawn.inventory.innerContainer.TryDrop(pickedUpIngredient, ThingPlaceMode.Near, out var _);
                            }
                        }
                    }
                }
            });
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => job.biosculpterCycleKey == null || !BiosculpterPod.CanAcceptOnceCycleChosen(GetActor()));
            Toil goToPod = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_Jump.JumpIf(goToPod, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
            foreach (Toil item in CollectIngredientsToilsHelper(TargetIndex.B, pawn, pickedUpIngredients))
            {
                yield return item;
            }

            yield return goToPod.FailOn(() => !BiosculpterPod.PawnCarryingExtraCycleIngredients(pawn, job.biosculpterCycleKey));
            yield return PrepareToEnterToil(TargetIndex.A);
            Toil enter = ToilMaker.MakeToil("MakeNewToils");
            enter.initAction = delegate
            {
                BiosculpterPod.TryAcceptPawn(enter.actor, job.biosculpterCycleKey);
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }

        public override string GetReport()
        {
            if (!BiosculpterPod.PawnCarryingExtraCycleIngredients(pawn, job.biosculpterCycleKey))
            {
                return "BiosculpterJobReportCollectIngredients".Translate();
            }

            return base.GetReport();
        }

        public static IEnumerable<Toil> CollectIngredientsToilsHelper(TargetIndex ingredientIndex, Pawn carrier, List<Thing> pickedUpIngredients)
        {
            Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientIndex);
            yield return extract;
            yield return Toils_Goto.GotoThing(ingredientIndex, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(ingredientIndex).FailOnSomeonePhysicallyInteracting(ingredientIndex);
            yield return Toils_Haul.StartCarryThing(ingredientIndex, putRemainderInQueue: true, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true);
            yield return HaulBiosculpterIngredientInInventory(carrier, pickedUpIngredients);
            yield return Toils_Jump.JumpIfHaveTargetInQueue(ingredientIndex, extract);
        }

        public static Toil HaulBiosculpterIngredientInInventory(Pawn carrier, List<Thing> pickedUpIngredients)
        {
            Toil toil = ToilMaker.MakeToil("HaulBiosculpterIngredientInInventory");
            toil.initAction = delegate
            {
                Thing carriedThing = carrier.carryTracker.CarriedThing;
                carrier.carryTracker.innerContainer.TryTransferToContainer(carriedThing, carrier.inventory.innerContainer, carriedThing.stackCount, out var resultingTransferredItem);
                if (resultingTransferredItem != null)
                {
                    pickedUpIngredients.Add(resultingTransferredItem);
                }
            };
            return toil;
        }

        public static Toil PrepareToEnterToil(TargetIndex podIndex)
        {
            Toil prepare = Toils_General.Wait(70);
            prepare.FailOnCannotTouch(podIndex, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(podIndex);
            prepare.PlaySustainerOrSound(() => prepare.actor.CurJob.GetTarget(podIndex).Thing?.TryGetComp<CompRepairGantry>()?.Props.enterSound);
            return prepare;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pickedUpIngredients, "pickedUpIngredients", LookMode.Reference);
        }
    }
}