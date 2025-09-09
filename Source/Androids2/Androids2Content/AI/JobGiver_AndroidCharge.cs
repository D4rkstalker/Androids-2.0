using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using VREAndroids;

namespace Androids2
{
    public class JobGiver_AndroidCharge : ThinkNode_JobGiver
    {
        public const float RechargeThreshold = 0.2f;
        public override float GetPriority(Pawn pawn)
        {
            var power = pawn.needs.TryGetNeed<Need_ReactorPower>();
            if (power == null || power.CurLevelPercentage > RechargeThreshold)
            {
                return 0f;
            }
            return 999f;
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            var power = pawn.needs.TryGetNeed<Need_ReactorPower>();
            if (power == null)
            {
                return null;
            }
            Building_AndroidCharger stand = FindChargerFor(pawn);
            if (stand != null)
            {
                return JobMaker.MakeJob(A2_Defof.A2_AndroidCharge, stand);
            }
            return null;
        }

        public static Building_AndroidCharger FindChargerFor(Pawn pawn)
        {
            foreach (var charger in Building_AndroidCharger.chargers)
            {
                if (charger.CannotUseNowReason(pawn) is null
                    && pawn.CanReserveAndReach(charger, PathEndMode.OnCell, Danger.Deadly))
                {
                    return charger;
                }
            }
            return null;
        }
    }
}
