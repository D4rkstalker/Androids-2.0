using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VREAndroids;
namespace Androids2
{
    public static class Helpers
    {
        public static bool IsDroid(this Pawn pawn)
        {

            if (pawn.HasActiveGene(A2_Defof.A2_BasicDroid) || pawn.HasActiveGene(A2_Defof.A2_AdvancedDroid))
            {
                return true;
            }
            return false;
        }


    }
}
