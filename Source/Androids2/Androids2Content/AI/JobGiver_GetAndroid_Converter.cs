using Verse;
using Verse.AI;
using RimWorld;
namespace Androids2
{

    public class JobGiver_GetAndroid_Converter : ThinkNode_JobGiver
    {
        public static Building_Converter GetClosestConverter(Pawn android, Pawn carrier, bool forced)
        {
            if (!android.Spawned || !carrier.Spawned)
            {
                return null;
            }

            Danger danger = (forced ? Danger.Deadly : Danger.Some);
            return (Building_Converter)GenClosest.ClosestThingReachable(android.Position, android.Map, ThingRequest.ForDef(A2_Defof.A2_Converter), PathEndMode.InteractionCell, TraverseParms.For(carrier, danger), 9999f, delegate (Thing t)
            {
                Building_Converter building_converter = (Building_Converter)t;
                if (!carrier.CanReach(t, PathEndMode.InteractionCell, danger))
                {
                    return false;
                }

                if (carrier != android)
                {
                    if (!forced && building_converter.Map.reservationManager.ReservedBy(building_converter, carrier))
                    {
                        return false;
                    }

                    if (forced && KeyBindingDefOf.QueueOrder.IsDownEvent && building_converter.Map.reservationManager.ReservedBy(building_converter, carrier))
                    {
                        return false;
                    }
                }

                return !t.IsForbidden(carrier) && carrier.CanReserve(t, 1, -1, null, forced) && building_converter.CannotUseNowReason(android) == null;
            });
        }

        public override Job TryGiveJob(Pawn pawn)
        {

            Building_Converter closestCharger = GetClosestConverter(pawn, pawn, forced: false);
            if (closestCharger != null)
            {
                Job job = JobMaker.MakeJob(A2_Defof.A2_EnterConverter, closestCharger);
                job.overrideFacing = Rot4.South;
                return job;
            }

            return null;
        }
    }

}