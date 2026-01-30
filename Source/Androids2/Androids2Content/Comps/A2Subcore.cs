using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEF;
using Verse;

namespace Androids2
{
    [HotSwappable]
    [StaticConstructorOnStartup]

    public class A2Subcore : ThingWithComps
    {
        List<SkillRecord> srs = new List<SkillRecord>();
        public void InitializeWithPawns(List<Pawn> pawns)
        {
            Dictionary<SkillDef,Passion> possiblePassions = new Dictionary<SkillDef, Passion>();
            foreach (var pawn in pawns)
            {
                Passion _p = Passion.None;
                pawn.skills.skills.ForEach(skill =>
                {
                    if (skill.passion > _p)
                    {
                        _p = skill.passion;
                    }
                });
                if(_p == Passion.None)
                {
                    Log.Warning("Pawn " + pawn.Name.ToStringFull + " has no passions. Skipping.");
                    continue;
                }
                pawn.skills.skills.ForEach(skill =>
                {
                    if (skill.passion >= _p)
                    {
                        if(possiblePassions.ContainsKey(skill.def))
                        {
                            if(skill.passion > possiblePassions[skill.def])
                            {
                                possiblePassions[skill.def] = skill.passion;
                            }
                        }
                        else
                        {
                            possiblePassions.Add(skill.def, _p);
                        }
                    }
                });
                Log.Warning("Pawn " + pawn.Name.ToStringFull + " contributed passions. Current possible passions count: " + possiblePassions.Count);
            }
            for(int i = 0; i< pawns.Count; i++)
            {
                if (possiblePassions.Count > 0)
                {
                    var chosenSkill = possiblePassions.RandomElement();
                    SkillRecord sr = new SkillRecord();
                    sr.def = chosenSkill.key;
                    sr.passion = chosenSkill.value;
                    srs.Add(sr);
                    possiblePassions.Remove(chosenSkill.key);
                    Log.Warning("Added skill " + sr.def.defName + " with passion " + sr.passion.ToString() + " to subcore.");
                }
                else 
                {
                    Log.Warning("No possible passions to add!");
                    break;
                }
            }

        }
    }
}
