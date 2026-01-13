using System;
using System.Collections.Generic;
using Verse;

namespace Androids2.Androids2Content.Comps
{
    // Token: 0x02002C31 RID: 11313
    public class CompProperties_RepairGantry_RepairCycle : CompProperties_RepairGantry_BaseCycle
    {
        // Token: 0x0400A681 RID: 42625
        public List<BodyPartDef> bodyPartsToRestore;

        // Token: 0x0400A682 RID: 42626
        public List<HediffDef> conditionsToPossiblyCure;
    }
}
