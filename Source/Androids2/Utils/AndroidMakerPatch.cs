
using Androids2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2.Utils
{
    public class AndroidMakerPatch
    {
        public static void ApplyXenotype(Pawn pawn, CustomXenotype curAndroidProject, bool neutroLoss = true)
        {

            if (pawn == null)
            {
                Log.Warning("Androids2: AndroidMakerPatch received a null pawn from UI, generating a new one.");
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    VREA_DefOf.VREA_AndroidBasic,
                    Faction.OfPlayer,
                    allowDowned: true,
                    allowAddictions: false));

                pawn.apparel?.wornApparel?.Clear();
                pawn.equipment?.equipment?.Clear();
                pawn.inventory?.innerContainer?.Clear();
            }
            foreach (var gene in VREAndroids.Utils.allAndroidGenes)
            {
                var existing = pawn.genes.GetGene(gene);
                if (existing != null)
                    pawn.genes.RemoveGene(existing);
            }
            int skillFloor = 0;

            foreach (var gene in curAndroidProject.genes.OrderByDescending(g => !g.CanBeRemovedFromAndroid()))
            {
                pawn.genes.AddGene(gene, true);
                //Log.Warning("Adding gene: " + gene.defName);
                if (gene.GetModExtension<SkillFloor>() is SkillFloor extension)
                {
                    //Log.Warning("setting skillfloor: " + extension.floor);
                    skillFloor = extension.floor;
                }

            }

            var geneSyntheticBody = pawn.genes?.GetGene(VREA_DefOf.VREA_SyntheticBody) as Gene_SyntheticBody;
            if (geneSyntheticBody != null)
            {
                //pawn.story.Adulthood = null;
                //if (pawn.IsAwakened())
                //{
                //    VREAndroids.Utils.TryAssignBackstory(pawn, "AwakenedAndroid");
                //}
                //else
                //{
                //    VREAndroids.Utils.TryAssignBackstory(pawn, "ColonyAndroid");
                //}
                var years = Rand.Range(0f, 25f);
                pawn.ageTracker.AgeBiologicalTicks = (long)(years * 3600000f);
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
                pawn.Notify_DisabledWorkTypesChanged();
            }

            if (pawn.HasActiveGene(VREA_DefOf.VREA_NoSkillGain))
            {
                List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    SkillDef skillDef = allDefsListForReading[i];
                    var skillRecord = pawn.skills.GetSkill(skillDef);
                    skillRecord.Level = skillFloor;
                    skillRecord.passion = Passion.None;
                }
            }
            else if (geneSyntheticBody != null)
            {
                List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    SkillDef skillDef = allDefsListForReading[i];
                    var skillRecord = pawn.skills.GetSkill(skillDef);
                    skillRecord.Level = FinalLevelOfSkill(pawn, skillDef) + skillFloor;
                    skillRecord.passion = Passion.None;
                }
                if (pawn.IsAwakened())
                {
                    var majorPassions = 2;
                    var minorPassions = 2;

                    foreach (SkillRecord item in pawn.skills.skills.OrderByDescending((SkillRecord sr) =>
                        sr.GetLevel(includeAptitudes: false)))
                    {
                        if (item.TotallyDisabled)
                        {
                            continue;
                        }
                        bool flag = false;
                        foreach (Trait allTrait2 in pawn.story.traits.allTraits)
                        {
                            if (allTrait2.def.ConflictsWithPassion(item.def))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (ModsConfig.BiotechActive && pawn.genes != null)
                        {
                            foreach (Gene item2 in pawn.genes.GenesListForReading)
                            {
                                if (item2.Active && item2.def.passionMod != null && item2.def.passionMod.modType == PassionMod.PassionModType.DropAll && item2.def.passionMod.skill == item.def)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            CreatePassion(item, force: false);
                        }
                    }

                    void CreatePassion(SkillRecord record, bool force)
                    {
                        if (majorPassions > 0)
                        {
                            record.passion = Passion.Major;
                            majorPassions--;
                        }
                        else if (minorPassions > 0 || force)
                        {
                            record.passion = Passion.Minor;
                            minorPassions--;
                        }
                    }
                }
            }
            
            pawn.ageTracker.AgeBiologicalTicks = 0;
            pawn.ageTracker.AgeChronologicalTicks = 0;

            pawn.genes.xenotypeName = curAndroidProject.name;
            pawn.genes.iconDef = curAndroidProject.IconDef;
 
            if (pawn.genes.GetGene(VREA_DefOf.VREA_NeutroCirculation) != null && neutroLoss)
            {
                var neutroloss = HediffMaker.MakeHediff(VREA_DefOf.VREA_NeutroLoss, pawn);
                neutroloss.Severity = 1f;
                pawn.health.AddHediff(neutroloss);

            }




        }
        public static void ApplyXenotype(Pawn pawn, bool neutroLoss = true)
        {

            if (pawn == null)
            {
                Log.Warning("Androids2: AndroidMakerPatch received a null pawn from UI, generating a new one.");
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    VREA_DefOf.VREA_AndroidBasic,
                    Faction.OfPlayer,
                    allowDowned: true,
                    allowAddictions: false));

                pawn.apparel?.wornApparel?.Clear();
                pawn.equipment?.equipment?.Clear();
                pawn.inventory?.innerContainer?.Clear();
            }
            int skillFloor = 0;

            foreach (Gene gene in pawn.genes.GenesListForReading)
            {
                if (gene.def.GetModExtension<SkillFloor>() is SkillFloor extension)
                {
                    //Log.Warning("setting skillfloor: " + extension.floor);
                    skillFloor = extension.floor;
                }

            }

            var geneSyntheticBody = pawn.genes?.GetGene(VREA_DefOf.VREA_SyntheticBody) as Gene_SyntheticBody;
            if (geneSyntheticBody != null)
            {
                //pawn.story.Adulthood = null;
                //if (pawn.IsAwakened())
                //{
                //    VREAndroids.Utils.TryAssignBackstory(pawn, "AwakenedAndroid");
                //}
                //else
                //{
                //    VREAndroids.Utils.TryAssignBackstory(pawn, "ColonyAndroid");
                //}
                var years = Rand.Range(0f, 25f);
                pawn.ageTracker.AgeBiologicalTicks = (long)(years * 3600000f);
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
                pawn.Notify_DisabledWorkTypesChanged();
            }

            if (pawn.HasActiveGene(VREA_DefOf.VREA_NoSkillGain))
            {
                List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    SkillDef skillDef = allDefsListForReading[i];
                    var skillRecord = pawn.skills.GetSkill(skillDef);
                    skillRecord.Level = skillFloor;
                    skillRecord.passion = Passion.None;
                }
            }
            else if (geneSyntheticBody != null)
            {
                List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    SkillDef skillDef = allDefsListForReading[i];
                    var skillRecord = pawn.skills.GetSkill(skillDef);
                    skillRecord.Level = FinalLevelOfSkill(pawn, skillDef) + skillFloor;
                    skillRecord.passion = Passion.None;
                }
                if (pawn.IsAwakened())
                {
                    var majorPassions = 2;
                    var minorPassions = 2;

                    foreach (SkillRecord item in pawn.skills.skills.OrderByDescending((SkillRecord sr) =>
                        sr.GetLevel(includeAptitudes: false)))
                    {
                        if (item.TotallyDisabled)
                        {
                            continue;
                        }
                        bool flag = false;
                        foreach (Trait allTrait2 in pawn.story.traits.allTraits)
                        {
                            if (allTrait2.def.ConflictsWithPassion(item.def))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (ModsConfig.BiotechActive && pawn.genes != null)
                        {
                            foreach (Gene item2 in pawn.genes.GenesListForReading)
                            {
                                if (item2.Active && item2.def.passionMod != null && item2.def.passionMod.modType == PassionMod.PassionModType.DropAll && item2.def.passionMod.skill == item.def)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            CreatePassion(item, force: false);
                        }
                    }

                    void CreatePassion(SkillRecord record, bool force)
                    {
                        if (majorPassions > 0)
                        {
                            record.passion = Passion.Major;
                            majorPassions--;
                        }
                        else if (minorPassions > 0 || force)
                        {
                            record.passion = Passion.Minor;
                            minorPassions--;
                        }
                    }
                }
            }

            pawn.ageTracker.AgeBiologicalTicks = 0;
            pawn.ageTracker.AgeChronologicalTicks = 0;

            if (pawn.genes.GetGene(VREA_DefOf.VREA_NeutroCirculation) != null && neutroLoss)
            {
                var neutroloss = HediffMaker.MakeHediff(VREA_DefOf.VREA_NeutroLoss, pawn);
                neutroloss.Severity = 1f;
                pawn.health.AddHediff(neutroloss);

            }
        }

        private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num = 0;
            foreach (BackstoryDef item in pawn.story.AllBackstories.Where((BackstoryDef bs) => bs != null))
            {
                foreach (var skillGain in item.skillGains)
                {
                    if (skillGain.skill == sk)
                    {
                        num += (float)skillGain.amount;
                    }
                }
            }
            for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
            {
                if (!pawn.story.traits.allTraits[i].Suppressed
                    && pawn.story.traits.allTraits[i].CurrentData.skillGains.FirstOrDefault(x => x.skill == sk) is SkillGain skillGain)
                {
                    num += (float)skillGain.amount;

                }
            }
            if (num > 0f)
            {
                num += (float)pawn.kindDef.extraSkillLevels;
            }
            if (pawn.kindDef.skills != null)
            {
                foreach (SkillRange skill in pawn.kindDef.skills)
                {
                    if (skill.Skill == sk)
                    {
                        if (num < (float)skill.Range.min || num > (float)skill.Range.max)
                        {
                            num = skill.Range.RandomInRange;
                        }
                        break;
                    }
                }
            }
            return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
        }

    }
}
