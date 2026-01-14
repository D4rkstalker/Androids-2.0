using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Androids2
{

    public class CompProperties_RepairGantry_GantryCycle : CompProperties_RepairGantry_BaseCycle
    {
        public List<BodyPartDef> bodyPartsToRestore;

        public List<HediffDef> conditionsToPossiblyCure;
    }
}
