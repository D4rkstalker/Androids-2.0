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

namespace Androids2
{
    [StaticConstructorOnStartup]

    public class Building_DroidFab : Building_PawnCrafter
    {
        /// <summary>
        /// Sustained sound.
        /// </summary>
        Sustainer soundSustainer;

        //Repeat crafting stuff.
        public bool repeatLastPawn = false;

        public override void InitiatePawnCrafting()
        {
            //Bring up Float Menu
            //FloatMenuUtility.
            List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
            foreach (AndroidRecipe def in DefDatabase<AndroidRecipe>.AllDefs.OrderBy(def => def.orderID))
            {
                bool disabled = false;
                string labelText = "";
                if (def.requiredResearch != null && !def.requiredResearch.IsFinished)
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
            Log.Warning("Base ingredient count: " + def.ingredients.Count);
            def.CalcCost();
            foreach (IngredientCount cost in def.ingredients)
            {
                Log.Warning("Adding Ingredient " + cost.filter.ToString() + " x" + cost.count);
                IngredientCount costCopy = new IngredientCount();
                costCopy.filter = cost.filter;
                costCopy.count = cost.count;

                orderProcessor.requestedItems.Add(costCopy);
            }

            craftingTime = (int)def.workAmount;
            if(def == null)
            {
                Log.Error("No recipedef for droid crafting!");
                return;
            }
            if(def.xenotype == null)
            {
                Log.Error("No xenotype for droid crafting: " + def.defName);
                return;
            }
            pawnBeingCrafted = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction);
            Log.Warning("Setting xenotype for pawn.");
            pawnBeingCrafted.genes.SetXenotype(def.xenotype);
            AndroidMakerPatch.ApplyXenotype(pawnBeingCrafted,false);
            pawnBeingCrafted.apparel?.wornApparel?.Clear();
            pawnBeingCrafted.equipment?.equipment?.Clear();
            pawnBeingCrafted.inventory?.innerContainer?.Clear();
            crafterStatus = CrafterStatus.Filling;
        }

        public override void ExtraCrafterTickAction()
        {
            if (!powerComp.PowerOn && soundSustainer != null && !soundSustainer.Ended)
                soundSustainer.End();

            //Make construction effects
            switch (crafterStatus)
            {
                case CrafterStatus.Filling:
                    //Emit smoke
                    if (powerComp.PowerOn && Current.Game.tickManager.TicksGame % 300 == 0)
                    {
                        FleckMaker.ThrowSmoke(Position.ToVector3(), Map, 1f);
                    }
                    break;

                case CrafterStatus.Crafting:
                    //Emit smoke
                    if (powerComp.PowerOn && Current.Game.tickManager.TicksGame % 100 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                            FleckMaker.ThrowMicroSparks(Position.ToVector3() + new Vector3(Rand.Range(-1, 1), 0f, Rand.Range(-1, 1)), Map);
                        for (int i = 0; i < 3; i++)
                            FleckMaker.ThrowSmoke(Position.ToVector3() + new Vector3(Rand.Range(-1f, 1f), 0f, Rand.Range(-1f, 1f)), Map, Rand.Range(0.5f, 0.75f));
                        FleckMaker.ThrowHeatGlow(Position, Map, 1f);

                        //if (soundSustainer == null || soundSustainer.Ended)
                        //{
                        //    SoundDef soundDef = printerProperties.craftingSound;
                        //    if (soundDef != null && soundDef.sustain)
                        //    {
                        //        SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
                        //        soundSustainer = soundDef.TrySpawnSustainer(info);
                        //    }
                        //}
                    }
                    if (soundSustainer != null && !soundSustainer.Ended)
                        soundSustainer.Maintain();
                    break;

                default:
                    {
                        if (soundSustainer != null && !soundSustainer.Ended)
                            soundSustainer.End();
                    }
                    break;
            }
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

            Scribe_Deep.Look(ref orderProcessor, "orderProcessor", innerContainer, inputSettings);
            Scribe_Defs.Look(ref recipe, "recipe");
            Scribe_Values.Look(ref repeatLastPawn, "repeatLastPawn");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                orderProcessor = new ThingOrderProcessor(innerContainer, inputSettings);
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
