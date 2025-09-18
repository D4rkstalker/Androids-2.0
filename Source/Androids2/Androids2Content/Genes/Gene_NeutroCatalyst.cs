using RimWorld;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class Gene_NeutroCatalyst : Gene
    {
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            var neutroloss = pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_NeutroLoss);
            if (neutroloss != null)
            {
                var refillRate = 0.001f * delta * (neutroloss.def.maxSeverity - neutroloss.Severity);
                Log.Warning(Label + " refilling neutro loss by " + refillRate );
                neutroloss.Severity -= refillRate;
            }
        }
    }
}
