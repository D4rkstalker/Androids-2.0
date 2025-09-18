using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2
{
    /// <summary>
    /// Creates a nuclear esque explosion upon death.
    /// </summary>
    public class Hediff_Vanometric : Hediff
    {
        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(60))
            {

                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor) as Hediff_AndroidReactor;
                if (hediff != null)
                {
                    hediff.Energy += 0.01f;

                }
            }
        }
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);

            if (pawn.Corpse != null)
            {
                GenExplosion.DoExplosion(pawn.Corpse.Position, pawn.Corpse.Map, 25f, RimWorld.DamageDefOf.Bomb, null, 500, 15);
            }
        }
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            BodyPartRecord stomach = pawn.health.hediffSet.GetBodyPartRecord(A2_Defof.Stomach);
            if (pawn.health.hediffSet.PartIsMissing(stomach))
            {
                GenExplosion.DoExplosion(pawn.Corpse.Position, pawn.Corpse.Map, 25f, RimWorld.DamageDefOf.Bomb, null, 500, 15);
            }

        }

    }
}
