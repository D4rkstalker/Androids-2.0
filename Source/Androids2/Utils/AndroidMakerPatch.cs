﻿
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
        public static void ApplyXenotype(Pawn pawn, List<GeneDef> genes, bool neutroLoss = true)
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
            Debug.LogWarning(pawn.story.adulthood?.defName + " " + pawn.story.childhood?.defName);
            foreach (var gene in genes.OrderByDescending(g => !g.CanBeRemovedFromAndroid()))
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
                var years = Rand.Range(0f, 25f);
                pawn.ageTracker.AgeBiologicalTicks = (long)(years * 3600000f);
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
                pawn.Notify_DisabledWorkTypesChanged();
            }

            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                SkillDef skillDef = allDefsListForReading[i];
                var skillRecord = pawn.skills.GetSkill(skillDef);
                skillRecord.Level = FinalLevelOfSkill(pawn, skillDef) + skillFloor;
                if (pawn.HasActiveGene(VREA_DefOf.VREA_NoSkillGain))
                {
                    skillRecord.passion = Passion.None;

                }
                else
                {
                    skillRecord.passion = Passion.None;
                    if (!skillRecord.TotallyDisabled)
                    {
                        float num2 = (float)skillRecord.Level * 0.11f;
                        float value = Rand.Value;
                        if (value < num2)
                        {
                            if (value < num2 * 0.2f)
                            {
                                skillRecord.passion = Passion.Major;
                            }
                            else
                            {
                                skillRecord.passion = Passion.Minor;
                            }
                        }
                        skillRecord.xpSinceLastLevel = Rand.Range(skillRecord.XpRequiredForLevelUp * 0.1f, skillRecord.XpRequiredForLevelUp * 0.9f);
                    }
                }
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


            pawn.ageTracker.AgeBiologicalTicks = 0;
            pawn.ageTracker.AgeChronologicalTicks = 0;


            if (pawn.genes.GetGene(VREA_DefOf.VREA_NeutroCirculation) != null && neutroLoss)
            {
                var neutroloss = HediffMaker.MakeHediff(VREA_DefOf.VREA_NeutroLoss, pawn);
                neutroloss.Severity = 1f;
                pawn.health.AddHediff(neutroloss);

            }

            if (pawn.genes.HasActiveGene(A2_Defof.A2_BatteryPower))
            {
                pawn.health.AddHediff(A2_Defof.A2_HediffBattery, pawn.health.hediffSet.GetBodyPartRecord(A2_Defof.Stomach));
                pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(VREA_DefOf.VREA_Reactor));
            }


        }
        private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num = 0;
            Debug.LogWarning("Calculating final level of skill: " + sk.defName);
            foreach (BackstoryDef item in pawn.story.AllBackstories.Where((BackstoryDef bs) => bs != null))
            {
                Debug.LogWarning("Checking backstory: " + item.defName);
                foreach (var skillGain in item.skillGains)
                {
                    if (skillGain.skill == sk)
                    {
                        Debug.LogWarning("Found skill gain in backstory: " + item.defName + " for skill: " + sk.defName);
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
