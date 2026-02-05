using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class HediffComp_Reboot : HediffComp
    {
        public HediffCompProperties_Reboot Props => (HediffCompProperties_Reboot)props;

        public override void CompPostPostRemoved()
        {
            var memorySpace = Pawn.needs.TryGetNeed<Need_MemorySpace>();
            if (memorySpace != null)
            {
                memorySpace.curLevelInt = 1f;
            }

        }

    }
}
