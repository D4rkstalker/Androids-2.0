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
    [StaticConstructorOnStartup]
    public class Building_AndroidCharger : Building
    {
        // Token: 0x17002539 RID: 9529
        // (get) Token: 0x0600EF81 RID: 61313 RVA: 0x00458548 File Offset: 0x00456748
        public CompPowerTrader Power
        {
            get
            {
                return this.TryGetComp<CompPowerTrader>();
            }
        }

        // Token: 0x1700253A RID: 9530
        // (get) Token: 0x0600EF82 RID: 61314 RVA: 0x00458550 File Offset: 0x00456750
        public bool IsPowered
        {
            get
            {
                return Power.PowerOn;
            }
        }

        // Token: 0x1700253D RID: 9533
        // (get) Token: 0x0600EF85 RID: 61317 RVA: 0x0045859C File Offset: 0x0045679C
        public CompThingContainer Container
        {
            get
            {
                if (container == null)
                {
                    container = base.GetComp<CompThingContainer>();
                }
                return container;
            }
        }

        // Token: 0x1700253E RID: 9534
        // (get) Token: 0x0600EF86 RID: 61318 RVA: 0x004585B8 File Offset: 0x004567B8
        public GenDraw.FillableBarRequest BarDrawData
        {
            get
            {
                return def.building.BarDrawDataFor(base.Rotation);
            }
        }

        // Token: 0x1700253F RID: 9535
        // (get) Token: 0x0600EF87 RID: 61319 RVA: 0x004585D0 File Offset: 0x004567D0
        private Material WireMaterial
        {
            get
            {
                if (wireMaterial == null)
                {
                    wireMaterial = MaterialPool.MatFrom("Other/BundledWires", ShaderDatabase.Transparent, Color.white);
                }
                return wireMaterial;
            }
        }

        // Token: 0x17002540 RID: 9536
        // (get) Token: 0x0600EF88 RID: 61320 RVA: 0x00458600 File Offset: 0x00456800
        private bool IsAttachedToMech
        {
            get
            {
                return currentlyChargingMech != null && wireExtensionTicks >= 70;
            }
        }

        // Token: 0x17002544 RID: 9540
        // (get) Token: 0x0600EF8C RID: 61324 RVA: 0x0045866B File Offset: 0x0045686B
        public Pawn CurrentlyChargingMech
        {
            get
            {
                return currentlyChargingMech;
            }
        }

        // Token: 0x0600EF8D RID: 61325 RVA: 0x00458673 File Offset: 0x00456873
        public override void PostPostMake()
        {
            if (!ModLister.CheckBiotech("Mech recharger"))
            {
                Destroy(DestroyMode.Vanish);
                return;
            }
            base.PostPostMake();
        }

        // Token: 0x0600EF8E RID: 61326 RVA: 0x00458690 File Offset: 0x00456890
        public bool CanPawnChargeCurrently(Pawn pawn)
        {
            if (Power.PowerNet == null)
            {
                return false;
            }
            if (IsPowered)
            {
                if (!pawn.IsAndroid() || !pawn.HasActiveGene(A2_Defof.A2_BatteryPower))
                {
                    return false;
                }

                if (currentlyChargingMech == null)
                {
                    return true;
                }
                if (currentlyChargingMech == pawn)
                {
                    return true;
                }
            }
            return false;
        }

        // Token: 0x0600EF92 RID: 61330 RVA: 0x0045874C File Offset: 0x0045694C
        public override void Tick()
        {
            base.Tick();
            if (currentlyChargingMech != null && (currentlyChargingMech.CurJobDef != A2_Defof.A2_AndroidCharge || currentlyChargingMech.CurJob.targetA.Thing != this))
            {
                Log.Warning("Mech did not clean up his charging job properly");
                StopCharging();
            }
            if (currentlyChargingMech != null && Power.PowerOn)
            {
                var hediff = currentlyChargingMech.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor) as Hediff_AndroidReactor;
                if (hediff != null)
                {
                    hediff.Energy += 0.001f;
                }

                if (moteCablePulse == null || moteCablePulse.Destroyed)
                {
                    moteCablePulse = MoteMaker.MakeInteractionOverlay(ThingDefOf.Mote_ChargingCablesPulse, this, new TargetInfo(InteractionCell, base.Map, false));
                }
                Mote mote = moteCablePulse;
                if (mote != null)
                {
                    mote.Maintain();
                }
            }
            if (currentlyChargingMech != null && Power.PowerOn && IsAttachedToMech)
            {
                if (sustainerCharging == null)
                {
                    sustainerCharging = SoundDefOf.MechChargerCharging.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.None));
                }
                sustainerCharging.Maintain();
                if (moteCharging == null || moteCharging.Destroyed)
                {
                    moteCharging = MoteMaker.MakeAttachedOverlay(currentlyChargingMech, ThingDefOf.Mote_MechCharging, Vector3.zero, 1f, -1f);
                }
                Mote mote2 = moteCharging;
                if (mote2 != null)
                {
                    mote2.Maintain();
                }
            }
            else if (sustainerCharging != null && (currentlyChargingMech == null || !Power.PowerOn))
            {
                sustainerCharging.End();
                sustainerCharging = null;
            }
            if (wireExtensionTicks < 70)
            {
                wireExtensionTicks++;
            }
        }
        // Token: 0x0600EF95 RID: 61333 RVA: 0x004589B8 File Offset: 0x00456BB8
        public void StartCharging(Pawn mech)
        {
            if (!ModLister.CheckBiotech("Mech charging"))
            {
                return;
            }
            if (currentlyChargingMech != null)
            {
                Log.Error("Tried charging on already charging mech charger!");
                return;
            }
            if (!mech.IsColonyMech)
            {
                mech.jobs.EndCurrentJob(JobCondition.Incompletable, true, true);
                return;
            }
            currentlyChargingMech = mech;

            wireExtensionTicks = 0;
            SoundDefOf.MechChargerStart.PlayOneShot(this);
        }

        // Token: 0x0600EF96 RID: 61334 RVA: 0x00458A2C File Offset: 0x00456C2C
        public void StopCharging()
        {
            if (currentlyChargingMech == null)
            {
                Log.Error("Tried stopping charging on currently not charging mech charger!");
                return;
            }
            currentlyChargingMech = null;
            wireExtensionTicks = 0;
        }

        // Token: 0x0600EF97 RID: 61335 RVA: 0x00458A84 File Offset: 0x00456C84
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (base.BeingTransportedOnGravship)
            {
                base.DeSpawn(mode);
                return;
            }
            if (currentlyChargingMech != null && mode != DestroyMode.WillReplace)
            {
                Messages.Message("MessageMechChargerDestroyedMechGoesBerserk".Translate(currentlyChargingMech.Named("PAWN")), new LookTargets(currentlyChargingMech), MessageTypeDefOf.NegativeEvent, true);
                currentlyChargingMech.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.BerserkMechanoid, null, false, false, false, null, false, false, false);
            }
            base.DeSpawn(mode);
        }
        // Token: 0x0600EF9C RID: 61340 RVA: 0x00458CBC File Offset: 0x00456EBC
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref currentlyChargingMech, "currentlyChargingMech", false);
            Scribe_Values.Look<int>(ref wireExtensionTicks, "wireExtensionTicks", 0, false);
        }

        // Token: 0x04009E3A RID: 40506
        private Pawn currentlyChargingMech;

        // Token: 0x04009E3C RID: 40508
        private int wireExtensionTicks = 70;

        // Token: 0x04009E3E RID: 40510
        private CompThingContainer container;

        // Token: 0x04009E3F RID: 40511
        private Sustainer sustainerCharging;

        // Token: 0x04009E40 RID: 40512
        private Mote moteCharging;

        // Token: 0x04009E41 RID: 40513
        private Mote moteCablePulse;

        // Token: 0x04009E42 RID: 40514
        public const float ChargePerDay = 50f;

        // Token: 0x04009E48 RID: 40520
        private Material wireMaterial;
    }

}
