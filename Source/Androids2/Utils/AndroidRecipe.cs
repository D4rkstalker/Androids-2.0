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
    public class AndroidRecipe : Def
    {
        public List<ThingOrderRequest> costList = new List<ThingOrderRequest>();
        public CustomXenotype customXenotype;
        public XenotypeDef xenotypeDef;
        public int resourceTick = 2500;
        public ResearchProjectDef requiredResearch;
        public int orderID = 0;
        public int nutrition = 0;
        public int timeCost = 0;
        public int progress = 0;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            customXenotype = new CustomXenotype();
            foreach (GeneDef gene in xenotypeDef.genes)
            {
                customXenotype.genes.Add(gene);
            }
            customXenotype.name = xenotypeDef.label;
        }

        public void CalcCost()
        {
            if(customXenotype == null)
            {
                Log.Error("No xenotype for android recipe: " + defName);
                return;
            }
            foreach (GeneDef gene in customXenotype.genes)
            {
                if (gene is A2GeneDef a2Gene)
                {
                    if(a2Gene.costList.Count > 0)
                    {
                        costList.AddRange(a2Gene.costList);
                        timeCost += a2Gene.timeCost;
                    }
                }
            }
        }

        public float ResourceTicks()
        {
            return (float)Math.Ceiling((double)timeCost / resourceTick);
        }

    }
}