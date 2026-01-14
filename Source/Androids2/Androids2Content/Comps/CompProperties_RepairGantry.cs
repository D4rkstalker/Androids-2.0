using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Androids2
{
    public class CompProperties_RepairGantry : CompProperties
    {
        public SoundDef enterSound;

        public SoundDef exitSound;

        public EffecterDef operatingEffecter;

        public EffecterDef readyEffecter;

        public Color selectCycleColor;

        public float biotunedCycleSpeedFactor;

        public CompProperties_RepairGantry()
        {
            compClass = typeof(CompRepairGantry);
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
    }
}
