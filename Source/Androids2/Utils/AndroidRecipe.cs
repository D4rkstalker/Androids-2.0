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
        public bool hidden = false;
        public BackstoryDef backstory;
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
            costList.Clear();
            if (customXenotype == null)
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
                        // Create copies instead of sharing references
                        foreach (ThingOrderRequest item in a2Gene.costList)
                        {
                            costList.Add(new ThingOrderRequest(item));
                        }
                        timeCost += a2Gene.timeCost;
                    }
                }
            }
        }

        public float ResourceTicks()
        {
            return (float)Math.Ceiling((double)timeCost / resourceTick);
        }

        public AndroidRecipe Clone()
        {
            AndroidRecipe clone = new AndroidRecipe
            {
                defName = this.defName,
                label = this.label,
                description = this.description,
                costList = this.costList.ToList(),
                xenotypeDef = this.xenotypeDef,
                resourceTick = this.resourceTick,
                requiredResearch = this.requiredResearch,
                orderID = this.orderID,
                nutrition = this.nutrition,
                timeCost = this.timeCost,
                progress = this.progress,
                hidden = this.hidden,
                backstory = this.backstory
            };

            // Deep copy customXenotype if it exists
            if (this.customXenotype != null)
            {
                clone.customXenotype = new CustomXenotype
                {
                    name = this.customXenotype.name,
                    inheritable = this.customXenotype.inheritable,
                    iconDef = this.customXenotype.iconDef
                };
                
                // Copy genes list
                foreach (GeneDef gene in this.customXenotype.genes)
                {
                    clone.customXenotype.genes.Add(gene);
                }
            }

            return clone;
        }

    }
}