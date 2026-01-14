using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Androids2
{
    public abstract class CompRepairGantry_Cycle : ThingComp
    {
        public List<string> tmpMissingResearchLabels = new List<string>();

        public CompProperties_RepairGantry_BaseCycle Props => (CompProperties_RepairGantry_BaseCycle)props;

        public abstract void CycleCompleted(Pawn occupant);

        public virtual string Description(Pawn tunedFor)
        {
            return Props.description;
        }

        public List<string> MissingResearchLabels()
        {
            tmpMissingResearchLabels.Clear();
            if (Props.requiredResearch.NullOrEmpty())
            {
                return tmpMissingResearchLabels;
            }

            foreach (ResearchProjectDef item in Props.requiredResearch)
            {
                if (!item.IsFinished)
                {
                    tmpMissingResearchLabels.Add(item.LabelCap);
                }
            }

            return tmpMissingResearchLabels;
        }

        public CompRepairGantry_Cycle()
        {
        }
    }

}

