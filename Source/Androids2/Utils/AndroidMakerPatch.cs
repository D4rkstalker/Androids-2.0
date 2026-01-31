
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
        public static void ApplyXenotype(Pawn pawn, List<GeneDef> genes, bool neutroLoss = true, bool changeAge = true, bool keepBaseSkill = false)
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
            Log.Warning(pawn.story.adulthood?.defName + " " + pawn.story.childhood?.defName);
            foreach (var gene in VREAndroids.Utils.allAndroidGenes)
            {
                var existing = pawn.genes.GetGene(gene);
                if (existing != null)
                    pawn.genes.RemoveGene(existing);
            }
            int skillFloor = 0;
            int skillCeiling = 20;
            bool isNeuralLocked = false;
            foreach (var gene in genes.OrderByDescending(g => !g.CanBeRemovedFromAndroid()))
            {
                //if(pawn.genes.GetGene(gene) != null)
                //{
                //    Log.Warning("Skipping duplicate gene: " + gene.defName);
                //    continue;
                //}
                pawn.genes.AddGene(gene, false);
                //Log.Warning("Adding gene: " + gene.defName);
                if (gene.GetModExtension<SkillFloor>() is SkillFloor extension)
                {
                    Log.Warning("setting skillfloor: " + extension.floor);
                    Log.Warning("setting skillceilling: " + extension.ceiling);
                    if(extension.floor > skillFloor)
                        skillFloor = extension.floor;
                    if(extension.ceiling < skillCeiling)
                        skillCeiling = extension.ceiling;
                }
                if (gene == A2_Defof.VREA_A2_NeuralLock)
                {
                    isNeuralLocked |= true;
                }

            }

            var geneSyntheticBody = pawn.genes?.GetGene(VREA_DefOf.VREA_SyntheticBody) as Gene_SyntheticBody;
            if (geneSyntheticBody != null && changeAge)
            {
                var years = Rand.Range(0f, 25f);
                pawn.ageTracker.AgeBiologicalTicks = (long)(years * 3600000f);
                pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            }

            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            Log.Warning("skill defs count: " + allDefsListForReading.Count);
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                SkillDef skillDef = allDefsListForReading[i];
                var skillRecord = pawn.skills.GetSkill(skillDef);
                int tempLevel = FinalLevelOfSkill(pawn, skillDef);
                Log.Warning("Final level for skill " + skillDef.defName + " is " + tempLevel);
                if (keepBaseSkill)
                {
                    if (tempLevel < skillRecord.Level)
                    {
                        tempLevel = skillRecord.Level;
                    }
                }
                else
                {
                    skillRecord.Level = tempLevel;
                }
                
                if (pawn.HasActiveGene(VREA_DefOf.VREA_NoSkillGain))
                {
                    skillRecord.passion = Passion.None;

                }
                else
                {
                    if (!keepBaseSkill)
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
                if(skillRecord.Level > skillCeiling)
                {
                    skillRecord.Level = skillCeiling;
                }
                if(skillFloor > -1 && skillRecord.Level < skillFloor)
                {
                    skillRecord.Level = skillFloor;
                }
            }
            if (pawn.IsAwakened())
            {
                if (isNeuralLocked)
                {
                    foreach(var _gene in VREAndroids.Utils.AndroidGenesGenesInOrder.Where(x => x.CanBeRemovedFromAndroid() is false).ToList())
                    {
                        pawn.genes.AddGene(_gene,false);
                    }
                    pawn.genes.AddGene(VREA_DefOf.VREA_AntiAwakeningProtocols,false);
                }
                else
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
            if (pawn.lord != null && pawn.lord.faction != Faction.OfPlayer)
            {
                if(!pawn.IsAwakened() || isNeuralLocked)
                    pawn.lord.faction = Faction.OfPlayer;
            }

            pawn.ageTracker.AgeBiologicalTicks = 0;
            pawn.ageTracker.AgeChronologicalTicks = 0;




            if (pawn.genes.GetGene(VREA_DefOf.VREA_NeutroCirculation) != null && neutroLoss)
            {
                var neutroloss = HediffMaker.MakeHediff(VREA_DefOf.VREA_NeutroLoss, pawn);
                neutroloss.Severity = 1f;
                pawn.health.AddHediff(neutroloss);

            }

           pawn.Notify_DisabledWorkTypesChanged();

        }
        private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num = 0;
            Log.Warning("Calculating final level of skill: " + sk.defName);
            foreach (BackstoryDef item in pawn.story.AllBackstories.Where((BackstoryDef bs) => bs != null))
            {
                Log.Warning("Checking backstory: " + item.defName);
                foreach (var skillGain in item.skillGains)
                {
                    if (skillGain.skill == sk)
                    {
                        Log.Warning("Found skill gain in backstory: " + item.defName + " for skill: " + sk.defName);
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
