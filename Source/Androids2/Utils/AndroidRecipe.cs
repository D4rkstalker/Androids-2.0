using Androids2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Androids2
{
    public class AndroidRecipe : RecipeDef
    {
        public XenotypeDef xenotype;
        public int resourceTick = 2500;
        public ResearchProjectDef requiredResearch;
        public int orderID = 0;
        public int nutrition = 0;

        public void CalcCost()
        {
            if(xenotype == null)
            {
                Log.Error("No xenotype for android recipe: " + defName);
                return;
            }
            foreach (GeneDef gene in xenotype.genes)
            {
                if (gene is A2GeneDef a2Gene)
                {
                    if(a2Gene.ingredients.Count > 0)
                    {
                        ingredients.AddRange(a2Gene.ingredients);
                        workAmount += a2Gene.extraTime;
                    }
                }
            }
        }

        public float ResourceTicks()
        {
            return (float)Math.Ceiling((double)workAmount / resourceTick);
        }

    }
}