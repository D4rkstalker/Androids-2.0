
using Verse;
using Verse.AI;
using RimWorld;

namespace Androids2
{

    public class JobGiver_GetAndroid_Charger : ThinkNode_JobGiver
    {
        public static Building_AndroidCharger GetClosestCharger(Pawn android, Pawn carrier, bool forced)
        {
            if (!android.Spawned || !carrier.Spawned)
            {
                return null;
            }

            Danger danger = (forced ? Danger.Deadly : Danger.Some);
            return (Building_AndroidCharger)GenClosest.ClosestThingReachable(android.Position, android.Map, ThingRequest.ForDef(A2_Defof.A2_AndroidCharger), PathEndMode.InteractionCell, TraverseParms.For(carrier, danger), 9999f, delegate (Thing t)
            {
                Building_AndroidCharger building_androidCharger = (Building_AndroidCharger)t;
                if (!carrier.CanReach(t, PathEndMode.InteractionCell, danger))
                {
                    return false;
                }

                if (carrier != android)
                {
                    if (!forced && building_androidCharger.Map.reservationManager.ReservedBy(building_androidCharger, carrier))
                    {
                        return false;
                    }

                    if (forced && KeyBindingDefOf.QueueOrder.IsDownEvent && building_androidCharger.Map.reservationManager.ReservedBy(building_androidCharger, carrier))
                    {
                        return false;
                    }
                }

                return !t.IsForbidden(carrier) && carrier.CanReserve(t, 1, -1, null, forced) && building_androidCharger.CannotUseNowReason(android) == null;
            });
        }

        public override Job TryGiveJob(Pawn pawn)
        {

            Building_AndroidCharger closestCharger = GetClosestCharger(pawn, pawn, forced: false);
            if (closestCharger != null)
            {
                Job job = JobMaker.MakeJob(A2_Defof.A2_AndroidCharge, closestCharger);
                job.overrideFacing = Rot4.South;
                return job;
            }

            return null;
        }
    }

}