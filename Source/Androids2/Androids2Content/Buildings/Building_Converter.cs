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

namespace Androids2
{
    public class Building_ConversionChamber : Building_PawnCrafter
    {

        public Building_ConversionChamber()
        {
            innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            ingredients = new ThingOwner<Thing>();
            _power = GetComp<CompPowerTrader>();
        }

        public bool HasAnyContents
        {
            get
            {
                return innerContainer.Count > 0;
            }
        }

        public void EjectContents()
        {
            innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near, null, null, true);
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
            tickToMod = 0;
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000039DF File Offset: 0x00001BDF
        public virtual bool Accepts(Thing thing)
        {
            return innerContainer.CanAcceptAnyOf(thing, true);
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
            if (innerContainer.TryAdd(thing, true))
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
                if (innerContainer.Count != 0)
                {
                    return innerContainer[0];
                }
                return null;
            }
        }
        public void Notify_PawnEntered()
        {
            base.Map.mapDrawer.MapMeshDirty(base.Position, 0);//TODO
        }

        private CompPowerTrader PowerCompTrader
        {
            get
            {
                return _power;
            }
        }

        // Token: 0x06000047 RID: 71 RVA: 0x00003BC7 File Offset: 0x00001DC7
        private void ResetProcess()
        {
            crafterStatus = CrafterStatus.WaitingForPawn;
            orderProcessor.requestedItems.Clear();
        }

        // Token: 0x06000048 RID: 72 RVA: 0x00003BE0 File Offset: 0x00001DE0
        public bool IsPawnAndroid()
        {
            return RaceUtility.IsAndroid(currentPawn);
        }

        // Token: 0x06000049 RID: 73 RVA: 0x00003BED File Offset: 0x00001DED
        public void InitiatePawnModing()
        {
            newPawn = (Pawn)currentPawn.CloneObjectShallowly();
            Find.WindowStack.Add(new AndroidConversionWindow(this));
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

            if (!IsPawnAndroid())
            {

                AndroidUtility.Androidify(currentPawn);
                //LifeStageAge lifeStageAge = currentPawn.RaceProps.lifeStageAges[currentPawn.ageTracker.CurLifeStageIndex];
                //if (lifeStageAge != null)
                //{
                //	long num = (long)Math.Ceiling((double)lifeStageAge.minAge) * 3600000L;
                //	currentPawn.ageTracker.AgeBiologicalTicks = num;
                //	currentPawn.ageTracker.AgeChronologicalTicks = num;
                //}
                //else
                //{
                //	long num2 = (long)(currentPawn.RaceProps.lifeExpectancy * 3600000f * 0.2f);
                //	currentPawn.ageTracker.AgeBiologicalTicks = num2;
                //	currentPawn.ageTracker.AgeChronologicalTicks = num2;
                //}
            }
            //foreach (ModCommand modCommand in savedChanges)
            //{
            //    if (modCommand.isActive)
            //    {
            //        if (modCommand.removing)
            //        {
            //            modCommand.Remove(currentPawn);
            //        }
            //    }
            //    else
            //    {
            //        modCommand.Apply(currentPawn);
            //    }
            //}
            if (!IsPawnAndroid())
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
                list.Insert(0, new Gizmo_StartMod(this));
                list.Insert(0, new Gizmo_AbortMod(this));
            }
            else if (crafterStatus == CrafterStatus.Filling)
            {
                list.Insert(0, new Gizmo_AbortMod(this));
            }
            if (DebugSettings.godMode && (crafterStatus == CrafterStatus.Filling || crafterStatus == CrafterStatus.Modding))
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

