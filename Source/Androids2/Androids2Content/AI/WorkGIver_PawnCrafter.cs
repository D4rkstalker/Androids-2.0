using Androids2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Androids2
{
    public class PawnCrafterWorkgiverProperties : DefModExtension
    {
        /// <summary>
        /// ThingDef to scan for.
        /// </summary>
        public ThingDef defToScan;

        /// <summary>
        /// Fill Job to give.
        /// </summary>
        public JobDef fillJob;
    }
    /// <summary>
    /// Generic variant of the Android Printer WorkGiver which make pawns attempt to fill up the crafter.
    /// </summary>
    public class WorkGiver_PawnCrafter : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(WorkGiverProperties.defToScan);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        private PawnCrafterWorkgiverProperties intWorkGiverProperties = null;

        public PawnCrafterWorkgiverProperties WorkGiverProperties
        {
            get
            {
                if (intWorkGiverProperties == null)
                {
                    intWorkGiverProperties = def.GetModExtension<PawnCrafterWorkgiverProperties>();
                }

                return intWorkGiverProperties;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_PawnCrafter pawnCrafter = t as Building_PawnCrafter;

            if (pawnCrafter == null || pawnCrafter.crafterStatus != CrafterStatus.Filling)
                return false;

            if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
            {
                return false;
            }

            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }

            //Check if there is anything to fill.
            IEnumerable<IngredientCount> potentionalRequests = pawnCrafter.orderProcessor.PendingRequests();
            if (potentionalRequests != null)
            {
               return FindIngredient(pawn, pawnCrafter, potentionalRequests.ToList()).count > 0;

            }

            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing crafterThing, bool forced = false)
        {
            Building_PawnCrafter pawnCrafter = crafterThing as Building_PawnCrafter;

            IEnumerable<IngredientCount> potentionalRequests = pawnCrafter.orderProcessor.PendingRequests();

            if (potentionalRequests != null)
            {
                ThingCount thingCount = FindIngredient(pawn, pawnCrafter,potentionalRequests);

                if (thingCount.thing != null)
                {
                    return new Job(WorkGiverProperties.fillJob, thingCount.thing, crafterThing)
                    {
                        count = (int)thingCount.count
                    };
                }

            }

            return null;
        }

        /// <summary>
        /// Tries to find a appropiate ingredient.
        /// </summary>
        /// <param name="pawn">Pawn to search for.</param>
        /// <param name="androidPrinter">Printer to fill.</param>
        /// <param name="request">Thing order request to fulfill.</param>
        /// <returns>Valid thing if found, otherwise null.</returns>
        private ThingCount FindIngredient(Pawn pawn, Building_PawnCrafter androidPrinter, IEnumerable<IngredientCount> ingredients)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
               ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
               PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
            if (thing == null)
            {
                return default(ThingCount);
            }
            int requiredCountOf = androidPrinter.GetRequiredCountOf(thing.def, ingredients);
            return new ThingCount(thing, Mathf.Min(thing.stackCount, requiredCountOf));
            bool Validator(Thing x)
            {
                if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
                {
                    return false;
                }
                return androidPrinter.CanAcceptIngredient(x, ingredients);
            }
        }
    }
}
