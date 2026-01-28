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
    [StaticConstructorOnStartup]
    public class CompRepairGantry : ThingComp, ISuspendableThingHolder, IThingHolder, IThingHolderWithDrawnPawn, IStoreSettingsParent, INotifyHauledTo, ISearchableContents
    {
        public const int NoPowerEjectCumulativeTicks = 60000;

        public const int SteelRequired = 5;

        public const float CacheForSecs = 2f;

        public static readonly Texture2D InterruptCycleIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        public static readonly Material BackgroundMat = SolidColorMaterials.NewSolidColorMaterial(new Color(0.082f, 0.078f, 0.063f), ShaderDatabase.SolidColorBehind);

        public const float BackgroundRect_YOff = 0.07317074f;

        public const float Pawn_YOff = 0.03658537f;

        public string currentCycleKey;

        public float currentCycleTicksRemaining;

        public int currentCyclePowerCutTicks;

        public ThingOwner innerContainer;

        public StorageSettings allowedMaterialsSettings;

        public int storedMaterials;

        public bool autoLoadMaterials = true;

        public bool devFillPodLatch;

        public int tickEntered = -99999;

        public Job queuedEnterJob;

        public Pawn queuedPawn;

        public List<ThingCount> chosenExtraItems = new List<ThingCount>();

        public List<FloatMenuOption> cycleEligiblePawnOptions = new List<FloatMenuOption>();
        public Pawn pawnEnteringGantry;

        public Dictionary<CompRepairGantry_Cycle, List<IngredientCount>> cachedExtraIngredients = new Dictionary<CompRepairGantry_Cycle, List<IngredientCount>>();

        public Dictionary<CompRepairGantry_Cycle, CacheAnyPawnEligibleCycle> cachedAnyPawnEligible = new Dictionary<CompRepairGantry_Cycle, CacheAnyPawnEligibleCycle>();

        public Pawn cacheReachIngredientsPawn;

        public CompRepairGantry_Cycle cacheReachIngredientsCycle;

        public float cacheReachIngredientsTime = float.MinValue;

        public bool cacheReachIngredientsResult;

        public Effecter progressBarEffecter;

        public Effecter operatingEffecter;

        public Effecter readyEffecter;

        public Texture2D cachedAutoAgeReverseIcon;

        public List<CompRepairGantry_Cycle> cachedAvailableCycles;

        public Dictionary<string, CompRepairGantry_Cycle> cycleLookup;

        public static string cachedAgeReversalCycleKey = null;

        public List<string> tmpIngredientsStrings = new List<string>();

        public static readonly List<Thing> tmpItems = new List<Thing>();

        public CompPowerTrader powerTraderComp;

        public CompPower powerComp;

        public static List<ThingDef> cachedPodDefs;

        public CompProperties_RepairGantry Props => props as CompProperties_RepairGantry;
        public ThingOwner SearchableContents => innerContainer;

        public bool IsContentsSuspended => true;

        public float RequiredMaterialsRemaining => Mathf.Max(5f - storedMaterials, 0f);

        public bool NutritionLoaded => RequiredMaterialsRemaining <= 0f;

        public RepairGantryState State
        {
            get
            {
                if (Occupant != null)
                {
                    return RepairGantryState.Occupied;
                }

                if (NutritionLoaded)
                {
                    return RepairGantryState.SelectingCycle;
                }

                return RepairGantryState.LoadingMaterials;
            }
        }

        public Pawn Occupant
        {
            get
            {
                if (pawnEnteringGantry != null)
                {
                    return pawnEnteringGantry;
                }

                if (currentCycleKey == null)
                {
                    return null;
                }

                if (innerContainer.Count != 1)
                {
                    return null;
                }

                return innerContainer[0] as Pawn;
            }
        }

        public CompRepairGantry_Cycle CurrentCycle
        {
            get
            {
                if (currentCycleKey == null)
                {
                    return null;
                }

                foreach (CompRepairGantry_Cycle availableCycle in AvailableCycles)
                {
                    if (availableCycle.Props.key == currentCycleKey)
                    {
                        return availableCycle;
                    }
                }

                return null;
            }
        }

        public List<CompRepairGantry_Cycle> AvailableCycles
        {
            get
            {
                if (cachedAvailableCycles == null)
                {
                    SetupCycleCaches();
                }

                return cachedAvailableCycles;
            }
        }

        public string AgeReversalCycleKey
        {
            get
            {
                if (cachedAgeReversalCycleKey == null)
                {
                    SetupCycleCaches();
                }

                return cachedAgeReversalCycleKey;
            }
        }

        public float CycleSpeedFactorNoPawn => CleanlinessSpeedFactor;

        public float CycleSpeedFactor
        {
            get
            {
                if (Occupant == null)
                {
                    return Mathf.Max(0.1f, CycleSpeedFactorNoPawn);
                }

                return GetCycleSpeedFactorForPawn(Occupant);
            }
        }

        public float CleanlinessSpeedFactor => parent.GetStatValue(StatDefOf.BiosculpterPodSpeedFactor);


        public bool PowerOn => parent.TryGetComp<CompPowerTrader>().PowerOn;

        public float HeldPawnDrawPos_Y => parent.DrawPos.y - 0.03658537f;

        public float HeldPawnBodyAngle => parent.Rotation.Opposite.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

        public bool StorageTabVisible => true;

        public CompRepairGantry()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            allowedMaterialsSettings = new StorageSettings(this);
            if (parent.def.building.defaultStorageSettings != null)
            {
                allowedMaterialsSettings.CopyFrom(parent.def.building.defaultStorageSettings);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (ModLister.CheckIdeology("Biosculpter pod comp"))
            {
                base.PostSpawnSetup(respawningAfterLoad);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref currentCycleKey, "currentCycleKey");
            Scribe_Values.Look(ref currentCycleTicksRemaining, "currentCycleTicksRemaining", 0f);
            Scribe_Values.Look(ref currentCyclePowerCutTicks, "currentCyclePowerCutTicks", 0);
            Scribe_Deep.Look(ref allowedMaterialsSettings, "allowedMaterialsSettings");
            Scribe_Values.Look(ref storedMaterials, "storedMaterials", 0);
            Scribe_Values.Look(ref autoLoadMaterials, "autoLoadMaterials", defaultValue: false);
            Scribe_Values.Look(ref devFillPodLatch, "devFillPodLatch", defaultValue: false);
            Scribe_Values.Look(ref tickEntered, "tickEntered", 0);
            Scribe_References.Look(ref queuedEnterJob, "queuedEnterJob");
            Scribe_References.Look(ref queuedPawn, "queuedPawn");
            if (allowedMaterialsSettings == null)
            {
                allowedMaterialsSettings = new StorageSettings(this);
                if (parent.def.building.defaultStorageSettings != null)
                {
                    allowedMaterialsSettings.CopyFrom(parent.def.building.defaultStorageSettings);
                }
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (currentCycleKey == "healing")
                {
                    currentCycleKey = "medic";
                }

                StoreMaterials();
            }
        }

        public CompRepairGantry_Cycle GetCycle(string key)
        {
            if (cycleLookup == null)
            {
                SetupCycleCaches();
            }

            return cycleLookup[key];
        }

        public float GetCycleSpeedFactorForPawn(Pawn p)
        {
            return Mathf.Max(0.1f, CycleSpeedFactorNoPawn * p.GetStatValue(StatDefOf.BiosculpterOccupantSpeed));
        }

        public void SetupCycleCaches()
        {
            cachedAvailableCycles = new List<CompRepairGantry_Cycle>();
            cachedAvailableCycles.AddRange(parent.AllComps.OfType<CompRepairGantry_Cycle>());
            cycleLookup = new Dictionary<string, CompRepairGantry_Cycle>();
            foreach (CompRepairGantry_Cycle cachedAvailableCycle in cachedAvailableCycles)
            {
                cycleLookup[cachedAvailableCycle.Props.key] = cachedAvailableCycle;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize)
            {
                EjectContents(interrupted: true, playSounds: false, previousMap);
            }

            innerContainer.ClearAndDestroyContents();
            base.PostDestroy(mode, previousMap);
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode != DestroyMode.WillReplace)
            {
                EjectContents(interrupted: true, playSounds: false, map);
                currentCycleKey = null;
            }

            progressBarEffecter?.Cleanup();
            progressBarEffecter = null;
            operatingEffecter?.Cleanup();
            operatingEffecter = null;
            readyEffecter?.Cleanup();
            readyEffecter = null;
        }

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (!Find.ScreenshotModeHandler.Active && Occupant != null)
            {
                GenMapUI.DrawThingLabel(parent, Occupant.LabelShort, GenMapUI.DefaultThingLabelColor);
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            RepairGantryState state = State;
            if (parent.Spawned)
            {
                CompRepairGantry_Cycle currentCycle = CurrentCycle;
                if (currentCycle != null)
                {
                    stringBuilder.AppendLineIfNotEmpty().Append("RepairGantryCycleLabel".Translate()).Append(": ")
                        .Append(currentCycle.Props.LabelCap);
                }
                else if (state == RepairGantryState.SelectingCycle)
                {
                    if (PowerOn)
                    {
                        if (queuedEnterJob != null && !queuedEnterJob.biosculpterCycleKey.NullOrEmpty())
                        {
                            stringBuilder.Append("RepairGantryCycleStandby".Translate(GetCycle(queuedEnterJob.biosculpterCycleKey).Props.label.Named("CYCLE"), queuedPawn.Named("PAWN")));
                        }
                        else
                        {
                            stringBuilder.Append("RepairGantryCycleSelection".Translate().CapitalizeFirst());
                        }
                    }
                    else
                    {
                        stringBuilder.Append("RepairGantryCycleSelectionNoPower".Translate().CapitalizeFirst());
                    }
                }

                if (state == RepairGantryState.LoadingMaterials)
                {
                    stringBuilder.Append("RepairGantryCycleLabelLoading".Translate().CapitalizeFirst());
                    stringBuilder.AppendLineIfNotEmpty().Append("Materials".Translate()).Append(": ")
                        .Append(storedMaterials);
                }

                if (state == RepairGantryState.Occupied)
                {
                    float num = currentCycleTicksRemaining / CycleSpeedFactor;
                    stringBuilder.AppendLineIfNotEmpty().Append("Contains".Translate()).Append(": ")
                        .Append(Occupant.NameShortColored.Resolve());
                    if (!PowerOn)
                    {
                        stringBuilder.AppendLine().Append("BiosculpterCycleNoPowerInterrupt".Translate((60000 - currentCyclePowerCutTicks).ToStringTicksToPeriod().Named("TIME")).Colorize(ColorLibrary.RedReadable));
                    }

                    stringBuilder.AppendLine().Append("BiosculpterCycleTimeRemaining".Translate()).Append(": ")
                        .Append(((int)num).ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor));
                    Ideo ideo = Occupant.Ideo;
                    if (ideo != null && ideo.HasPrecept(PreceptDefOf.Biosculpting_Accelerated))
                    {
                        stringBuilder.Append(" (" + "BiosculpterCycleAccelerated".Translate() + ")");
                    }

                    stringBuilder.AppendLine().Append("BiosculpterCleanlinessSpeedFactor".Translate()).Append(": ")
                        .Append(CleanlinessSpeedFactor.ToStringPercent());
                }
            }

            if (stringBuilder.Length <= 0)
            {
                return null;
            }

            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            RepairGantryState state = State;
            string cycleIndependentCannotUseReason = CannotUseNowReason();
            foreach (CompRepairGantry_Cycle cycle in AvailableCycles)
            {
                string text = cycleIndependentCannotUseReason ?? CannotUseNowCycleReason(cycle);
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "RepairGantryCycleCommand".Translate(cycle.Props.label) + "";
                command_Action.defaultDesc = CycleDescription(cycle);
                command_Action.icon = cycle.Props.Icon;
                command_Action.action = delegate
                {
                    SelectPawnsForCycleOptions(cycle, out var options2);
                    Find.WindowStack.Add(new FloatMenu(options2));

                };
                command_Action.activateSound = SoundDefOf.Tick_Tiny;
                command_Action.Disabled = text != null;
                List<FloatMenuOption> options;
                if (text != null)
                {
                    command_Action.Disable(text);
                }
                else if (!SelectPawnsForCycleOptions(cycle, out options, shortCircuit: true))
                {
                    command_Action.Disable("BiosculpterNoEligiblePawns".Translate());
                }

                yield return command_Action;
            }

            if (state == RepairGantryState.Occupied)
            {
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "BiosculpterInteruptCycle".Translate();
                command_Action2.defaultDesc = "BiosculpterInteruptCycleDesc".Translate();
                command_Action2.icon = InterruptCycleIcon;
                command_Action2.action = delegate
                {
                    EjectContents(interrupted: true, playSounds: true);
                };
                command_Action2.activateSound = SoundDefOf.Designate_Cancel;
                yield return command_Action2;
            }

            Command_Toggle command_Toggle = new Command_Toggle();
            command_Toggle.defaultLabel = "BiosculpterAutoLoadNutritionLabel".Translate();
            command_Toggle.defaultDesc = "BiosculpterAutoLoadNutritionDescription".Translate();
            command_Toggle.icon = (autoLoadMaterials ? TexCommand.ForbidOff : TexCommand.ForbidOn);
            command_Toggle.isActive = () => autoLoadMaterials;
            command_Toggle.toggleAction = delegate
            {
                autoLoadMaterials = !autoLoadMaterials;
            };
            yield return command_Toggle;

            foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(allowedMaterialsSettings))
            {
                yield return item;
            }

            Gizmo gizmo = Building.SelectContainedItemGizmo(parent, Occupant);
            if (gizmo != null)
            {
                yield return gizmo;
            }

            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: complete cycle",
                    action = delegate
                    {
                        currentCycleTicksRemaining = 10f;
                    },
                    Disabled = (State != RepairGantryState.Occupied)
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: advance cycle +1 day",
                    action = delegate
                    {
                        currentCycleTicksRemaining -= 60000f;
                    },
                    Disabled = (State != RepairGantryState.Occupied)
                };
                yield return new Command_Action
                {
                    defaultLabel = "DEV: fill nutrition and cycle ingredients",
                    action = delegate
                    {
                        storedMaterials = 5;
                        devFillPodLatch = true;
                    },
                    Disabled = (State == RepairGantryState.Occupied || (devFillPodLatch && storedMaterials == 5f))
                };
            }
        }

        public string IngredientsDescription(CompRepairGantry_Cycle cycle)
        {
            tmpIngredientsStrings.Clear();
            if (!cycle.Props.extraRequiredIngredients.NullOrEmpty() && !devFillPodLatch)
            {
                for (int i = 0; i < cycle.Props.extraRequiredIngredients.Count; i++)
                {
                    tmpIngredientsStrings.Add(cycle.Props.extraRequiredIngredients[i].Summary);
                }
            }

            return tmpIngredientsStrings.ToCommaList(useAnd: true);
        }

        public string CycleDescription(CompRepairGantry_Cycle cycle)
        {
            StringBuilder stringBuilder = new StringBuilder();
            float num = cycle.Props.durationDays / CycleSpeedFactor;
            float num2 = num / PreceptDefOf.Biosculpting_Accelerated.biosculpterPodCycleSpeedFactor;
            stringBuilder.AppendLine("\n\n" + "RepairGantryCycleDuration".Translate() + ": " + ((int)(num * 60000f)).ToStringTicksToDays());
            if (!Find.IdeoManager.classicMode)
            {
                stringBuilder.Append("RepairGantryCycleDurationTranshumanists".Translate() + ": " + ((int)(num2 * 60000f)).ToStringTicksToDays());
            }

            return stringBuilder.ToString();
        }

        public bool PawnCanUseNow(Pawn pawn, CompRepairGantry_Cycle cycle)
        {
            return (CannotUseNowReason() ?? CannotUseNowPawnReason(pawn) ?? CannotUseNowCycleReason(cycle) ?? CannotUseNowPawnCycleReason(pawn, cycle)) == null;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (selPawn.IsQuestLodger())
            {
                yield return new FloatMenuOption("CannotEnter".Translate() + ": " + "CryptosleepCasketGuestsNotAllowed".Translate().CapitalizeFirst(), null);
                yield break;
            }

            string cycleIndependentfailureReason = CannotUseNowReason() ?? CannotUseNowPawnReason(selPawn);
            foreach (CompRepairGantry_Cycle cycle in AvailableCycles)
            {
                string text = cycleIndependentfailureReason ?? CannotUseNowCycleReason(cycle) ?? CannotUseNowPawnCycleReason(selPawn, cycle);
                if (text != null)
                {
                    yield return new FloatMenuOption(CannotStartText(cycle, text), null);
                    continue;
                }

                string label = "EnterRepairGantry".Translate(cycle.Props.label, ((int)(cycle.Props.durationDays / GetCycleSpeedFactorForPawn(selPawn) * 60000f)).ToStringTicksToDays());
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, delegate
                {
                    PrepareCycleJob(selPawn, selPawn, cycle, EnterBiosculpterJob());
                }), selPawn, parent);
            }
        }


        public static string CannotStartText(CompRepairGantry_Cycle cycle, string translatedReason)
        {
            return "BiosculpterCannotStartCycle".Translate(cycle.Props.label) + ": " + translatedReason.CapitalizeFirst();
        }

        public string CannotUseNowCycleReason(CompRepairGantry_Cycle cycle)
        {
            List<string> list = cycle.MissingResearchLabels();
            if (list.Any())
            {
                return "MissingRequiredResearch".Translate() + " " + list.ToCommaList();
            }

            return null;
        }

        public string CannotUseNowPawnCycleReason(Pawn p, CompRepairGantry_Cycle cycle, bool checkIngredients = true)
        {
            return CannotUseNowPawnCycleReason(p, p, cycle, checkIngredients);
        }

        public bool CanReachOrHasIngredients(Pawn hauler, Pawn biosculptee, CompRepairGantry_Cycle cycle, bool useCache = false)
        {
            if (!PawnCarryingExtraCycleIngredients(biosculptee, cycle) && (biosculptee == hauler || !PawnCarryingExtraCycleIngredients(hauler, cycle)))
            {
                return CanReachRequiredIngredients(hauler, cycle, useCache);
            }

            return true;
        }

        public string CannotUseNowPawnCycleReason(Pawn hauler, Pawn biosculptee, CompRepairGantry_Cycle cycle, bool checkIngredients = true)
        {
            if (checkIngredients && !CanReachOrHasIngredients(hauler, biosculptee, cycle, useCache: true))
            {
                return "BiosculpterMissingIngredients".Translate(IngredientsDescription(cycle).Named("INGREDIENTS")).CapitalizeFirst();
            }

            return null;
        }

        public string CannotUseNowPawnReason(Pawn p)
        {
            if (!p.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
            {
                return "NoPath".Translate().CapitalizeFirst();
            }

            return null;
        }

        public string CannotUseNowReason()
        {
            if (!PowerOn)
            {
                return "NoPower".Translate().CapitalizeFirst();
            }

            if (State == RepairGantryState.LoadingMaterials)
            {
                return "BiosculpterNutritionNotLoaded".Translate().CapitalizeFirst();
            }

            if (State == RepairGantryState.Occupied)
            {
                return "BiosculpterOccupied".Translate().CapitalizeFirst();
            }

            return null;
        }

        public List<IngredientCount> RequiredIngredients(CompRepairGantry_Cycle cycle)
        {
            List<ThingDefCountClass> extraRequiredIngredients = cycle.Props.extraRequiredIngredients;
            if (extraRequiredIngredients == null || devFillPodLatch)
            {
                return null;
            }

            if (!cachedExtraIngredients.ContainsKey(cycle))
            {
                cachedExtraIngredients[cycle] = extraRequiredIngredients.Select((ThingDefCountClass tc) => tc.ToIngredientCount()).ToList();
            }

            return cachedExtraIngredients[cycle];
        }

        public bool CanReachRequiredIngredients(Pawn pawn, CompRepairGantry_Cycle cycle, bool useCache = false)
        {
            chosenExtraItems.Clear();
            if (cycle.Props.extraRequiredIngredients == null || devFillPodLatch)
            {
                return true;
            }

            float realtimeSinceStartup = Time.realtimeSinceStartup;
            if (useCache && cacheReachIngredientsPawn == pawn && cacheReachIngredientsCycle == cycle && realtimeSinceStartup < cacheReachIngredientsTime + 2f)
            {
                return cacheReachIngredientsResult;
            }

            cacheReachIngredientsPawn = pawn;
            cacheReachIngredientsCycle = cycle;
            cacheReachIngredientsTime = realtimeSinceStartup;
            cacheReachIngredientsResult = WorkGiver_DoBill.TryFindBestFixedIngredients(RequiredIngredients(cycle), pawn, parent, chosenExtraItems);
            return cacheReachIngredientsResult;
        }

        public bool SelectPawnCycleOption(Pawn pawn, CompRepairGantry_Cycle cycle, out FloatMenuOption option)
        {
            string text = CannotUseNowPawnReason(pawn) ?? CannotUseNowPawnCycleReason(pawn, cycle, checkIngredients: false);
            string label = pawn.Label + ((text == null) ? "" : (": " + text));
            Action action = null;
            if (text == null)
            {
                action = delegate
                {
                    PrepareCycleJob(pawn, pawn, cycle, EnterBiosculpterJob());
                };
            }

            option = new FloatMenuOption(label, action);
            return text == null;
        }

        public bool SelectPawnsForCycleOptions(CompRepairGantry_Cycle cycle, out List<FloatMenuOption> options, bool shortCircuit = false)
        {
            cycleEligiblePawnOptions.Clear();
            options = cycleEligiblePawnOptions;
            if (!cachedAnyPawnEligible.ContainsKey(cycle))
            {
                cachedAnyPawnEligible[cycle] = new CacheAnyPawnEligibleCycle
                {
                    gameTime = float.MinValue
                };
            }

            int ticksGame = Find.TickManager.TicksGame;
            if (shortCircuit && (float)ticksGame < cachedAnyPawnEligible[cycle].gameTime + 2f)
            {
                return cachedAnyPawnEligible[cycle].anyEligible;
            }

            cachedAnyPawnEligible[cycle].gameTime = ticksGame;
            foreach (Pawn item in parent.Map.mapPawns.FreeColonistsSpawned)
            {
                if (SelectPawnCycleOption(item, cycle, out var option2) && shortCircuit)
                {
                    cachedAnyPawnEligible[cycle].anyEligible = true;
                    return cachedAnyPawnEligible[cycle].anyEligible;
                }

                cycleEligiblePawnOptions.Add(option2);
            }


            cachedAnyPawnEligible[cycle].anyEligible = cycleEligiblePawnOptions.Count > 0;
            return cachedAnyPawnEligible[cycle].anyEligible;
        }

        public Job EnterBiosculpterJob()
        {
            return JobMaker.MakeJob(A2_Defof.A2_EnterRepairGantry, parent);
        }

        public Job MakeCarryToBiosculpterJob(Pawn willBeCarried)
        {
            return JobMaker.MakeJob(A2_Defof.A2_CarryToRepairGantry, willBeCarried, LocalTargetInfo.Invalid, parent);
        }

        public void ConfigureJobForCycle(Job job, CompRepairGantry_Cycle cycle, List<ThingCount> extraIngredients)
        {
            if (!extraIngredients.NullOrEmpty())
            {
                job.targetQueueB = new List<LocalTargetInfo>(extraIngredients.Count);
                job.countQueue = new List<int>(extraIngredients.Count);
                foreach (ThingCount extraIngredient in extraIngredients)
                {
                    job.targetQueueB.Add(extraIngredient.Thing);
                    job.countQueue.Add(extraIngredient.Count);
                }
            }

            job.haulMode = HaulMode.ToCellNonStorage;
            job.biosculpterCycleKey = cycle.Props.key;
        }

        public void PrepareCycleJob(Pawn hauler, Pawn biosculptee, CompRepairGantry_Cycle cycle, Job job)
        {
            OrderToPod(cycle, biosculptee, delegate
            {
                chosenExtraItems.Clear();
                if (!CanReachOrHasIngredients(hauler, biosculptee, cycle))
                {
                    Messages.Message("BiosculpterMissingIngredients".Translate(IngredientsDescription(cycle).Named("INGREDIENTS")).CapitalizeFirst(), parent, MessageTypeDefOf.NegativeEvent, historical: false);
                }
                else
                {
                    ConfigureJobForCycle(job, cycle, chosenExtraItems);
                    if (cycle.Props.extraRequiredIngredients != null && !devFillPodLatch)
                    {
                        if (job.def == A2_Defof.A2_CarryToRepairGantry)
                        {
                            Messages.Message("BiosculpterCarryStartedMessage".Translate(hauler.Named("PAWN"), IngredientsDescription(cycle).Named("INGREDIENTS"), biosculptee.Named("DOWNED"), cycle.Props.label.Named("CYCLE")), parent, MessageTypeDefOf.SilentInput, historical: false);
                        }
                        else
                        {
                            Messages.Message("BiosculpterLoadingStartedMessage".Translate(hauler.Named("PAWN"), IngredientsDescription(cycle).Named("INGREDIENTS"), cycle.Props.label.Named("CYCLE")), parent, MessageTypeDefOf.SilentInput, historical: false);
                        }
                    }

                    if (hauler.jobs.TryTakeOrderedJob(job, JobTag.Misc))
                    {
                        SetQueuedInformation(job, biosculptee);
                    }
                }
            });
        }

        public void ClearQueuedInformation()
        {
            SetQueuedInformation(null, null);
        }

        public void SetQueuedInformation(Job job, Pawn biosculptee)
        {
            queuedEnterJob = job;
            queuedPawn = biosculptee;
        }

        public bool CanAcceptNutrition(Thing thing)
        {
            return allowedMaterialsSettings.AllowedToAccept(thing);
        }

        public bool CanAcceptOnceCycleChosen(Pawn pawn)
        {
            if (State != RepairGantryState.SelectingCycle || !PowerOn)
            {
                return false;
            }

            return true;
        }

        public bool PawnCarryingExtraCycleIngredients(Pawn pawn, string cycleKey, bool remove = false)
        {
            return PawnCarryingExtraCycleIngredients(pawn, GetCycle(cycleKey), remove);
        }

        public bool PawnCarryingExtraCycleIngredients(Pawn pawn, CompRepairGantry_Cycle cycle, bool remove = false)
        {
            if (cycle.Props.extraRequiredIngredients.NullOrEmpty() || devFillPodLatch)
            {
                return true;
            }

            foreach (ThingDefCountClass extraRequiredIngredient in cycle.Props.extraRequiredIngredients)
            {
                if (pawn.inventory.Count(extraRequiredIngredient.thingDef) < extraRequiredIngredient.count)
                {
                    return false;
                }
            }

            if (remove)
            {
                foreach (ThingDefCountClass extraRequiredIngredient2 in cycle.Props.extraRequiredIngredients)
                {
                    pawn.inventory.RemoveCount(extraRequiredIngredient2.thingDef, extraRequiredIngredient2.count);
                }
            }

            return true;
        }

        public bool TryAcceptPawn(Pawn pawn, string cycleKey)
        {
            return TryAcceptPawn(pawn, GetCycle(cycleKey));
        }

        public bool TryAcceptPawn(Pawn pawn, CompRepairGantry_Cycle cycle)
        {
            if (!CanAcceptOnceCycleChosen(pawn))
            {
                return false;
            }

            if (!PawnCarryingExtraCycleIngredients(pawn, cycle, remove: true))
            {
                return false;
            }

            currentCycleKey = cycle.Props.key;
            innerContainer.ClearAndDestroyContents();
            pawnEnteringGantry = pawn;
            bool num = pawn.DeSpawnOrDeselect();
            if (pawn.holdingOwner != null)
            {
                pawn.holdingOwner.TryTransferToContainer(pawn, innerContainer);
            }
            else
            {
                innerContainer.TryAdd(pawn);
            }

            if (num)
            {
                Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
            }

            pawnEnteringGantry = null;
            currentCycleTicksRemaining = cycle.Props.durationDays * 60000f;
            storedMaterials = 0;
            devFillPodLatch = false;
            ClearQueuedInformation();
            tickEntered = Find.TickManager.TicksGame;
            return true;
        }

        public void EjectContents(bool interrupted, bool playSounds, Map destMap = null)
        {
            if (destMap == null)
            {
                destMap = parent.Map;
            }

            Pawn occupant = Occupant;
            currentCycleKey = null;
            currentCycleTicksRemaining = 0f;
            currentCyclePowerCutTicks = 0;
            storedMaterials = 0;
            devFillPodLatch = false;
            innerContainer.TryDropAll(parent.InteractionCell, destMap, ThingPlaceMode.Near);
            if (occupant != null)
            {
                FilthMaker.TryMakeFilth(parent.InteractionCell, destMap, ThingDefOf.Filth_PodSlime, new IntRange(3, 6).RandomInRange);
                if (interrupted)
                {
                    occupant.needs?.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SoakingWet);
                    occupant.health?.AddHediff(HediffDefOf.BiosculptingSickness);
                }
            }

            if (playSounds)
            {
                Props.exitSound?.PlayOneShot(SoundInfo.InMap(new TargetInfo(parent.Position, parent.Map)));
            }
        }

        public void CycleCompleted()
        {
            Pawn occupant = Occupant;
            CompRepairGantry_Cycle currentCycle = CurrentCycle;
            currentCycle.CycleCompleted(occupant);
            EjectContents(interrupted: false, playSounds: true);
            if (occupant != null)
            {
                Need_Food need_Food = occupant.needs?.food;
                if (need_Food != null)
                {
                    need_Food.CurLevelPercentage = 1f;
                }

                Need_Rest need_Rest = occupant.needs?.rest;
                if (need_Rest != null)
                {
                    need_Rest.CurLevelPercentage = 1f;
                }

                if (currentCycle.Props.gainThoughtOnCompletion != null)
                {
                    occupant.needs?.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.AgeReversalReceived);
                }

                //  Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.UsedRepairGantry, occupant.Named(HistoryEventArgsNames.Doer)));
            }

            if (tickEntered > 0)
            {
                occupant.drugs.Notify_LeftSuspension(Find.TickManager.TicksGame - tickEntered);
            }
        }

        public void StoreMaterials()
        {
            tmpItems.AddRange(innerContainer);
            foreach (Thing tmpItem in tmpItems)
            {
                int num = tmpItem.stackCount;
                if (!(num <= 0f) && !(tmpItem is Pawn))
                {
                    storedMaterials = Math.Min(5, storedMaterials + num);
                    tmpItem.Destroy();
                }
            }

            tmpItems.Clear();
        }

        public override void CompTick()
        {
            if (!ModLister.CheckIdeology("Biosculpting"))
            {
                return;
            }

            base.CompTick();
            if (State != RepairGantryState.SelectingCycle || !PowerOn)
            {
                readyEffecter?.Cleanup();
                readyEffecter = null;
            }
            else if (Props.readyEffecter != null)
            {
                if (readyEffecter == null)
                {
                    readyEffecter = Props.readyEffecter.Spawn();
                    ColorizeEffecter(readyEffecter, Props.selectCycleColor);
                    readyEffecter.Trigger(parent, new TargetInfo(parent.InteractionCell, parent.Map));
                }

                readyEffecter.EffectTick(parent, new TargetInfo(parent.InteractionCell, parent.Map));
            }

            if (State != RepairGantryState.Occupied)
            {
                progressBarEffecter?.Cleanup();
                progressBarEffecter = null;
                operatingEffecter?.Cleanup();
                operatingEffecter = null;
            }
            else
            {
                Pawn occupant = Occupant;
                if (PowerOn)
                {
                    int num = 1;
                    currentCycleTicksRemaining -= (float)num * CycleSpeedFactor;
                    if (currentCycleTicksRemaining <= 0f)
                    {
                        CycleCompleted();
                    }
                }
                else
                {
                    currentCyclePowerCutTicks++;
                    if (currentCyclePowerCutTicks >= 60000)
                    {
                        EjectContents(interrupted: true, playSounds: true);
                        Messages.Message("BiosculpterNoPowerEjectedMessage".Translate(occupant.Named("PAWN")), occupant, MessageTypeDefOf.NegativeEvent, historical: false);
                    }
                }

                if (currentCycleTicksRemaining > 0f)
                {
                    if (progressBarEffecter == null)
                    {
                        progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
                    }

                    progressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
                    MoteProgressBar moteProgressBar = (progressBarEffecter.children[0] as SubEffecter_ProgressBar)?.mote;
                    if (moteProgressBar != null)
                    {
                        float num2 = CurrentCycle.Props.durationDays * 60000f;
                        moteProgressBar.progress = 1f - Mathf.Clamp01(currentCycleTicksRemaining / num2);
                        int num3 = (parent.RotatedSize.z - 1) / 2;
                        moteProgressBar.offsetZ = 0f - ((float)num3 + 0.5f);
                    }

                    if (Props.operatingEffecter != null)
                    {
                        if (!PowerOn)
                        {
                            operatingEffecter?.Cleanup();
                            operatingEffecter = null;
                        }
                        else
                        {
                            if (operatingEffecter == null)
                            {
                                operatingEffecter = Props.operatingEffecter.Spawn();
                                ColorizeEffecter(operatingEffecter, CurrentCycle.Props.operatingColor);
                                operatingEffecter.Trigger(parent, new TargetInfo(parent.InteractionCell, parent.Map));
                            }

                            operatingEffecter.EffectTick(parent, new TargetInfo(parent.InteractionCell, parent.Map));
                        }
                    }
                }
            }
            SetPower();
        }

        public void SetPower()
        {
            if (powerTraderComp == null)
            {
                powerTraderComp = parent.TryGetComp<CompPowerTrader>();
            }

            if (powerComp == null)
            {
                powerComp = parent.TryGetComp<CompPower>();
            }

            if (State == RepairGantryState.Occupied)
            {
                powerTraderComp.PowerOutput = 0f - powerComp.Props.PowerConsumption;
            }
            else
            {
                powerTraderComp.PowerOutput = 0f - powerComp.Props.idlePowerDraw;
            }
        }

        public void ColorizeEffecter(Effecter effecter, Color color)
        {
            foreach (SubEffecter child in effecter.children)
            {
                if (child is SubEffecter_Sprayer subEffecter_Sprayer)
                {
                    subEffecter_Sprayer.colorOverride = color * child.def.color;
                }
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            Rot4 rotation = parent.Rotation;
            Vector3 s = new Vector3(parent.def.graphicData.drawSize.x * 0.8f, 1f, parent.def.graphicData.drawSize.y * 0.8f);
            Vector3 drawPos = parent.DrawPos;
            drawPos.y -= 0.07317074f;
            Graphics.DrawMesh(MeshPool.plane10, Matrix4x4.TRS(drawPos, rotation.AsQuat, s), BackgroundMat, 0);
            if (State == RepairGantryState.Occupied)
            {
                Pawn occupant = Occupant;
                Vector3 drawLoc = parent.DrawPos + FloatingOffset(currentCycleTicksRemaining + (float)currentCyclePowerCutTicks);
                Rot4 rotation2 = parent.Rotation;
                if (rotation2 == Rot4.East || rotation2 == Rot4.West)
                {
                    drawLoc.z += 0.2f;
                }

                occupant.Drawer.renderer.RenderPawnAt(drawLoc, null, neverAimWeapon: true);
            }
        }

        public static Vector3 FloatingOffset(float tickOffset)
        {
            float num = tickOffset % 500f / 500f;
            float num2 = Mathf.Sin((float)Math.PI * num);
            float z = num2 * num2 * 0.04f;
            return new Vector3(0f, 0f, z);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public StorageSettings GetStoreSettings()
        {
            return allowedMaterialsSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return parent.def.building.fixedStorageSettings;
        }

        public void Notify_SettingsChanged()
        {
        }

        public static void OrderToPod(CompRepairGantry_Cycle cycle, Pawn pawn, Action giveJobAct)
        {
            if (cycle is CompRepairGantry_BaseCycle compRepairGantry_HealingCycle)
            {
                string healingDescriptionForPawn = compRepairGantry_HealingCycle.GetHealingDescriptionForPawn(pawn);
                string text = (healingDescriptionForPawn.NullOrEmpty() ? "BiosculpterNoCoditionsToHeal".Translate(pawn.Named("PAWN"), compRepairGantry_HealingCycle.Props.label.Named("CYCLE")).Resolve() : ("OnCompletionOfCycle".Translate(compRepairGantry_HealingCycle.Props.label.Named("CYCLE")).Resolve() + ":\n\n" + healingDescriptionForPawn));
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, giveJobAct, healingDescriptionForPawn.NullOrEmpty()));
            }
            else
            {
                giveJobAct();
            }
        }

        public static Thing FindPodFor(Pawn pawn, Pawn traveller, bool biotuned)
        {
            if (cachedPodDefs.NullOrEmpty())
            {
                cachedPodDefs = DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.GetCompProperties<CompProperties_RepairGantry>() != null).ToList();
            }

            foreach (ThingDef cachedPodDef in cachedPodDefs)
            {
                Thing thing = GenClosest.ClosestThingReachable(traveller.Position, pawn.Map, ThingRequest.ForDef(cachedPodDef), PathEndMode.InteractionCell, TraverseParms.For(pawn), 9999f, Validator);
                if (thing != null)
                {
                    return thing;
                }
            }

            return null;
            bool Validator(Thing t)
            {
                CompRepairGantry compRepairGantry = t.TryGetComp<CompRepairGantry>();

                return compRepairGantry.CanAcceptOnceCycleChosen(traveller);
            }
        }

        public static bool WasLoadingCanceled(Thing thing)
        {
            CompRepairGantry compRepairGantry = thing.TryGetComp<CompRepairGantry>();
            if (compRepairGantry != null && compRepairGantry.State != 0)
            {
                return true;
            }

            return false;
        }

        public void ClearCycle()
        {
            currentCycleKey = null;
        }

        public void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
            StoreMaterials();
            SoundDefOf.Standard_Drop.PlayOneShot(parent);
        }
    }
}