        // Token: 0x0600004D RID: 77 RVA: 0x00003E14 File Offset: 0x00002014
        public bool CanEnter(Pawn testPawn)
        {
            string pawnRaceName = testPawn.kindDef.race.defName;
            return (!(pawnRaceName != "Human") || RaceUtility.AlienRaceKinds.Any((PawnKindDef kind) => kind.race.defName == pawnRaceName)) && Accepts(testPawn);
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
            if (innerContainer.Count == 0)
            {
                if (!ReachabilityUtility.CanReach(myPawn, this, PathEndMode.InteractionCell, Danger.Deadly, false, false, 0))
                {
                    FloatMenuOption floatMenuOption3 = new FloatMenuOption(Translator.Translate("CannotUseNoPath"), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield return floatMenuOption3;
                }
                else if (!CanEnter(myPawn))
                {
                    FloatMenuOption floatMenuOption4 = new FloatMenuOption(Translator.Translate("CannotBeConverted"), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                    yield return floatMenuOption4;
                }
                else
                {
                    JobDef jobDef = JobDefOf.DekEnterConversionChamber;
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

        // Token: 0x06000051 RID: 81 RVA: 0x00003F94 File Offset: 0x00002194
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner<Thing>>(ref ingredients, "ingredients", Array.Empty<object>());
            Scribe_Values.Look<CrafterStatus>(ref crafterStatus, "printerStatus", CrafterStatus.Idle, false);
            Scribe_Values.Look<int>(ref remainingTickTracker, "printingTicksLeft", 0, false);
            Scribe_Values.Look<int>(ref nextResourceTick, "nextResourceTick", 0, false);
            Scribe_Deep.Look<StorageSettings>(ref inputSettings, "inputSettings", Array.Empty<object>());
            Scribe_Deep.Look<ThingOrderProcessor>(ref orderProcessor, "orderProcessor", new object[]
            {
                ingredients,
                inputSettings
            });
            Scribe_Values.Look<int>(ref tickToMod, "totaltimeCost", 0, false);
            Scribe_Deep.Look<ThingOwner>(ref innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look<bool>(ref contentsKnown, "contentsKnown", false, false);
            Scribe_Collections.Look<ModCommand>(ref savedChanges, "savedChanges", LookMode.Deep, Array.Empty<object>());
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
                if (crafterStatus == CrafterStatus.Modding)
                {
                    stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("AndroidModdingProgress", GenText.ToStringPercent(((float)tickToMod - (float)remainingTickTracker) / (float)tickToMod)));
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

        // Token: 0x06000054 RID: 84 RVA: 0x00004234 File Offset: 0x00002434
        public override void Tick()
        {
            base.Tick();

            AdjustPowerNeed();
            if (!powerComp.PowerOn && soundSustainer != null && !soundSustainer.Ended)
            {
                soundSustainer.End();
            }
            if (flickableComp == null || (flickableComp != null && flickableComp.SwitchIsOn))
            {
                switch (crafterStatus)
                {
                    case CrafterStatus.Filling:
                        handleFillingTick();
                        return;
                    case CrafterStatus.Crafting:
                        handleModdingTick();
                        return;
                    case CrafterStatus.Finished:
                        handleFinalTick();
                        return;
                    default:
                        if (soundSustainer != null && !soundSustainer.Ended)
                        {
                            soundSustainer.End();
                        }
                        break;
                }
            }
        }

        // Token: 0x06000055 RID: 85 RVA: 0x000042EC File Offset: 0x000024EC
        public void handleFillingTick()
        {
            if (powerComp.PowerOn && Current.Game.tickManager.TicksGame % 300 == 0)
            {
                FleckMaker.ThrowSmoke(base.Position.ToVector3(), base.Map, 1f);
            }
            IEnumerable<ThingOrderRequest> enumerable = orderProcessor.PendingRequests();
            bool flag = enumerable == null;
            if (!flag && enumerable.Count<ThingOrderRequest>() == 0)
            {
                flag = true;
            }
            if (flag)
            {
                crafterStatus = CrafterStatus.Modding;
            }
        }

        // Token: 0x06000056 RID: 86 RVA: 0x00004364 File Offset: 0x00002564
        public void handleModdingTick()
        {
            if (powerComp.PowerOn)
            {
                if (Current.Game.tickManager.TicksGame % 100 == 0)
                {
                    FleckMaker.ThrowSmoke(base.Position.ToVector3(), base.Map, 1.33f);
                }
                if (Current.Game.tickManager.TicksGame % 250 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        FleckMaker.ThrowMicroSparks(base.Position.ToVector3() + new Vector3((float)Rand.Range(-1, 1), 0f, (float)Rand.Range(-1, 1)), base.Map);
                    }
                }
                if (soundSustainer == null || soundSustainer.Ended)
                {
                    SoundDef craftingSound = conversionProperties.craftingSound;
                    if (craftingSound != null && craftingSound.sustain)
                    {
                        SoundInfo soundInfo = SoundInfo.InMap(this, MaintenanceType.PerTick);
                        soundSustainer = SoundStarter.TrySpawnSustainer(craftingSound, soundInfo);
                    }
                }
                if (soundSustainer != null && !soundSustainer.Ended)
                {
                    soundSustainer.Maintain();
                }
                nextResourceTick--;
                if (nextResourceTick <= 0)
                {
                    nextResourceTick = conversionProperties.resourceTick;
                    using (List<ThingOrderRequest>.Enumerator enumerator = orderProcessor.requestedItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            ThingOrderRequest thingOrderRequest = enumerator.Current;
                            if (thingOrderRequest.nutrition)
                            {
                                if (CountNutrition() > 0f)
                                {
                                    Thing thing4 = ingredients.First((Thing thing) => thing.def.IsIngestible);
                                    if (thing4 != null)
                                    {
                                        int num = Math.Min((int)Math.Ceiling((double)thingOrderRequest.amount / ((double)tickToMod / (double)conversionProperties.resourceTick)), thing4.stackCount);
                                        Thing thing2 = null;
                                        Corpse corpse = thing4 as Corpse;
                                        if (corpse != null)
                                        {
                                            if (RottableUtility.IsDessicated(corpse))
                                            {
                                                ingredients.TryDrop(corpse, InteractionCell, base.Map, ThingPlaceMode.Near, 1, out thing2, null, null);
                                            }
                                            else
                                            {
                                                ingredients.TryDrop(corpse, InteractionCell, base.Map, ThingPlaceMode.Near, 1, out thing2, null, null);
                                                Pawn innerPawn = corpse.InnerPawn;
                                                if (innerPawn != null)
                                                {
                                                    Pawn_EquipmentTracker equipment = innerPawn.equipment;
                                                    if (equipment != null)
                                                    {
                                                        equipment.DropAllEquipment(InteractionCell, false);
                                                    }
                                                    Pawn_ApparelTracker apparel = innerPawn.apparel;
                                                    if (apparel != null)
                                                    {
                                                        apparel.DropAll(InteractionCell, false, true);
                                                    }
                                                }
                                                thing4.Destroy(0);
                                            }
                                        }
                                        else
                                        {
                                            ingredients.Take(thing4, num).Destroy(0);
                                        }
                                    }
                                }
                            }
                            else if (ingredients.Any((Thing thing) => thing.def == thingOrderRequest.thingDef))
                            {
                                Thing thing3 = ingredients.First((Thing thing) => thing.def == thingOrderRequest.thingDef);
                                if (thing3 != null)
                                {
                                    int num2 = Math.Min((int)Math.Ceiling((double)(thingOrderRequest.amount / ((float)tickToMod / (float)conversionProperties.resourceTick))), thing3.stackCount);
                                    ingredients.Take(thing3, num2).Destroy(0);
                                }
                            }
                        }
                    }
                }
                if (remainingTickTracker > 0)
                {
                    remainingTickTracker--;
                    return;
                }
                crafterStatus = CrafterStatus.Finished;
            }
        }

        // Token: 0x06000057 RID: 87 RVA: 0x00004700 File Offset: 0x00002900
        public void handleFinalTick()
        {
            ingredients.ClearAndDestroyContents(0);
            FilthMaker.TryMakeFilth(InteractionCell, base.Map, RimWorld.ThingDefOf.Filth_Slime, 5, 0);
            ChoiceLetter choiceLetter;
            if (IsPawnAndroid())
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
                return innerContainer.First<Thing>() as Pawn;
            }
        }
        // Token: 0x04000042 RID: 66
        protected bool contentsKnown;

        // Token: 0x04000043 RID: 67
        private CompPowerTrader _power;

        // Token: 0x04000049 RID: 73
        protected ThingOwner innerContainer;

        // Token: 0x0400004C RID: 76
        public int tickToMod;

        // Token: 0x0400004D RID: 77
        public int remainingTickTracker;

        // Token: 0x04000051 RID: 81
        private Graphic cachedGraphicFull;

        public Pawn newPawn;
    }

}
