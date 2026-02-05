using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class Hediff_DataFrag : Hediff
    {
        public override float Severity
        {
            get => base.Severity;
            set
            {
                pawn.health.hediffSet.DirtyCache();
                base.Severity = value;
            }
        }

        public override void PostRemoved()
        {
            pawn.health.hediffSet.DirtyCache();
            base.PostRemoved();
        }

        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if(pawn.Downed)
            {
                var reboot = pawn.health.hediffSet.GetFirstHediffOfDef(A2_Defof.A2_Reboot);
                if (reboot == null)
                {
                    reboot = HediffMaker.MakeHediff(A2_Defof.A2_Reboot, pawn);
                    reboot.Severity = 1f;
                    pawn.health.AddHediff(reboot);
                }
                pawn.health.RemoveHediff(this);
            }
        }
    }
}
