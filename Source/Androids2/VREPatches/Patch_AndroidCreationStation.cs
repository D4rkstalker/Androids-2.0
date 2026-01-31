using Androids2.Androids2Content;
using Androids2.Utils;
using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using Verse;
using VREAndroids;


namespace Androids2.VREPatches
{
    [HarmonyPatch(typeof(Building_AndroidCreationStation), "CallAndroidCreationWindow")]
    public static class Patch_CallAndroidCreationWindow
    {
        public static bool Prefix(Building_AndroidCreationStation __instance, Pawn creator)
        {
            //Debug.LogError("Androids2: Patching CallAndroidCreationWindow to use custom window.");
            Find.WindowStack.Add(new Window_CustomizeAndroid(__instance, creator, null));
            return false;
        }
    }
    [HarmonyPatch(typeof(Building_AndroidCreationStation), "FinishAndroidProject")]
    public static class Patch_FinishAndroidProject
    {
        public static bool Prefix(Building_AndroidCreationStation __instance)
        {
            var android = PawnGenerator.GeneratePawn(new PawnGenerationRequest(VREA_DefOf.VREA_AndroidBasic, Faction.OfPlayer,
                allowDowned: true, allowAddictions: false));
            android.apparel.wornApparel.Clear();
            android.equipment.equipment.Clear();
            android.inventory.innerContainer.Clear();

            android.ageTracker.AgeBiologicalTicks = 0;
            android.ageTracker.AgeChronologicalTicks = 0;
            var neutroloss = HediffMaker.MakeHediff(VREA_DefOf.VREA_NeutroLoss, android);
            neutroloss.Severity = 1;
            android.health.AddHediff(neutroloss);
            android.genes.xenotypeName = __instance.curAndroidProject.name;
            android.genes.iconDef = __instance.curAndroidProject.IconDef;
            foreach (var gene in VREAndroids.Utils.allAndroidGenes)
            {
                var existingGene = android.genes.GetGene(gene);
                if (existingGene != null)
                {
                    android.genes.RemoveGene(existingGene);
                }
            }

            foreach (GeneDef gene in __instance.curAndroidProject.genes.OrderByDescending(x => x.CanBeRemovedFromAndroid() is false).ToList())
            {
                android.genes.AddGene(gene, true);
            }
            __instance.curAndroidProject = null;
            GenSpawn.Spawn(android, __instance.Position, __instance.Map);
            __instance.currentWorkAmountDone = 0;
            __instance.totalWorkAmount = 0;

            foreach(Thing t in __instance.unfinishedAndroid.resources){
                if(t is A2Subcore core)
                {
                    if( core.srs.Count > 0)
                    {
                        foreach(SkillRecord record in core.srs)
                        {
                            foreach (SkillRecord androidRecord in android.skills.skills)
                            {
                                if(androidRecord.def == record.def )
                                {
                                    if(androidRecord.passion < record.passion)
                                        androidRecord.passion = record.passion;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }

            __instance.unfinishedAndroid?.Destroy();
            __instance.unfinishedAndroid = null;

            return false; // skip original method
        }
    }

}
