using RimWorld;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class Gene_ReconstructionMechanites : Gene
    {
        private uint cycles_since_last_repair = 0;
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.IsHashIntervalTick(360, delta))
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury injury && injury.Bleeding)
                    {
                        injury.Tended(1f, 1f);
                    }
                    else if (hediff is Hediff_MissingPart _injury)
                    {
                        if(cycles_since_last_repair >= 10)
                        {
                            pawn.health.RestorePart(_injury.Part);
                            cycles_since_last_repair = 0;
                        }
                    }
                }
                cycles_since_last_repair++;

            }
        }
    }
}
