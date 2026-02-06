using RimWorld;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class Gene_AuxMechanite : Gene
    {
        private uint cycles_since_last_repair = 0;
        private bool has_synthflesh = false;
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.IsHashIntervalTick(360, delta))
            {
                has_synthflesh = pawn.genes.HasActiveGene(A2_Defof.VREA_A2_SynthFlesh);
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury injury && injury.Bleeding)
                    {
                        injury.Tended(1f, 1f);
                        return;
                    }
                    else if (has_synthflesh && hediff is Hediff_MissingPart _injury)
                    {
                        if(cycles_since_last_repair >= 10)
                        {
                            pawn.health.RestorePart(_injury.Part);
                            cycles_since_last_repair = 0;
                            return;
                        }
                    }
                }
                cycles_since_last_repair++;

            }
        }
    }
}
