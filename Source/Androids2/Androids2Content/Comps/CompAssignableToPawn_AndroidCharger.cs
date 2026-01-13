using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class CompAssignableToPawn_AndroidCharger : CompAssignableToPawn_Bed
    {
        public override void PostPostExposeData()
        {
        }
        public override IEnumerable<Pawn> AssigningCandidates
        {
            get
            {
                if (!parent.Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }
                return parent.Map.mapPawns.FreeColonists.Where((Pawn p) => p.HasActiveGene(VREA_DefOf.VREA_MemoryProcessing) || p.HasActiveGene(VREA_DefOf.VREA_Power)) ;
            }
        }

        public override string GetAssignmentGizmoDesc()
        {
            return "VREA.CommandAndroidStandSetOwnerDesc".Translate();
        }

        public override bool AssignedAnything(Pawn pawn)
        {
            foreach (var other in Building_AndroidCharger.chargers)
            {
                if (other.CompAssignableToPawn.AssignedPawns.Contains(pawn))
                {
                    return true;
                }
            }
            return false;
        }

        public override void TryAssignPawn(Pawn pawn)
        {
            Building_AndroidCharger stand = (Building_AndroidCharger)parent;
            foreach (var assignedPawn in stand.CompAssignableToPawn.AssignedPawns.ToList())
            {
                stand.CompAssignableToPawn.ForceRemovePawn(assignedPawn);
            }
            foreach (var other in Building_AndroidCharger.chargers)
            {
                if (other.CompAssignableToPawn.AssignedPawns.Contains(pawn))
                {
                    other.CompAssignableToPawn.ForceRemovePawn(pawn);
                }
            }
            stand.CompAssignableToPawn.ForceAddPawn(pawn);
        }

        public override void TryUnassignPawn(Pawn pawn, bool sort = true, bool uninstall = false)
        {
            Building_AndroidCharger stand = (Building_AndroidCharger)parent;
            stand.CompAssignableToPawn.ForceRemovePawn(pawn);
        }

        public override bool ShouldShowAssignmentGizmo()
        {
            return parent.Faction == Faction.OfPlayer;
        }
    }
}
