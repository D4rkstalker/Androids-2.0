using global::RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using VREAndroids;

namespace Androids2
{

    // Token: 0x02002A35 RID: 10805
    [HotSwappable]
    public class Building_AndroidCharger : Building_Bed
    {
        public static HashSet<Building_AndroidCharger> chargers = new HashSet<Building_AndroidCharger>();

        public CompPowerTrader compPower;
        public float chargeRate = 0.001f; 
        public Pawn CurOccupant
        {
            get
            {
                List<Thing> list = Map.thingGrid.ThingsListAt(this.Position);
                for (int i = 0; i < list.Count; i++)
                {
                    Pawn pawn = list[i] as Pawn;
                    if (pawn != null && pawn.IsAndroid() && pawn.pather.moving is false)
                    {
                        return pawn;
                    }
                }
                return null;
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            chargers.Add(this);
            compPower = this.TryGetComp<CompPowerTrader>();
            this.Medical = true;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            chargers.Remove(this);
        }
        public override string GetInspectString()
        {
            this.Medical = false;
            this.def.building.bed_humanlike = false;
            var sb = new StringBuilder(base.GetInspectString() + "\n");
            this.Medical = true;
            this.def.building.bed_humanlike = true;
            return sb.ToString().TrimEndNewlines();
        }

        public override void Tick()
        {
            base.Tick();
            var occupant = CurOccupant;
            if (occupant != null)
            {
                occupant.Rotation = Rot4.South;
                if (occupant.jobs.curDriver is JobDriver_LayDown)
                {
                    occupant.jobs.curDriver.rotateToFace = TargetIndex.C;
                }
            }
        }
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(selPawn))
            {
                yield return opt;
            }
            if (this.Faction == Faction.OfPlayer && selPawn.HasActiveGene(A2_Defof.A2_BatteryPower))
            {
                var cannotUseReason = CannotUseNowReason(selPawn);
                if (cannotUseReason.NullOrEmpty())
                {
                    yield return new FloatMenuOption("Androids2.Recharge".Translate(), delegate
                    {
                        if (CompAssignableToPawn.AssignedPawns.Contains(selPawn) is false)
                        {
                            CompAssignableToPawn.TryAssignPawn(selPawn);
                        }
                        selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(A2_Defof.A2_AndroidCharge, this));
                    });
                }
                else
                {
                    yield return new FloatMenuOption("Androids2.Recharge".Translate() + ": " + cannotUseReason, null);
                }
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Toggle toggle)
                {
                    if (toggle.defaultLabel == "CommandBedSetAsMedicalLabel".Translate())
                    {
                        continue;
                    }
                }
                yield return gizmo;
            }
        }

        public string CannotUseNowReason(Pawn selPawn)
        {
            if (compPower != null && !compPower.PowerOn)
            {
                return "NoPower".Translate().CapitalizeFirst();
            }
            if (!selPawn.CanReach(this, PathEndMode.OnCell, Danger.Deadly))
            {
                return "NoPath".Translate().CapitalizeFirst();
            }
            if (!selPawn.CanReserve(this))
            {
                Pawn pawn = selPawn.Map.reservationManager.FirstRespectedReserver(this, selPawn);
                if (pawn == null)
                {
                    pawn = selPawn.Map.physicalInteractionReservationManager.FirstReserverOf(selPawn);
                }
                if (pawn != null)
                {
                    return "ReservedBy".Translate(pawn.LabelShort, pawn);
                }
                else
                {
                    return "Reserved".Translate();
                }
            }
            if (CurOccupant != null)
            {
                return "VREA.AndroidStandIsOccupied".Translate();
            }
            if (!RestUtility.CanUseBedNow(this, selPawn, checkSocialProperness: false))
            {
                return "VREA.CannotUse".Translate();
            }
            return null;
        }

    }
}
