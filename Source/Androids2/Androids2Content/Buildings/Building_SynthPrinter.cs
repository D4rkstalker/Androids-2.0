using Androids2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VEF.Genes;
using Verse;
using Verse.Sound;
using VREAndroids;

namespace Androids2
{
    public class Building_SynthPrinter : Building_PawnCrafter
    {

        //Static values
        /// <summary>
        /// Requested nutrition to print one Android.
        /// </summary>
        public float requestedNutrition = 0f;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            flickableComp = GetComp<CompFlickable>();
            if (inputSettings == null)
            {
                inputSettings = new StorageSettings(this);
                if (def.building.defaultStorageSettings != null)
                {
                    inputSettings.CopyFrom(def.building.defaultStorageSettings);
                }
            }

            if (!respawningAfterLoad)
            {
                orderProcessor = new ThingOrderProcessor(ingredients, inputSettings);

            }
            useSubpersonaCore = true;
            AdjustPowerNeed();
        }

        public override void PostMake()
        {
            base.PostMake();

            inputSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                inputSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }

        public override void DeSpawn(DestroyMode mode)
        {

            StopPawnCrafting();

            base.DeSpawn(mode);
        }


        public string FormatIngredientCosts(out bool needsFulfilled, IEnumerable<ThingOrderRequest> requestedItems, bool deductCosts = true)
        {
            StringBuilder builder = new StringBuilder();
            needsFulfilled = true;

            foreach (ThingOrderRequest thingOrderRequest in requestedItems)
            {
                if (thingOrderRequest.nutrition)
                {
                    float totalNutrition = CountNutrition();

                    if (deductCosts)
                    {
                        float nutritionDifference = thingOrderRequest.amount - totalNutrition;
                        if (nutritionDifference > 0f)
                        {
                            builder.Append(crafterMaterialNeedText.Translate((nutritionDifference), crafterNutritionText.Translate()) + " ");
                            needsFulfilled = false;
                        }
                    }
                    else
                    {
                        builder.Append(crafterMaterialNeedText.Translate((thingOrderRequest.amount), crafterNutritionText.Translate()) + " ");
                    }
                }
                else
                {
                    int itemCount = ingredients.TotalStackCountOfDef(thingOrderRequest.thingDef);
                    if (deductCosts)
                    {
                        if (itemCount < thingOrderRequest.amount)
                        {
                            builder.Append(crafterMaterialNeedText.Translate((thingOrderRequest.amount - itemCount), thingOrderRequest.thingDef.LabelCap) + " ");
                            needsFulfilled = false;
                        }
                    }
                    else
                    {
                        builder.Append(crafterMaterialNeedText.Translate((thingOrderRequest.amount), thingOrderRequest.thingDef.LabelCap) + " ");
                    }
                }
            }

            return builder.ToString();
        }

        public override void InitiatePawnCrafting()
        {
            //Open Android Customization window.
            Find.WindowStack.Add(new Window_CreateSynth(this, null));
        }


    }
}
