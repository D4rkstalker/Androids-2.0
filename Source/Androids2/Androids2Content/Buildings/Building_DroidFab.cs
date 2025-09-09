using Androids2.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;
using VREAndroids;
using static UnityEngine.TouchScreenKeyboard;

namespace Androids2
{
    [StaticConstructorOnStartup]

    public class Building_DroidFab : Building_PawnCrafter
    {

        //Repeat crafting stuff.

        public override void InitiatePawnCrafting()
        {
            //Bring up Float Menu
            //FloatMenuUtility.
            List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
            foreach (AndroidRecipe def in DefDatabase<AndroidRecipe>.AllDefs.OrderBy(def => def.orderID))
            {
                if(def.hidden)
                {
                    continue;
                }
                bool disabled = false;
                string labelText = "";
                if (def.requiredResearch != null && !def.requiredResearch.IsFinished )
                {
                    disabled = true;
                }

                if (disabled)
                {
                    labelText = "AndroidDroidCrafterPawnNeedResearch".Translate(def.label, def.requiredResearch.LabelCap);
                }
                else
                {
                    labelText = "AndroidDroidCrafterPawnMake".Translate(def.label);
                }

                FloatMenuOption option = new FloatMenuOption(labelText,
                delegate ()
                {
                    //Stuff
                    if (!disabled)
                    {
                        recipe = def;
                        MakePawnAndInitCrafting(def);
                    }
                }
                );

                option.Disabled = disabled;
                floatMenuOptions.Add(option);
            }

            if (floatMenuOptions.Count > 0)
            {
                FloatMenu floatMenu = new FloatMenu(floatMenuOptions);
                Find.WindowStack.Add(floatMenu);
            }

            /*pawnBeingCrafted = 
                DroidUtility.MakeDroidTemplate(
                    printerProperties.pawnKind.race, 
                    printerProperties.pawnKind, Faction, 
                    Map, 
                    printerProperties.skills, 
                    printerProperties.defaultSkillLevel);

            crafterStatus = CrafterStatus.Filling;*/
        }

        public void MakePawnAndInitCrafting(AndroidRecipe def)
        {
            //Update costs.
            orderProcessor.requestedItems.Clear();
            Log.Warning("Base ingredient count: " + def.costList.Count);
            def.CalcCost();
            foreach (ThingOrderRequest cost in def.costList)
            {
                ThingOrderRequest costCopy = new ThingOrderRequest();
                costCopy.nutrition = cost.nutrition;
                costCopy.thingDef = cost.thingDef;
                costCopy.amount = cost.amount;

                orderProcessor.requestedItems.Add(costCopy);
            }

            if(def == null)
            {
                Log.Error("No recipedef for droid crafting!");
                return;
            }
            if(def.customXenotype == null)
            {
                Log.Error("No xenotype for droid crafting: " + def.defName);
                return;
            }
            pawnBeingCrafted = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction);
            Log.Warning("Setting xenotype for pawn.");
            pawnBeingCrafted.genes.xenotypeName = def.customXenotype.name;
            pawnBeingCrafted.genes.iconDef = def.customXenotype.IconDef;
            foreach (var gene in VREAndroids.Utils.allAndroidGenes)
            {
                var existingGene = pawnBeingCrafted.genes.GetGene(gene);
                if (existingGene != null)
                {
                    pawnBeingCrafted.genes.RemoveGene(existingGene);
                }
            }
            for (int i = pawnBeingCrafted.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = pawnBeingCrafted.health.hediffSet.hediffs[i];
                pawnBeingCrafted.health.hediffSet.hediffs.RemoveAt(i);
                
            }

            foreach (GeneDef gene in recipe.customXenotype.genes.OrderByDescending(x => x.CanBeRemovedFromAndroid() is false).ToList())
            {
                pawnBeingCrafted.genes.AddGene(gene, true);
            }
            AndroidMakerPatch.ApplyXenotype(pawnBeingCrafted, def.customXenotype.genes,false);
            pawnBeingCrafted.apparel?.wornApparel?.Clear();
            pawnBeingCrafted.equipment?.equipment?.Clear();
            pawnBeingCrafted.inventory?.innerContainer?.Clear();
            pawnBeingCrafted.story.traits.allTraits.Clear();
            pawnBeingCrafted.story.adulthood = null;
            if(recipe.backstory != null)
                pawnBeingCrafted.story.childhood = recipe.backstory;
            crafterStatus = CrafterStatus.Filling;
            pawnBeingCrafted.Drawer.renderer.EnsureGraphicsInitialized();

        }


        public override void FinishAction()
        {
            orderProcessor.requestedItems.Clear();

            if (repeatLastPawn && recipe != null)
            {
                MakePawnAndInitCrafting(recipe);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref recipe, "recipe");
            Scribe_Values.Look(ref repeatLastPawn, "repeatLastPawn");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                orderProcessor = new ThingOrderProcessor(ingredients, inputSettings);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Command_Toggle()
            {
                defaultLabel = "AndroidGizmoRepeatPawnCraftingLabel".Translate(),
                defaultDesc = "AndroidGizmoRepeatPawnCraftingDescription".Translate(),
                icon = ContentFinder<Texture2D>.Get("ui/designators/PlanOn", true),
                isActive = () => repeatLastPawn,
                toggleAction = delegate ()
                {
                    repeatLastPawn = !repeatLastPawn;
                }
            };
        }

    }
}
