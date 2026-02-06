using RimWorld;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class Gene_SynthFlesh : Gene
    {
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.IsHashIntervalTick(300, delta))
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury injury)
                    {
                        hediff.Heal(0.1f);
                    }
                }
            }
        }
    }
}
