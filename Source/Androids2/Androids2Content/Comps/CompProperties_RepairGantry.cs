using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Androids2.Androids2Content.Comps
{
    public class CompProperties_RepairGantry : CompProperties
    {
        // Token: 0x0600FD3D RID: 64829 RVA: 0x00496F23 File Offset: 0x00495123
        public CompProperties_RepairGantry()
        {
            this.compClass = typeof(CompBiosculpterPod);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string item in base.ConfigErrors(parentDef))
            {
                yield return item;
            }

            if (parentDef.tickerType != TickerType.Normal)
            {
                yield return GetType().Name + " requires parent ticker type Normal";
            }
        }
        // Token: 0x0400A613 RID: 42515
        public SoundDef enterSound;

        // Token: 0x0400A614 RID: 42516
        public SoundDef exitSound;

        // Token: 0x0400A615 RID: 42517
        public EffecterDef operatingEffecter;

        // Token: 0x0400A616 RID: 42518
        public EffecterDef readyEffecter;

        // Token: 0x0400A617 RID: 42519
        public Color selectCycleColor;

        // Token: 0x0400A618 RID: 42520
        public float biotunedCycleSpeedFactor;
    }
    public enum RepairGantryState
    {
        // Token: 0x0400A623 RID: 42531
        LoadingMaterials,
        // Token: 0x0400A624 RID: 42532
        SelectingCycle,
        // Token: 0x0400A625 RID: 42533
        Occupied
    }

}
