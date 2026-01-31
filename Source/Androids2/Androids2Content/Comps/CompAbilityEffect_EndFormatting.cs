using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VREAndroids;
using static UnityEngine.TouchScreenKeyboard;

namespace Androids2
{
    public class CompAbilityEffect_EndFormatting : CompAbilityEffect
    {
        public new CompProperties_EndFormatting Props => (CompProperties_EndFormatting)props;

        public Pawn Pawn => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (Pawn.MentalStateDef == VREA_DefOf.VREA_Reformatting)
            {
                var memorySpace = Pawn.needs.TryGetNeed<Need_MemorySpace>();
                memorySpace.curLevelInt += 0.01f;
                Pawn.MentalState?.RecoverFromState();
                var frag = Pawn.health.hediffSet.GetFirstHediffOfDef(A2_Defof.A2_DataFragmentation);
                if (frag != null)
                {
                    frag.Severity += 0.3f;

                }
                else
                {
                    frag = HediffMaker.MakeHediff(A2_Defof.A2_DataFragmentation, Pawn);
                    frag.Severity = 0.3f;
                    Pawn.health.AddHediff(frag);

                }

                base.Apply(target, dest);

            }
        }
    }
}
