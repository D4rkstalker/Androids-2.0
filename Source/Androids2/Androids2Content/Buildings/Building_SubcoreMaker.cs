using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using VREAndroids;
namespace Androids2
{
    [HotSwappable]
    [StaticConstructorOnStartup]

    public class Building_SubcoreMaker : Building_SubcorePolyanalyzer
    {
        public override void Tick()
        {
            base.Tick();
            if (MotePerRotation == null)
            {
                MotePerRotation = new Dictionary<Rot4, ThingDef>
                {
                    {
                        Rot4.South,
                        VREA_DefOf.VREA_SubcorePolyanalyzer_South
                    },
                    {
                        Rot4.East,
                        VREA_DefOf.VREA_SubcorePolyanalyzer_East
                    },
                    {
                        Rot4.West,
                        VREA_DefOf.VREA_SubcorePolyanalyzer_West
                    },
                    {
                        Rot4.North,
                        VREA_DefOf.VREA_SubcorePolyanalyzer_North
                    }
                };
            }
            SubcoreScannerState state = State;
            if (state == SubcoreScannerState.Occupied)
            {
                fabricationTicksLeft--;
                scanProgress = ((1f - (fabricationTicksLeft / (float)def.building.subcoreScannerTicks)) / 4f) + (0.25f * scannedPawns.Count);
                if (fabricationTicksLeft <= 0)
                {
                    Messages.Message("VREA.MessageSubcorePolyanalyzerCompleted".Translate(Occupant.Named("PAWN"),
                        scanProgress.ToStringPercent()), Occupant, MessageTypeDefOf.PositiveEvent);
                    scannedPawns.Add(Occupant);
                    EjectContents();
                    if (scanProgress >= 1)
                    {
                        A2Subcore subcore = (A2Subcore)ThingMaker.MakeThing(def.building.subcoreScannerOutputDef);
                        subcore.InitializeWithPawns(scannedPawns);
                        GenPlace.TryPlaceThing(subcore, InteractionCell, base.Map, ThingPlaceMode.Near);
                        if (def.building.subcoreScannerComplete != null)
                        {
                            def.building.subcoreScannerComplete.PlayOneShot(this);
                        }
                        Reset();
                        innerContainer.ClearAndDestroyContents();
                    }
                }

                if (workingMote == null || workingMote.Destroyed)
                {
                    workingMote = MoteMaker.MakeAttachedOverlay(this, MotePerRotation[base.Rotation], Vector3.zero);
                }
                workingMote.Maintain();


                if (def.building.subcoreScannerWorking != null)
                {
                    if (sustainerWorking == null || sustainerWorking.Ended)
                    {
                        sustainerWorking = def.building.subcoreScannerWorking.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                    }
                    else
                    {
                        sustainerWorking.Maintain();
                    }
                }
            }

            if (state == SubcoreScannerState.Occupied)
            {
                if (def.building.subcoreScannerStartEffect != null)
                {
                    if (effectStart == null)
                    {
                        effectStart = def.building.subcoreScannerStartEffect.Spawn();
                        effectStart.Trigger(this, new TargetInfo(InteractionCell, base.Map));
                    }
                    effectStart.EffectTick(this, new TargetInfo(InteractionCell, base.Map));
                }
            }
            else
            {
                effectStart?.Cleanup();
                effectStart = null;
            }
        }


    }
}
