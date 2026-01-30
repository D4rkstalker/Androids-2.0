using RimWorld;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class Gene_ReconstructionMechanites : Gene
    {
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.IsHashIntervalTick(3600, delta))
            {
                foreach(Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_MissingPart injury)
                    {
                        pawn.health.RestorePart(injury.Part);
                        break;
                    }
                }
            }
        }
    }
}
