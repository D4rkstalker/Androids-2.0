using Androids2;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using VREAndroids;

namespace RimWorld
{
    // Token: 0x020015DC RID: 5596
    public class WorkGiver_HaulAndroidToCharger : WorkGiver_Scanner
    {
        // Token: 0x17001434 RID: 5172
        // (get) Token: 0x06008407 RID: 33799 RVA: 0x00273681 File Offset: 0x00271881
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            }
        }

        // Token: 0x06008408 RID: 33800 RVA: 0x0026F8F3 File Offset: 0x0026DAF3
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
        }

        // Token: 0x06008409 RID: 33801 RVA: 0x00279A1C File Offset: 0x00277C1C
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || !pawn2.Spawned || !pawn2.IsAndroid() || !pawn.HasActiveGene(A2_Defof.A2_BatteryPower))
            {
                return false;
            }
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor) as Hediff_AndroidReactor;
            if (hediff != null)
            {
                if (hediff.Energy >= 0.01f)
                {
                    return false;
                }
            }

            return pawn2.CurJobDef != A2_Defof.A2_AndroidCharge && pawn.CanReserve(t, 1, -1, null, forced) && JobGiver_GetAndroid_Charger.GetClosestCharger(pawn2, pawn, forced) != null;
        }

        // Token: 0x0600840A RID: 33802 RVA: 0x00279AEC File Offset: 0x00277CEC
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = (Pawn)t;
            Building_AndroidCharger closestCharger = JobGiver_GetAndroid_Charger.GetClosestCharger(pawn2, pawn, forced);
            Job job = JobMaker.MakeJob(A2_Defof.A2_HaulAndroidToCharger, pawn2, closestCharger, closestCharger.InteractionCell);
            job.count = 1;
            return job;
        }
    }
}
