using Androids2;
using Verse;
using Verse.AI;
using RimWorld;

public class JobGiver_GetAndroid_Charger : ThinkNode_JobGiver
{
    public static Building_AndroidCharger GetClosestCharger(Pawn mech, Pawn carrier, bool forced)
    {
        if (!mech.Spawned || !carrier.Spawned)
        {
            return null;
        }

        Danger danger = (forced ? Danger.Deadly : Danger.Some);
        return (Building_AndroidCharger)GenClosest.ClosestThingReachable(mech.Position, mech.Map, ThingRequest.ForDef(A2_Defof.A2_AndroidRecharger), PathEndMode.InteractionCell, TraverseParms.For(carrier, danger), 9999f, delegate (Thing t)
        {
            Building_AndroidCharger building_MechCharger = (Building_AndroidCharger)t;
            if (!carrier.CanReach(t, PathEndMode.InteractionCell, danger))
            {
                return false;
            }

            if (carrier != mech)
            {
                if (!forced && building_MechCharger.Map.reservationManager.ReservedBy(building_MechCharger, carrier))
                {
                    return false;
                }

                if (forced && KeyBindingDefOf.QueueOrder.IsDownEvent && building_MechCharger.Map.reservationManager.ReservedBy(building_MechCharger, carrier))
                {
                    return false;
                }
            }

            return !t.IsForbidden(carrier) && carrier.CanReserve(t, 1, -1, null, forced) && building_MechCharger.CanPawnChargeCurrently(mech);
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

