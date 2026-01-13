using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VREAndroids;

namespace Androids2
{
    public class JobGiver_AndroidCharge : ThinkNode_JobGiver
    {
        public const float FreeMemorySpaceThreshold = 0.2f;
        public const float RechargeThreshold = 0.2f;
        public override float GetPriority(Pawn pawn)
        {
            var memorySpace = pawn.needs.TryGetNeed<Need_MemorySpace>();
            var power = pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor) as Hediff_AndroidReactor;
            if (memorySpace == null || power == null || (power.Energy > RechargeThreshold && memorySpace.CurLevelPercentage > FreeMemorySpaceThreshold))
            {
                return 0f;
            }
            return 999f;
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            var power = pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor) as Hediff_AndroidReactor;

            if (power == null)
            {
                return null;
            }
            Building_AndroidCharger stand = FindChargerFor(pawn);
            if (stand != null)
            {
                return JobMaker.MakeJob(A2_Defof.A2_AndroidCharge, stand);
            }
            Job eat = null;
            if(pawn.HasActiveGene(A2_Defof.VREA_A2_BioReactor))
            {
                eat = TryEatFood(pawn, power);
                if(eat != null)
                {
                    return eat;
                }   
            }
            if (pawn.HasActiveGene(A2_Defof.VREA_A2_ChemReactor))
            {
                eat = TryEatChemfuel(pawn, power);
                if (eat != null)
                {
                    return eat;
                }
            }
            if (pawn.HasActiveGene(A2_Defof.VREA_A2_ToxReactor))
            {
                eat = TryEatWastepack(pawn, power);
                if (eat != null)
                {
                    return eat;
                }
            }
            return null;
        }
        public Job TryEatFood(Pawn pawn, Hediff_AndroidReactor power)
        {
            FoodPreferability foodPreferability = FoodPreferability.Undefined;
            Thing thing;
            ThingDef thingDef;
            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, true, out thing, out thingDef, true, true,true, false, true, true, false, true, false, false, false, foodPreferability))
            {
                if (ModsConfig.OdysseyActive && pawn.RaceProps.canFishForFood)
                {
                    return JobGiver_GetFood.TryFindFishJob(pawn);
                }
                return null;
            }
            else
            {
                Pawn pawn2 = thing as Pawn;
                Building_NutrientPasteDispenser building_NutrientPasteDispenser = thing as Building_NutrientPasteDispenser;
                if (building_NutrientPasteDispenser != null && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())
                {
                    Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
                    if (building != null)
                    {
                        ISlotGroupParent slotGroupParent = building as ISlotGroupParent;
                        Job job2 = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, slotGroupParent, false);
                        if (job2 != null)
                        {
                            return job2;
                        }
                    }
                    thing = FoodUtility.BestFoodSourceOnMap(pawn, pawn, true, out thingDef, FoodPreferability.MealLavish, false, !pawn.IsTeetotaler(), false, false, false, false, false, false, true, false, false, FoodPreferability.Undefined, null, false);
                    if (thing == null)
                    {
                        return null;
                    }
                }
                IntVec3 intVec;
                if (!Toils_Ingest.TryFindChairOrSpot(pawn, thing, out intVec))
                {
                    return null;
                }
                float nutrition = FoodUtility.GetNutrition(pawn, thing, thingDef);
                Pawn_InventoryTracker pawn_InventoryTracker = thing.ParentHolder as Pawn_InventoryTracker;
                Pawn pawn3 = ((pawn_InventoryTracker != null) ? pawn_InventoryTracker.pawn : null);
                if (pawn3 != null && pawn3 != pawn)
                {
                    Job job3 = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, thing, pawn3);
                    job3.count = FoodUtility.WillIngestStackCountOf(pawn, thingDef, nutrition);
                    return job3;
                }
                Job job4 = JobMaker.MakeJob(JobDefOf.Ingest, thing);
                job4.count = FoodUtility.WillIngestStackCountOf(pawn, thingDef, nutrition);
                return job4;
            }
        }
        public Job TryEatChemfuel(Pawn pawn, Hediff_AndroidReactor power)
        {
            return null;
        }
        public Job TryEatWastepack(Pawn pawn, Hediff_AndroidReactor power)
        {
            return null;
        }
        public static Building_AndroidCharger FindChargerFor(Pawn pawn)
        {
            foreach (var charger in Building_AndroidCharger.chargers)
            {
                if (charger.CompAssignableToPawn.AssignedPawns.Contains(pawn) && charger.CannotUseNowReason(pawn) is null
                    && pawn.CanReserveAndReach(charger, PathEndMode.OnCell, Danger.Deadly))
                {
                    return charger;
                }
            }
            foreach (var charger in Building_AndroidCharger.chargers)
            {
                if (charger.CompAssignableToPawn.AssignedPawns.Any() is false && charger.CannotUseNowReason(pawn) is null
                    && pawn.CanReserveAndReach(charger, PathEndMode.OnCell, Danger.Deadly))
                {
                    charger.CompAssignableToPawn.TryAssignPawn(pawn);
                    return charger;
                }
            }
            return null;
        }
    }
}
