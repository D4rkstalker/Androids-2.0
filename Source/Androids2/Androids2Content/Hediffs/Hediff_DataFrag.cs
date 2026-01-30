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
    }
}
