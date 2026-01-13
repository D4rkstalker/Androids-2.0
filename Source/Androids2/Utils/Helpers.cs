using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;
namespace Androids2
{
    [StaticConstructorOnStartup]
    public static class Helpers
    {
        //public static readonly Texture2D PowerEfficiencyIcon = ContentFinder<Texture2D>.Get("UI/BiostatIcon/BiostatEfficiency");
        //public static readonly Texture2D ResourceCostIcon = ContentFinder<Texture2D>.Get("UI/BiostatIcon/BiostatResourceCost");
        //public static readonly CachedTexture PowerEfficiencyIconTex = new CachedTexture("UI/BiostatIcon/BiostatEfficiency");

        public static bool IsDroid(this Pawn pawn)
        {

            if (pawn.HasActiveGene(A2_Defof.VREA_A2_BasicDroid) || pawn.HasActiveGene(A2_Defof.VREA_A2_AdvancedDroid))
            {
                return true;
            }
            return false;
        }

        public static int Unpatch(MethodInfo original, string declaringTypeFullName)
        {
            if (original == null)
            {
                Log.Warning($"[Unpatch] Could not find original method for {declaringTypeFullName}");
                return 0;
            }

            var info = Harmony.GetPatchInfo(original);
            if (info == null)
            {
                Log.Message($"[Unpatch] No patches found on {original.DeclaringType?.FullName}.{original.Name}");
                return 0;
            }

            int removed = 0;

            // Local that checks the declaring type and unpatches that exact patch method
            void RemoveAllFrom(IEnumerable<Patch> patches)
            {
                foreach (var p in patches.ToList()) // ToList so we can modify while iterating
                {
                    var decl = p.PatchMethod?.DeclaringType?.FullName;
                    if (decl == declaringTypeFullName)
                    {
                        Androids2.harmony.Unpatch(original, p.PatchMethod);
                        removed++;
                    }
                }
            }

            RemoveAllFrom(info.Prefixes);
            RemoveAllFrom(info.Postfixes);
            RemoveAllFrom(info.Transpilers);
            RemoveAllFrom(info.Finalizers);

            if (removed > 0)
                Log.Message($"[Unpatch] Removed {removed} patch(es) from declaring type {declaringTypeFullName} on {original.DeclaringType?.FullName}.{original.Name}");
            else
                Log.Message($"[Unpatch] No matching patch methods found for {declaringTypeFullName} (already removed or different name).");

            return removed;
        }

    }
}
