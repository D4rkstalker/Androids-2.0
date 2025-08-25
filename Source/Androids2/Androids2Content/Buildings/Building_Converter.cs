
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using VREAndroids;

namespace Androids2
{
    public class Building_Converter : Building_PawnCrafter
    {
        public float requestedNutrition = 0f;

        public bool HasAnyContents
        {
            get
            {
                return ingredients.Count > 0;
            }
        }

        public void EjectContents()
        {
            ingredients.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near, null, null, true);
            contentsKnown = true;
            Notify_PawnEntered();
        }

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x0600003B RID: 59 RVA: 0x000039BF File Offset: 0x00001BBF
        public bool CanOpen
        {
            get
            {
                return HasAnyContents;
            }
        }

        // Token: 0x0600003C RID: 60 RVA: 0x000039C7 File Offset: 0x00001BC7
        public void Open()
        {
            if (!HasAnyContents)
            {
                return;
            }
            EjectContents();
            recipe.timeCost = 0;
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000039DF File Offset: 0x00001BDF
        public virtual bool Accepts(Thing thing)
        {
            return ingredients.CanAcceptAnyOf(thing, true);
        }

        // Token: 0x0600003E RID: 62 RVA: 0x000039F0 File Offset: 0x00001BF0
        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            Pawn pawn = thing as Pawn;
            if (pawn == null)
            {
                Log.Error(string.Concat(new string[]
                {
                    base.ThingID,
                    " accepted non pawn ",
                    pawn.ThingID,
                    "/",
                    pawn.GetType().Name,
                    "! this should never happen"
                }));
                return false;
            }
            if (ingredients.TryAdd(thing, true))
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    contentsKnown = true;
                }
                crafterStatus = CrafterStatus.Idle;
                return true;
            }
            Log.Warning("Could not add to container");
            return false;
        }

        public Thing ContainedThing
        {
            get
            {
                if (ingredients.Count != 0)
                {
                    return ingredients[0];
                }
                return null;
            }
        }
        public void Notify_PawnEntered()
        {
            base.Map.mapDrawer.MapMeshDirty(base.Position, 0);//TODO
        }

        // Token: 0x06000047 RID: 71 RVA: 0x00003BC7 File Offset: 0x00001DC7
        private void ResetProcess()
        {
            crafterStatus = CrafterStatus.Idle;
            orderProcessor.requestedItems.Clear();
        }

        public void InitiatePawnModing()
        {
            newPawn = (Pawn)currentPawn.CloneObjectShallowly();
            Find.WindowStack.Add(new Window_Convert(this,null));
        }

        // Token: 0x0600004A RID: 74 RVA: 0x00003BFF File Offset: 0x00001DFF
        public void EjectPawn()
        {
            ingredients.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near, null, null, true);


            Open();
            ResetProcess();
        }

        // Token: 0x0600004B RID: 75 RVA: 0x00003C2C File Offset: 0x00001E2C
        public void CompleteConversion()
        {
            if (newPawn != null)
            {
                //Log.Warning(currentPawn.story.headType.graphicPath);
                //Log.Warning(newPawn.story.headType.graphicPath);
                currentPawn.gender = newPawn.gender;
                //if (newPawn.def is AlienRace.ThingDef_AlienRace ndef && currentPawn.def is AlienRace.ThingDef_AlienRace cdef)
                //{
                //	cdef.alienRace.graphicPaths = ndef.alienRace.graphicPaths;

                //}
                currentPawn.def = newPawn.def;
                currentPawn.ChangeKind(newPawn.kindDef);
                currentPawn.story.headType = newPawn.story.headType;
                currentPawn.story.bodyType = newPawn.story.bodyType;
                currentPawn.style = newPawn.style;
                currentPawn.story.hairDef = newPawn.story.hairDef;
                currentPawn.story.SkinColorBase = newPawn.story.SkinColor;
                //currentPawn.health.hediffSet.hediffs = newPawn.health.hediffSet.hediffs;
                //currentPawn.skills.skills = newPawn.skills.skills;
                //currentPawn.story.traits.allTraits = newPawn.story.traits.allTraits;



                //currentPawn.ageTracker = newPawn.ageTracker;
                //long ageInTicks = 18 * (long)GenDate.TicksPerYear;

                //currentPawn.ageTracker.AgeBiologicalTicks = ageInTicks;
                //currentPawn.ageTracker.AgeChronologicalTicks = ageInTicks;


                currentPawn.Drawer.renderer.SetAllGraphicsDirty();
                //PortraitsCache.SetDirty(currentPawn);
                //PortraitsCache.PortraitsCacheUpdate();

            }

            if (!currentPawn.IsAndroid())
            {

                foreach (Hediff hediff in currentPawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.isBad)
                    {
                        currentPawn.health.RemoveHediff(hediff);
                    }
                }
            }
            Open();
            ResetProcess();
        }

        // Token: 0x0600004C RID: 76 RVA: 0x00003D70 File Offset: 0x00001F70
        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> list = new List<Gizmo>(base.GetGizmos());
            if (crafterStatus == CrafterStatus.Idle)
            {
                list.Insert(0, new Gizmo_StartConvert(this));
                list.Insert(0, new Gizmo_CancelConvert(this));
            }
            else if (crafterStatus == CrafterStatus.Filling)
            {
                list.Insert(0, new Gizmo_CancelConvert(this));
            }
            if (DebugSettings.godMode && (crafterStatus == CrafterStatus.Filling || crafterStatus == CrafterStatus.Crafting))
            {
                list.Insert(0, new Command_Action
                {
                    defaultLabel = "DEBUG: Finish modding.",
                    defaultDesc = "Finishes modding the pawn.",
                    action = delegate ()
                    {
                        crafterStatus = CrafterStatus.Finished;
                    }
                });
            }
            return list;
        }


        // Token: 0x0600004E RID: 78 RVA: 0x00003E75 File Offset: 0x00002075
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (QuestUtility.IsQuestLodger(myPawn))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(TranslatorFormattedStringExtensions.Translate("CannotUseReason", Translator.Translate("CryptosleepCasketGuestsNotAllowed")), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield return floatMenuOption;
                yield break;
            }
            foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption2;
            }
            if (ingredients.Count == 0)
            {
                if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, false, 0))
                {
                    FloatMenuOption floatMenuOption3 = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield return floatMenuOption3;
                }
                //else if (!CanEnter(myPawn))
                //{
                //    FloatMenuOption floatMenuOption4 = new FloatMenuOption(Translator.Translate("CannotBeConverted"), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                //    yield return floatMenuOption4;
                //}
                else
                {
                    JobDef jobDef = A2_Defof.A2_EnterConverter;
                    string text = Translator.Translate("EnterConversionChamber");
                    Action action = delegate ()
                    {
                        Job job = JobMaker.MakeJob(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, new JobTag?(0), false);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0), myPawn, this, "ReservedBy");
                }
            }
            yield break;
        }

        // Token: 0x0600004F RID: 79 RVA: 0x00003E8C File Offset: 0x0000208C
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = base.GetComp<CompPowerTrader>();
            flickableComp = base.GetComp<CompFlickable>();
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
            AdjustPowerNeed();
        }

        // Token: 0x06000050 RID: 80 RVA: 0x00003F44 File Offset: 0x00002144
        public override void PostMake()
        {
            base.PostMake();
            inputSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                inputSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }


        // Token: 0x06000052 RID: 82 RVA: 0x00004083 File Offset: 0x00002283
        public override void Destroy(DestroyMode mode = 0)
        {
            if (mode > 0)
            {
                ingredients.TryDropAll(base.PositionHeld, base.MapHeld, ThingPlaceMode.Near, null, null, true);
            }
            base.Destroy(mode);
        }

        // Token: 0x06000053 RID: 83 RVA: 0x000040B0 File Offset: 0x000022B0
        public override string GetInspectString()
        {
            string result;
            if (base.ParentHolder != null && !(base.ParentHolder is Map))
            {
                result = base.GetInspectString();
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
                stringBuilder.AppendLine();
                StringBuilder stringBuilder2 = stringBuilder;
                string text = "AndroidCrafterStatus";
                string str = "AndroidCrafterStatusEnum";
                stringBuilder2.AppendLine(TranslatorFormattedStringExtensions.Translate(text, Translator.Translate(str + crafterStatus.ToString())));
                if (crafterStatus == CrafterStatus.Crafting)
                {
                    stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("AndroidModdingProgress", GenText.ToStringPercent(((float)recipe.timeCost - (float)craftingTicksLeft) / (float)recipe.timeCost)));
                }
                if (crafterStatus == CrafterStatus.Filling)
                {
                    bool flag = true;
                    stringBuilder.Append(FormatIngredientCosts(out flag, orderProcessor.requestedItems, true));
                    if (!flag)
                    {
                        stringBuilder.AppendLine();
                    }
                }
                if (ingredients.Count > 0)
                {
                    stringBuilder.Append(Translator.Translate("AndroidPrinterMaterials") + " ");
                }
                foreach (Thing thing in ingredients)
                {
                    stringBuilder.Append(thing.LabelCap + "; ");
                }
                result = GenText.TrimEndNewlines(stringBuilder.ToString());
            }
            return result;
        }

        public override void FinishedTick()
        {
            ingredients.ClearAndDestroyContents(0);
            FilthMaker.TryMakeFilth(InteractionCell, base.Map, RimWorld.ThingDefOf.Filth_Slime, 5, 0);
            ChoiceLetter choiceLetter;
            if (currentPawn.IsAndroid())
            {
                choiceLetter = LetterMaker.MakeLetter(TranslatorFormattedStringExtensions.Translate("AndroidConvertLetterLabel", currentPawn.Name.ToStringShort), TranslatorFormattedStringExtensions.Translate("AndroidConvertLetterDescription", currentPawn.Name.ToStringFull), LetterDefOf.PositiveEvent, currentPawn, null, null, null);
            }
            else
            {
                choiceLetter = LetterMaker.MakeLetter(TranslatorFormattedStringExtensions.Translate("AndroidModLetterLabel", currentPawn.Name.ToStringShort), TranslatorFormattedStringExtensions.Translate("AndroidModLetterDescription", currentPawn.Name.ToStringFull), LetterDefOf.PositiveEvent, currentPawn, null, null, null);
            }
            Find.LetterStack.ReceiveLetter(choiceLetter, null);
            CompleteConversion();
        }

        // Token: 0x06000059 RID: 89 RVA: 0x000048BC File Offset: 0x00002ABC
        public string FormatIngredientCosts(out bool needsFulfilled, IEnumerable<ThingOrderRequest> requestedItems, bool deductCosts = true)
        {
            StringBuilder stringBuilder = new StringBuilder();
            needsFulfilled = true;
            foreach (ThingOrderRequest thingOrderRequest in requestedItems)
            {
                if (thingOrderRequest.nutrition)
                {
                    float num = CountNutrition();
                    if (deductCosts)
                    {
                        float num2 = thingOrderRequest.amount - num;
                        if (num2 > 0f)
                        {
                            stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("AndroidPrinterNeed", num2, Translator.Translate("AndroidNutrition")) + " ");
                            needsFulfilled = false;
                        }
                    }
                    else
                    {
                        stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("AndroidPrinterNeed", thingOrderRequest.amount, Translator.Translate("AndroidNutrition")) + " ");
                    }
                }
                else
                {
                    int num3 = ingredients.TotalStackCountOfDef(thingOrderRequest.thingDef);
                    if (deductCosts)
                    {
                        if ((float)num3 < thingOrderRequest.amount)
                        {
                            stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("AndroidPrinterNeed", thingOrderRequest.amount - (float)num3, thingOrderRequest.thingDef.LabelCap) + " ");
                            needsFulfilled = false;
                        }
                    }
                    else
                    {
                        stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("AndroidPrinterNeed", thingOrderRequest.amount, thingOrderRequest.thingDef.LabelCap) + " ");
                    }
                }
            }
            return stringBuilder.ToString();
        }

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x0600005A RID: 90 RVA: 0x00004A68 File Offset: 0x00002C68
        public Pawn currentPawn
        {
            get
            {
                return ingredients.First<Thing>() as Pawn;
            }
        }
        // Token: 0x04000042 RID: 66
        protected bool contentsKnown;


        // Token: 0x04000051 RID: 81
        private Graphic cachedGraphicFull;

        public Pawn newPawn;
    }

}
