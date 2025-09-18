using Androids2.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VEF.Apparels;
using Verse;
using Verse.Grammar;
using Verse.Noise;
using Verse.Sound;
using VREAndroids;

namespace Androids2
{
    public class Window_CreateSynth : GeneCreationDialogBase
    {
        protected Action callback;

        protected List<GeneDef> selectedGenes = new List<GeneDef>();

        protected bool? selectedCollapsed = false;

        protected HashSet<GeneCategoryDef> matchingCategories = new HashSet<GeneCategoryDef>();

        protected Dictionary<GeneCategoryDef, bool> collapsedCategories = new Dictionary<GeneCategoryDef, bool>();

        protected bool hoveredAnyGene;

        protected GeneDef hoveredGene;
        public override Vector2 InitialSize => new Vector2(UI.screenWidth, UI.screenHeight);
        public override List<GeneDef> SelectedGenes => selectedGenes;

        public override string Header => "VREA.CreateAndroid".Translate();
        public override string AcceptButtonLabel => "VREA.CreateAndroid".Translate();

        public List<ThingDefCount> requiredItems;

        public bool disableAndroidHardwareLimitation;

        public Building_SynthPrinter station;

        public Pawn newAndroid;
        public int finalExtraPrintingTimeCost = 0;
        public Vector2 traitsScrollPosition = new Vector2();
        List<Trait> allTraits = new List<Trait>();

        //Customization
        public PawnKindDef currentPawnKindDef = VREA_DefOf.VREA_AndroidBasic;
        public BackstoryDef newChildhoodBackstory;
        public BackstoryDef newAdulthoodBackstory;
        public Trait replacedTrait;
        public Trait newTrait;
        public List<ThingOrderRequest> requestedItems = new List<ThingOrderRequest>();
        float metMod = 1f;


        //Static Values
        private static readonly Vector2 PawnPortraitSize = new Vector2(100f, 140f);

        public static List<Color> DefaultHairColors = new List<Color>(new Color[] {
            //Mundane
            new Color(0.17f, 0.17f, 0.17f, 1),
            new Color(0.02f, 0.02f, 0.02f, 1f),
            new Color(0.90f, 0.90f, 0.90f, 1f),
            new Color(0.51f, 0.25f, 0.25f, 1f),
            new Color(1.00f, 0.66f, 0.32f, 1f),

            //Exotic
            new Color(0.0f, 0.5f, 1.0f, 1f),
            new Color(1.0f, 0.00f, 0.5f, 1f),
            new Color(1.00f, 0.00f, 0.00f, 1f),
            new Color(0.00f, 1.00f, 0.00f, 1f),
            new Color(0.00f, 1.00f, 1.00f, 1f),
            new Color(0.78f, 0.78f, 0.78f, 1f),
            new Color(0.92f, 0.92f, 0.29f, 1f),
            new Color(0.63f, 0.28f, 0.64f, 1f)
            });

        public IEnumerable<Color> HairColors
        {
            get
            {
                foreach (Color color in DefaultHairColors)
                    yield return color;
                yield break;
            }
        }


        public Window_CreateSynth(Building_SynthPrinter _station, Action callback)
        {
            this.callback = callback;
            xenotypeName = GetAndroidTypeName();
            forcePause = true;
            absorbInputAroundWindow = true;
            alwaysUseFullBiostatsTableHeight = true;
            searchWidgetOffsetX = ButSize.x * 2f + 4f;
            station = _station;
            foreach (GeneCategoryDef allDef in DefDatabase<GeneCategoryDef>.AllDefs)
            {
                collapsedCategories.Add(allDef, value: false);
            }
            selectedGenes = A2_Defof.A2_Synth.xenotypeDef.genes;
            newAndroid = GetNewPawn();
            OnGenesChanged();
        }

        public virtual bool GeneValidator(GeneDef x) => true;

        public override void DoWindowContents(Rect rect)
        {
            if (selectedGenes.Contains(A2_Defof.A2_Hardware_Integration_I))
            {
                metMod = A2_Defof.A2_Hardware_Integration_I.GetModExtension<HardwareIntegration>().complexityMult;
            }
            else if (selectedGenes.Contains(A2_Defof.A2_Hardware_Integration_II))
            {
                metMod = A2_Defof.A2_Hardware_Integration_II.GetModExtension<HardwareIntegration>().complexityMult;
            }
            else if (selectedGenes.Contains(A2_Defof.A2_Hardware_Integration_III))
            {
                metMod = A2_Defof.A2_Hardware_Integration_III.GetModExtension<HardwareIntegration>().complexityMult;
            }
            // Reserve bottom strip for buttons (unchanged)
            Rect content = rect;
            content.yMax -= ButSize.y + 4f;

            // Header bar (35f tall) at the top of content
            Rect headerRect = new Rect(content.x, content.y, content.width, 35f);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, Header);
            Text.Font = GameFont.Small;

            // Main content area below header
            Rect body = content;
            body.yMin += 39f;

            // Inner margins
            const float gutter = 12f;
            float margin = Margin; // keep using your existing Margin
            Rect inner = new Rect(body.x + margin, body.y, body.width - margin * 2f, body.height);

            // Split into left / right columns
            float rightWidth = Mathf.Clamp(inner.width * 0.38f, 360f, 520f); // pick a sane band
            Rect rightCol = new Rect(inner.xMax - rightWidth, inner.y, rightWidth, inner.height);
            Rect leftCol = new Rect(inner.x, inner.y, inner.width - rightWidth - gutter, inner.height);

            // --- LEFT COLUMN (everything that used to be before the three calls) ---

            // If your search box needs the available area, pipe leftCol (not full rect)
            DrawSearchRect(leftCol);

            // Compute sizes based on LEFT column width
            float num = leftCol.width * 0.25f - margin - 10f;
            float num2 = num - 24f - 10f;
            float num3 = Mathf.Max(AndroidStatsTable.HeightForBiostats(requiredItems), postXenotypeHeight);

            // Genes area inside left column
            Rect genesRect = new Rect(leftCol.x, leftCol.y, leftCol.width, leftCol.height - num3 - 8f);
            DrawGenes(genesRect);

            float afterGenesY = genesRect.yMax + 4f;

            // Stats table (left, 75% width of full window previously → now relative to leftCol)
            Rect statsRect = new Rect(
                leftCol.x, afterGenesY,
                leftCol.width, // use full left column width
                num3
            );
            statsRect.yMax = genesRect.yMax + num3 + 4f;
            AndroidStatsTable.Draw(statsRect, gcx, (int)(met * metMod), requiredItems);

            // Name line + text field and buttons
            string label = AndroidName().CapitalizeFirst() + ":";
            float labelWidth = Text.CalcSize(label).x;

            Rect nameLabelRect = new Rect(
                statsRect.xMax + /* no extra margin to the right of stats, keep within leftCol */ 0,
                afterGenesY,
                labelWidth,
                Text.LineHeight
            );
            // Keep the label inside leftCol; clamp if needed
            nameLabelRect.x = Mathf.Min(nameLabelRect.x, leftCol.xMax - labelWidth);

            Widgets.Label(nameLabelRect, label);

            Rect nameTextRect = new Rect(
                nameLabelRect.xMin,
                nameLabelRect.y + Text.LineHeight,
                num,
                Text.LineHeight
            );
            nameTextRect.xMax = Mathf.Min(leftCol.xMax - 17f - num2 * 0.25f, nameTextRect.xMax);

            string prev = xenotypeName;
            xenotypeName = Widgets.TextField(nameTextRect, xenotypeName, 40, ValidSymbolRegex);
            if (prev != xenotypeName)
            {
                if (xenotypeName.Length > prev.Length && xenotypeName.Length > 3) xenotypeNameLocked = true;
                else if (xenotypeName.Length == 0) xenotypeNameLocked = false;
            }

            // Icon selector aligned to text field baseline
            Rect iconRect = new Rect(nameTextRect.xMax + 4f, nameTextRect.y, 35f, 35f);
            if (iconRect.xMax > leftCol.xMax) iconRect.x = leftCol.xMax - iconRect.width; // clamp
            DrawIconSelector(iconRect);

            // Randomize / "..." / lock buttons (stay within leftCol)
            Rect randBtn = new Rect(nameTextRect.x, nameTextRect.yMax + 4f, num2 * 0.75f - 4f, 24f);
            if (Widgets.ButtonText(randBtn, "Randomize".Translate()))
            {
                GUI.FocusControl(null);
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                xenotypeName = GetAndroidTypeName();
            }

            Rect moreBtn = new Rect(randBtn.xMax + 4f, randBtn.y, num2 * 0.25f, 24f);
            if (Widgets.ButtonText(moreBtn, "..."))
            {
                List<string> list = new List<string>();
                int tries = 0;
                while (list.Count < 20)
                {
                    string t = GetAndroidTypeName();
                    if (t.NullOrEmpty()) break;
                    if (list.Contains(t) || t == xenotypeName)
                    {
                        tries++;
                        if (tries >= 1000) break;
                    }
                    else list.Add(t);
                }
                if (list.Any())
                {
                    var opts = new List<FloatMenuOption>();
                    for (int j = 0; j < list.Count; j++)
                    {
                        string i = list[j];
                        opts.Add(new FloatMenuOption(i, delegate { xenotypeName = i; }));
                    }
                    Find.WindowStack.Add(new FloatMenu(opts));
                }
            }

            Rect lockBtn = new Rect(moreBtn.xMax + 10f, randBtn.y, 24f, 24f);
            if (lockBtn.xMax > leftCol.xMax) lockBtn.x = leftCol.xMax - lockBtn.width; // clamp
            if (Widgets.ButtonImage(lockBtn, xenotypeNameLocked ? LockedTex : UnlockedTex))
            {
                xenotypeNameLocked = !xenotypeNameLocked;
                (xenotypeNameLocked ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff).PlayOneShotOnCamera();
            }
            if (Mouse.IsOver(lockBtn))
            {
                string tip = "LockNameButtonDesc".Translate() + "\n\n" + (xenotypeNameLocked ? "LockNameOn" : "LockNameOff").Translate();
                TooltipHandler.TipRegion(lockBtn, tip);
            }

            postXenotypeHeight = lockBtn.yMax - afterGenesY;
            PostXenotypeOnGUI(nameLabelRect.xMin, randBtn.y + 24f);

            // --- RIGHT COLUMN (moved here) ---

            // Partition right column vertically: portrait/customizations on top, traits/backstories below
            Rect rightTop = new Rect(rightCol.x, rightCol.y, rightCol.width, rightCol.height * 0.6f);
            Rect rightBottom = new Rect(rightCol.x, rightTop.yMax + 8f, rightCol.width, rightCol.yMax - (rightTop.yMax + 8f));

            // Group so child code can use (0,0) local coords safely
            GUI.BeginGroup(rightTop);
            {
                Rect local = new Rect(0f, 0f, rightTop.width, rightTop.height);
                DrawExtraCustomizations(local);
            }
            GUI.EndGroup();

            GUI.BeginGroup(rightBottom);
            {
                Rect local = new Rect(0f, 0f, rightBottom.width, rightBottom.height);
                // Traits and Backstories were previously passed rect12; they ignored yMin, so grouping fixes overlap.
                DrawTraits(local);
                DrawBackstories(local);
            }
            GUI.EndGroup();

            // --- Bottom buttons (unchanged, full-width strip) ---
            Rect bottom = rect;
            bottom.yMin = bottom.yMax - ButSize.y;
            DoBottomButtons(bottom);
        }



        public void DrawExtraCustomizations(Rect rect)
        {
            if (newChildhoodBackstory != null)
            {
                newAndroid.story.Childhood = newChildhoodBackstory;
                newChildhoodBackstory = null;
            }

            if (newAdulthoodBackstory != null)
            {
                newAndroid.story.Adulthood = newAdulthoodBackstory;
                newAdulthoodBackstory = null;
            }

            if (newTrait != null)
            {
                if (replacedTrait != null)
                {
                    newAndroid.story.traits.allTraits.Remove(replacedTrait);
                    replacedTrait = null;
                }

                Trait gainedTrait = new Trait(newTrait.def, newTrait.Degree);


                newAndroid.story.traits.allTraits.Add(gainedTrait);
                if (newAndroid.workSettings != null)
                {
                    newAndroid.workSettings.EnableAndInitialize();
                }
                if (newAndroid.skills != null)
                {
                    newAndroid.skills.Notify_SkillDisablesChanged();
                }
                if (newAndroid.RaceProps.Humanlike)
                {
                    newAndroid.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                }


                newTrait = null;
            }

            Rect pawnRect = new Rect(rect);
            pawnRect.width = PawnPortraitSize.x + 16f;
            pawnRect.height = PawnPortraitSize.y + 16f;
            pawnRect = pawnRect.CenteredOnXIn(rect);
            pawnRect = pawnRect.CenteredOnYIn(rect);
            pawnRect.x += 16f;
            pawnRect.y += 16f;

            //Draw Pawn stuff.
            if (newAndroid != null)
            {
                //Pawn
                Rect pawnRenderRect = new Rect(pawnRect.xMin + (pawnRect.width - PawnPortraitSize.x) / 2f - 10f, pawnRect.yMin + 20f, PawnPortraitSize.x, PawnPortraitSize.y);
                GUI.DrawTexture(pawnRenderRect, PortraitsCache.Get(newAndroid, PawnPortraitSize, Rot4.South, default(Vector3), 1f));

                Widgets.InfoCardButton(pawnRenderRect.xMax - 16f, pawnRenderRect.y, newAndroid);

                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0f, 0f, rect.width, 32f), "AndroidCustomization".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;

                //Name
                float row = 32f;

                Rect rowRect = new Rect(32, row, 256f - 16f, 24f);
                NameTriple nameTriple = newAndroid.Name as NameTriple;
                if (nameTriple != null)
                {
                    Rect rect3 = new Rect(rowRect);
                    rect3.width *= 0.333f;
                    Rect rect4 = new Rect(rowRect);
                    rect4.width *= 0.333f;
                    rect4.x += rect4.width;
                    Rect rect5 = new Rect(rowRect);
                    rect5.width *= 0.333f;
                    rect5.x += rect4.width * 2f;
                    string first = nameTriple.First;
                    string nick = nameTriple.Nick;
                    string last = nameTriple.Last;
                    CharacterCardUtility.DoNameInputRect(rect3, ref first, 12);
                    if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                    CharacterCardUtility.DoNameInputRect(rect4, ref nick, 9);
                    GUI.color = Color.white;
                    CharacterCardUtility.DoNameInputRect(rect5, ref last, 12);
                    if (nameTriple.First != first || nameTriple.Nick != nick || nameTriple.Last != last)
                    {
                        newAndroid.Name = new NameTriple(first, nick, last);
                    }
                    TooltipHandler.TipRegion(rect3, "FirstNameDesc".Translate());
                    TooltipHandler.TipRegion(rect4, "ShortIdentifierDesc".Translate());
                    TooltipHandler.TipRegion(rect5, "LastNameDesc".Translate());
                }
                else
                {
                    rowRect.width = 999f;
                    Text.Font = GameFont.Medium;
                    Widgets.Label(rowRect, newAndroid.Name.ToStringFull);
                    Text.Font = GameFont.Small;
                }


                //Hair customization
                float finalPawnCustomizationWidthOffset = (pawnRect.x + pawnRect.width + 16f);


                rowRect = new Rect(pawnRect.x + pawnRect.width + 16f, pawnRect.y, rect.width - finalPawnCustomizationWidthOffset, 24f);

                //Color
                //newAndroid.story.hairColor
                Rect hairColorRect = new Rect(rowRect);
                hairColorRect.width = hairColorRect.height;

                Widgets.DrawBoxSolid(hairColorRect, newAndroid.story.HairColor);
                Widgets.DrawBox(hairColorRect);
                Widgets.DrawHighlightIfMouseover(hairColorRect);

                if (Widgets.ButtonInvisible(hairColorRect))
                {
                    //Change color
                    Func<Color, Action> setColorAction = (Color color) => delegate
                    {
                        newAndroid.story.HairColor = color;
                        newAndroid.Drawer.renderer.SetAllGraphicsDirty();

                    };

                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    foreach (Color hairColor in HairColors)
                    {
                        list.Add(new FloatMenuOption("AndroidCustomizationChangeColor".Translate(), setColorAction(hairColor), MenuOptionPriority.Default, null, null, 24f, delegate (Rect rect)
                        {
                            Rect colorRect = new Rect(rect);
                            colorRect.x += 8f;
                            Widgets.DrawBoxSolid(colorRect, hairColor);
                            Widgets.DrawBox(colorRect);
                            return false;
                        }, null));
                    }
                    Find.WindowStack.Add(new FloatMenu(list));
                }

                Rect hairTypeRect = new Rect(rowRect);
                hairTypeRect.width -= hairColorRect.width;
                hairTypeRect.width -= 8f;
                hairTypeRect.x = hairColorRect.x + hairColorRect.width + 8f;

                if (Widgets.ButtonText(hairTypeRect, newAndroid?.story?.hairDef?.LabelCap ?? "Bald"))
                {

                    IEnumerable<HairDef> hairs =
                        from hairdef in DefDatabase<HairDef>.AllDefs
                        where (newAndroid.gender == Gender.Female && (hairdef.styleGender == StyleGender.Any || hairdef.styleGender == StyleGender.Female || hairdef.styleGender == StyleGender.FemaleUsually)) || (newAndroid.gender == Gender.Male && (hairdef.styleGender == StyleGender.Any || hairdef.styleGender == StyleGender.Male || hairdef.styleGender == StyleGender.MaleUsually))
                        select hairdef;

                    if (hairs != null)
                    {
                        FloatMenuUtility.MakeMenu<HairDef>(hairs, hairDef => hairDef.LabelCap, (HairDef hairDef) => delegate
                        {
                            newAndroid.story.hairDef = hairDef;
                            newAndroid.Drawer.renderer.SetAllGraphicsDirty();

                        });
                    }
                }


                if (AlienRaceCompat.AlienRaceKinds.Count() > 1)
                {
                    rowRect = new Rect(32 + 16f + 256f, row, 256f - 16f, 24f);
                    if (Widgets.ButtonText(rowRect, currentPawnKindDef.race.LabelCap))
                    {
                        FloatMenuUtility.MakeMenu<PawnKindDef>(AlienRaceCompat.AlienRaceKinds, raceKind => raceKind.race.LabelCap, (PawnKindDef raceKind) => delegate
                        {
                            currentPawnKindDef = raceKind;

                            //Figure out default gender.
                            Gender defaultGender = Gender.Female;

                            newAndroid = GetNewPawn(defaultGender);
                        });
                    }

                    row += 26f;
                }
                rowRect = new Rect(32 + 16f + 256f, row, 128f - 8f, 24f);

                if (Widgets.ButtonText(rowRect, "AndroidCustomizationRollFemale".Translate()))
                {
                    newAndroid.SetFactionDirect(null);
                    newAndroid.Destroy();
                    newAndroid = GetNewPawn(Gender.Female);
                }

                rowRect = new Rect(32 + 16f + 256f + 128f - 8f, row, 128f - 8f, 24f);

                if (Widgets.ButtonText(rowRect, "AndroidCustomizationRollMale".Translate()))
                {
                    newAndroid.SetFactionDirect(null);
                    newAndroid.Destroy();
                    newAndroid = GetNewPawn(Gender.Male);
                }

            }
        }

        public void DrawTraits(Rect rect)
        {
            float row = 32f;

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Medium;

            Rect traitsLabelRect = new Rect(32f, row, 256f, 26f);
            Widgets.DrawTitleBG(traitsLabelRect);
            Widgets.Label(traitsLabelRect.ContractedBy(2f), "AndroidCustomizationTraitsLabel".Translate());

            Text.Font = GameFont.Small;

            row += 26f;

            Text.Anchor = TextAnchor.MiddleCenter;

            //traitsScrollPosition

            Trait traitToBeRemoved = null;
            float traitRowWidth = 256f;
            float traitRowHeight = 24f;

            float innerTraitsRectHeight = (newAndroid.story.traits.allTraits.Count + 1) * traitRowHeight;

            Rect outerTraitsRect = new Rect(traitsLabelRect);
            outerTraitsRect.y += 26f;
            outerTraitsRect.height = rect.height - outerTraitsRect.y;
            outerTraitsRect.width += 12f;

            Rect innerTraitsRect = new Rect(outerTraitsRect);
            innerTraitsRect.height = innerTraitsRectHeight + 8f;
            //innerTraitsRect.width -= 8f;

            Widgets.BeginScrollView(outerTraitsRect, ref traitsScrollPosition, innerTraitsRect);

            foreach (Trait trait in newAndroid.story.traits.allTraits)
            {
                Rect rowRect = new Rect(32f, row, traitRowWidth, traitRowHeight);
                Widgets.DrawBox(rowRect);
                Widgets.DrawHighlightIfMouseover(rowRect);

                Rect traitLabelRect = new Rect(rowRect);
                traitLabelRect.width -= traitLabelRect.height;

                Rect removeButtonRect = new Rect(rowRect);
                removeButtonRect.width = removeButtonRect.height;
                removeButtonRect.x = traitLabelRect.xMax;

                Widgets.Label(traitLabelRect, trait.LabelCap);


                TooltipHandler.TipRegion(traitLabelRect, trait.TipString(newAndroid));

                //Bring up trait selection menu.
                if (Widgets.ButtonInvisible(traitLabelRect))
                {
                    PickTraitMenu(trait);
                }

                //Removes this trait.
                if (Widgets.ButtonImage(removeButtonRect, TexCommand.ForbidOn))
                {
                    traitToBeRemoved = trait;
                }

                row += 26f;
            }

            Text.Anchor = TextAnchor.MiddleRight;

            //Add traits. Until 7 by default.
            {
                Rect rowRect = new Rect(32f, row, traitRowWidth, traitRowHeight);

                Rect traitLabelRect = new Rect(rowRect);
                traitLabelRect.width -= traitLabelRect.height;

                Rect addButtonRect = new Rect(rowRect);
                addButtonRect.width = addButtonRect.height;
                addButtonRect.x = traitLabelRect.xMax;

                Widgets.Label(traitLabelRect, "AndroidCustomizationAddTraitLabel".Translate(newAndroid.story.traits.allTraits.Count, AndroidCustomizationTweaks.maxTraitsToPick));

                if (Widgets.ButtonImage(addButtonRect, TexCommand.Install) && newAndroid.story.traits.allTraits.Count < AndroidCustomizationTweaks.maxTraitsToPick)
                {
                    PickTraitMenu(null);
                }
            }

            Widgets.EndScrollView();

            Text.Anchor = TextAnchor.UpperLeft;

            if (traitToBeRemoved != null)
            {
                //Remove all associated bonuses and reroll skills.
                //TraitDef traitDef = traitToBeRemoved.def;

                newAndroid.story.traits.allTraits.Remove(traitToBeRemoved);

                traitToBeRemoved = null;
            }

        }

        public void DrawBackstories(Rect rect)
        {

            Rect rowRect = new Rect(48 + 256, 32, 256f - 16f, 24f);

            Widgets.DrawBox(rowRect);
            Widgets.DrawHighlightIfMouseover(rowRect);

            string label = "";

            if (newAndroid.story.Childhood != null)
                label = "AndroidCustomizationFirstIdentity".Translate() + " " + newAndroid.story.Childhood.TitleCapFor(newAndroid.gender);
            else
                label = "AndroidCustomizationFirstIdentity".Translate() + " " + "AndroidNone".Translate();

            if (Widgets.ButtonText(rowRect, label))
            {
                IEnumerable<BackstoryDef> backstories = from backstory in (from backstoryDef in DefDatabase<BackstoryDef>.AllDefs.ToList() select backstoryDef)
                                                        where (backstory.spawnCategories.Any(category => (currentPawnKindDef.backstoryCategories != null && currentPawnKindDef.backstoryCategories.Any(subCategory => subCategory == category))) || backstory.spawnCategories.Contains("A2_Model")) && backstory.slot == BackstorySlot.Childhood
                                                        select backstory;
                FloatMenuUtility.MakeMenu<BackstoryDef>(backstories, backstory => backstory.TitleCapFor(newAndroid.gender), (BackstoryDef backstory) => delegate
                {
                    newChildhoodBackstory = backstory;
                });
            }

            if (newAndroid.story.Childhood != null)
                TooltipHandler.TipRegion(rowRect, newAndroid.story.Childhood.FullDescriptionFor(newAndroid));
            rowRect = new Rect(48f + 256, 64, 256f - 16f, 24f);

            Widgets.DrawBox(rowRect);
            Widgets.DrawHighlightIfMouseover(rowRect);

            label = "";

            if (newAndroid.story.Adulthood != null)
                label = "AndroidCustomizationSecondIdentity".Translate() + " " + newAndroid.story.Adulthood.TitleCapFor(newAndroid.gender);
            else
                label = "AndroidCustomizationSecondIdentity".Translate() + " " + "AndroidNone".Translate();

            if (Widgets.ButtonText(rowRect, label))
            {
                IEnumerable<BackstoryDef> backstories = from backstory in (from backstoryDef in DefDatabase<BackstoryDef>.AllDefs.ToList()
                                                                           select backstoryDef)
                                                        where (backstory.spawnCategories.Any(category => currentPawnKindDef.backstoryCategories != null && currentPawnKindDef.backstoryCategories.Any(subCategory => subCategory == category)) || backstory.spawnCategories.Contains("A2_Specialization")) && backstory.slot == BackstorySlot.Adulthood
                                                        select backstory;
                FloatMenuUtility.MakeMenu<BackstoryDef>(backstories, backstory => backstory.TitleCapFor(newAndroid.gender), (BackstoryDef backstory) => delegate
                {
                    newAdulthoodBackstory = backstory;
                });
            }

            if (newAndroid.story.Adulthood != null)
                TooltipHandler.TipRegion(rowRect, newAndroid.story.Adulthood.FullDescriptionFor(newAndroid));

        }



        private string GetAndroidTypeName()
        {
            var rootKeyword = VREA_DefOf.VREA_AndroidTypeNameMaker.RulesPlusIncludes
                .Where(x => x.keyword == "r_name").RandomElement().keyword;
            var request = default(GrammarRequest);
            request.Rules.Add(new Rule_String("TotalComplexityNumber", gcx.ToString()));
            request.Includes.Add(VREA_DefOf.VREA_AndroidTypeNameMaker);
            return GrammarResolver.Resolve(rootKeyword, request);
        }

        protected virtual TaggedString AndroidName()
        {
            return "VREA.AndroidName".Translate();
        }

        public override void Accept()
        {
            AcceptInner();
            callback?.Invoke();
            Close();
        }

        public override void PostOpen()
        {
            if (!ModLister.CheckBiotech("xenotype creation"))
            {
                Close(doCloseSound: false);
            }
            else
            {
                base.PostOpen();
            }
        }

        public override void DrawGenes(Rect rect)
        {
            hoveredAnyGene = false;
            GUI.BeginGroup(rect);
            float curY = 0f;
            DrawSection(new Rect(0f, 0f, rect.width, selectedHeight), selectedGenes, "VREA.SelectedComponents".Translate(), ref curY, ref selectedHeight, adding: false, rect, ref selectedCollapsed);
            if (!selectedCollapsed.Value)
            {
                curY += 10f;
            }
            float num = curY;
            Widgets.Label(0f, ref curY, rect.width, "VREA.Components".Translate().CapitalizeFirst());
            curY += 10f;
            float height = curY - num - 4f;
            if (Widgets.ButtonText(new Rect(rect.width - 150f - 16f, num, 150f, height), "CollapseAllCategories".Translate()))
            {
                SoundDefOf.TabClose.PlayOneShotOnCamera();
                foreach (GeneCategoryDef allDef in DefDatabase<GeneCategoryDef>.AllDefs)
                {
                    collapsedCategories[allDef] = true;
                }
            }
            if (Widgets.ButtonText(new Rect(rect.width - 300f - 4f - 16f, num, 150f, height), "ExpandAllCategories".Translate()))
            {
                SoundDefOf.TabOpen.PlayOneShotOnCamera();
                foreach (GeneCategoryDef allDef2 in DefDatabase<GeneCategoryDef>.AllDefs)
                {
                    collapsedCategories[allDef2] = false;
                }
            }
            float num2 = curY;
            Rect rect2 = new Rect(0f, curY, rect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(new Rect(0f, curY, rect.width, rect.height - curY), ref scrollPosition, rect2);
            Rect containingRect = rect2;
            containingRect.y = curY + scrollPosition.y;
            containingRect.height = rect.height;
            bool? collapsed = null;
            DrawSection(rect, VREAndroids.Utils.AndroidGenesGenesInOrder.Where(GeneValidator).ToList(), null, ref curY, ref unselectedHeight, adding: true, containingRect, ref collapsed);
            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = curY - num2;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            if (!hoveredAnyGene)
            {
                hoveredGene = null;
            }
        }

        private void DrawSection(Rect rect, List<GeneDef> genes, string label, ref float curY, ref float sectionHeight, bool adding, Rect containingRect, ref bool? collapsed)
        {
            float curX = 4f;
            if (!label.NullOrEmpty())
            {
                Rect rect2 = new Rect(0f, curY, rect.width, Text.LineHeight);
                rect2.xMax -= (adding ? 16f : (Text.CalcSize("ClickToAddOrRemove".Translate()).x + 4f));
                if (collapsed.HasValue)
                {
                    Rect position = new Rect(rect2.x, rect2.y + (rect2.height - 18f) / 2f, 18f, 18f);
                    GUI.DrawTexture(position, collapsed.Value ? TexButton.Reveal : TexButton.Collapse);
                    if (Widgets.ButtonInvisible(rect2))
                    {
                        collapsed = !collapsed;
                        if (collapsed.Value)
                        {
                            SoundDefOf.TabClose.PlayOneShotOnCamera();
                        }
                        else
                        {
                            SoundDefOf.TabOpen.PlayOneShotOnCamera();
                        }
                    }
                    if (Mouse.IsOver(rect2))
                    {
                        Widgets.DrawHighlight(rect2);
                    }
                    rect2.xMin += position.width;
                }
                Widgets.Label(rect2, label);
                if (!adding)
                {
                    Text.Anchor = TextAnchor.UpperRight;
                    GUI.color = ColoredText.SubtleGrayColor;
                    Widgets.Label(new Rect(rect2.xMax - 18f, curY, rect.width - rect2.width, Text.LineHeight), "ClickToAddOrRemove".Translate());
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                curY += Text.LineHeight + 3f;
            }
            if (collapsed == true)
            {
                if (Event.current.type == EventType.Layout)
                {
                    sectionHeight = 0f;
                }
                return;
            }
            float num = curY;
            bool flag = false;
            float num2 = 34f + GeneSize.x + 8f;
            float num3 = rect.width - 16f;
            float num4 = num2 + 4f;
            float b = (num3 - num4 * Mathf.Floor(num3 / num4)) / 2f;
            Rect rect3 = new Rect(0f, curY, rect.width, sectionHeight);
            if (!adding)
            {
                Widgets.DrawRectFast(rect3, Widgets.MenuSectionBGFillColor);
            }
            curY += 4f;
            if (!genes.Any())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = ColoredText.SubtleGrayColor;
                Widgets.Label(rect3, "(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
            else
            {
                GeneCategoryDef geneCategoryDef = null;
                int num5 = 0;
                for (int i = 0; i < genes.Count; i++)
                {
                    GeneDef geneDef = genes[i];
                    if (geneDef is A2GeneDef a2GeneDef)
                    {
                        if (a2GeneDef.requiredResearch != null && !a2GeneDef.requiredResearch.IsFinished)
                        {
                            continue;
                        }
                    }
                    if ((adding && quickSearchWidget.filter.Active && (!matchingGenes.Contains(geneDef) || selectedGenes.Contains(geneDef)) && !matchingCategories.Contains(geneDef.displayCategory)))
                    {
                        continue;
                    }
                    bool flag2 = false;
                    if (curX + num2 > num3)
                    {
                        curX = 4f;
                        curY += GeneSize.y + 8f + 4f;
                        flag2 = true;
                    }
                    bool flag3 = quickSearchWidget.filter.Active && (matchingGenes.Contains(geneDef)
                        || matchingCategories.Contains(geneDef.displayCategory));
                    bool flag4 = collapsedCategories[geneDef.displayCategory] && !flag3;
                    if (adding && geneCategoryDef != geneDef.displayCategory)
                    {
                        if (!flag2 && flag)
                        {
                            curX = 4f;
                            curY += GeneSize.y + 8f + 4f;
                        }
                        geneCategoryDef = geneDef.displayCategory;
                        Rect rect4 = new Rect(curX, curY, rect.width - 8f, Text.LineHeight);
                        if (!flag3)
                        {
                            Rect position2 = new Rect(rect4.x, rect4.y + (rect4.height - 18f) / 2f, 18f, 18f);
                            GUI.DrawTexture(position2, flag4 ? TexButton.Reveal : TexButton.Collapse);
                            if (Widgets.ButtonInvisible(rect4))
                            {
                                collapsedCategories[geneDef.displayCategory] = !collapsedCategories[geneDef.displayCategory];
                                if (collapsedCategories[geneDef.displayCategory])
                                {
                                    SoundDefOf.TabClose.PlayOneShotOnCamera();
                                }
                                else
                                {
                                    SoundDefOf.TabOpen.PlayOneShotOnCamera();
                                }
                            }
                            if (num5 % 2 == 1)
                            {
                                Widgets.DrawLightHighlight(rect4);
                            }
                            if (Mouse.IsOver(rect4))
                            {
                                Widgets.DrawHighlight(rect4);
                            }
                            rect4.xMin += position2.width;
                        }
                        Widgets.Label(rect4, geneCategoryDef.LabelCap);
                        curY += rect4.height;
                        if (!flag4)
                        {
                            GUI.color = Color.grey;
                            Widgets.DrawLineHorizontal(curX, curY, rect.width - 8f);
                            GUI.color = Color.white;
                            curY += 10f;
                        }
                        num5++;
                    }
                    if (adding && flag4)
                    {
                        flag = false;
                        if (Event.current.type == EventType.Layout)
                        {
                            sectionHeight = curY - num;
                        }
                        continue;
                    }
                    curX = Mathf.Max(curX, b);
                    flag = true;
                    if (DrawGene(geneDef, !adding, ref curX, curY, num2, containingRect, flag3))
                    {
                        if (selectedGenes.Contains(geneDef))
                        {
                            if (geneDef.CanBeRemovedFromAndroid() || disableAndroidHardwareLimitation && geneDef.CanBeRemovedFromAndroidAwakened())
                            {
                                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                                selectedGenes.Remove(geneDef);
                            }
                        }
                        else
                        {
                            SoundDefOf.Tick_High.PlayOneShotOnCamera();
                            selectedGenes.Add(geneDef);
                        }
                        if (!xenotypeNameLocked)
                        {
                            xenotypeName = GetAndroidTypeName();
                        }
                        OnGenesChanged();
                        break;
                    }
                }
            }
            if (!adding || flag)
            {
                curY += GeneSize.y + 12f;
            }
            if (Event.current.type == EventType.Layout)
            {
                sectionHeight = curY - num;
            }
        }

        private bool DrawGene(GeneDef geneDef, bool selectedSection, ref float curX, float curY, float packWidth, Rect containingRect, bool isMatch)
        {
            bool result = false;
            Rect rect = new Rect(curX, curY, packWidth, GeneSize.y + 8f);
            if (!containingRect.Overlaps(rect))
            {
                curX = rect.xMax + 4f;
                return false;
            }
            bool selected = !selectedSection && selectedGenes.Contains(geneDef);
            bool overridden = leftChosenGroups.Any((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(geneDef));
            Widgets.DrawOptionBackground(rect, selected);
            curX += 4f;
            DrawBiostats(geneDef.biostatCpx, geneDef.biostatMet, geneDef.biostatArc, ref curX, curY, 4f);
            Rect rect2 = new Rect(curX, curY + 4f, GeneSize.x, GeneSize.y);
            if (isMatch)
            {
                Widgets.DrawStrongHighlight(rect2.ExpandedBy(6f));
            }
            GeneUIUtility.DrawGeneDef(geneDef, rect2, GeneType.Xenogene, () => GeneTip(geneDef, selectedSection), doBackground: false, clickable: false, overridden);
            curX += GeneSize.x + 4f;
            if (Mouse.IsOver(rect))
            {
                hoveredGene = geneDef;
                hoveredAnyGene = true;
            }
            else if (hoveredGene != null && geneDef.ConflictsWith(hoveredGene))
            {
                Widgets.DrawLightHighlight(rect);
            }
            if (Widgets.ButtonInvisible(rect))
            {
                result = true;
            }
            curX = Mathf.Max(curX, rect.xMax + 4f);
            return result;
        }

        public static void DrawBiostats(int gcx, int met, int arc, ref float curX, float curY, float margin = 6f)
        {
            float num = GeneSize.y / 3f;
            float num2 = 0f;
            float num3 = Text.LineHeightOf(GameFont.Small);
            Rect iconRect = new Rect(curX, curY + margin + num2, num3, num3);
            GeneUIUtility.DrawStat(iconRect, GeneUtility.GCXTex, gcx.ToString(), num3);
            Rect rect = new Rect(curX, iconRect.y, 38f, num3);
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                TooltipHandler.TipRegion(rect, "Complexity".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + "VREA.ComplexityTotalDesc".Translate());
            }
            num2 += num;
            if (met != 0)
            {
                Rect iconRect2 = new Rect(curX, curY + margin + num2, num3, num3);
                if (met < 10)
                {
                    GeneUIUtility.DrawStat(iconRect2, AndroidStatsTable.PowerEfficiencyIconTex, met.ToStringWithSign(), num3);
                }
                else
                {
                    GUI.DrawTexture(iconRect2, AndroidStatsTable.PowerEfficiencyIconTex.Texture);
                    Text.Anchor = TextAnchor.MiddleRight;
                    Widgets.Label(new Rect(iconRect2.xMax - 6, iconRect2.y, num3 + 6, num3), met.ToStringWithSign());
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                Rect rect2 = new Rect(curX, iconRect2.y, 38f, num3);
                if (Mouse.IsOver(rect2))
                {
                    Widgets.DrawHighlight(rect2);
                    TooltipHandler.TipRegion(rect2, "VREA.PowerEfficiency".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n\n"
                        + "VREA.PowerEfficiencyTotalDesc".Translate());
                }
                num2 += num;
            }
            curX += 34f;
        }

        private string GeneTip(GeneDef geneDef, bool selectedSection)
        {
            string text = null;
            if (selectedSection)
            {
                if (leftChosenGroups.Any((GeneLeftChosenGroup x) => x.leftChosen == geneDef))
                {
                    text = GroupInfo(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.leftChosen == geneDef));
                }
                else if (cachedOverriddenGenes.Contains(geneDef))
                {
                    text = GroupInfo(leftChosenGroups.FirstOrDefault((GeneLeftChosenGroup x) => x.overriddenGenes.Contains(geneDef)));
                }
                else if (randomChosenGroups.ContainsKey(geneDef))
                {
                    text = ("VREA.ComponentWillBeRandomChosen".Translate() + ":\n" + randomChosenGroups[geneDef].Select((GeneDef x) => x.label).ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.TipSectionTitleColor);
                }
            }
            if (selectedGenes.Contains(geneDef) && geneDef.prerequisite != null && !selectedGenes.Contains(geneDef.prerequisite))
            {
                if (!text.NullOrEmpty())
                {
                    text += "\n\n";
                }
                text += ("VREA.MessageComponentMissingPrerequisite".Translate(geneDef.label).CapitalizeFirst() + ": " + geneDef.prerequisite.LabelCap).Colorize(ColorLibrary.RedReadable);
            }
            if (!text.NullOrEmpty())
            {
                text += "\n\n";
            }
            if (geneDef.CanBeRemovedFromAndroid() || disableAndroidHardwareLimitation && geneDef.CanBeRemovedFromAndroidAwakened())
            {
                return text + (selectedGenes.Contains(geneDef) ? "ClickToRemove" : "ClickToAdd").Translate().Colorize(ColoredText.SubtleGrayColor);
            }
            return text;
            static string GroupInfo(GeneLeftChosenGroup group)
            {
                if (group == null)
                {
                    return null;
                }
                return ("VREA.ComponentLeftmostActive".Translate() + ":\n  - " + group.leftChosen.LabelCap + " (" + "Active".Translate() + ")" + "\n" + group.overriddenGenes.Select((GeneDef x) => (x.label + " (" + "Suppressed".Translate() + ")").Colorize(ColorLibrary.RedReadable)).ToLineList("  - ", capitalizeItems: true)).Colorize(ColoredText.TipSectionTitleColor);
            }
        }

        public override void PostXenotypeOnGUI(float curX, float curY)
        {

        }

        public override void OnGenesChanged()
        {
            selectedGenes.SortGeneDefs();
            base.OnGenesChanged();
        }

        public override void DoBottomButtons(Rect rect)
        {
            base.DoBottomButtons(rect);
            if (leftChosenGroups.Any())
            {
                int num = leftChosenGroups.Sum((GeneLeftChosenGroup x) => x.overriddenGenes.Count);
                GeneLeftChosenGroup geneLeftChosenGroup = leftChosenGroups[0];
                string text = "VREA.ComponentsConflict".Translate() + ": " + "GenesConflictDesc".Translate(geneLeftChosenGroup.leftChosen.Named("FIRST"), geneLeftChosenGroup.overriddenGenes[0].Named("SECOND")).CapitalizeFirst() + ((num > 1) ? (" +" + (num - 1)) : string.Empty);
                float x2 = Text.CalcSize(text).x;
                GUI.color = ColorLibrary.RedReadable;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rect.xMax - ButSize.x - x2 - 4f, rect.y, x2, rect.height), text);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            else if (met < -20)
            {
                string text = "VREA.TooLowEfficiency".Translate();
                float x2 = Text.CalcSize(text).x;
                GUI.color = ColorLibrary.RedReadable;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rect.xMax - ButSize.x - x2 - 4f, rect.y, x2, rect.height), text);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        public override bool WithinAcceptableBiostatLimits(bool showMessage)
        {
            if (met < AndroidStatsTable.AndroidStatRange.TrueMin)
            {
                if (showMessage)
                {
                    Messages.Message("VREA.EfficiencyTooLowToCreateAndroid".Translate(met.Named("AMOUNT"), AndroidStatsTable.AndroidStatRange.TrueMin.Named("MIN")), null, MessageTypeDefOf.RejectInput, historical: false);
                }
                return false;
            }
            return true;
        }

        public override bool CanAccept()
        {
            if (leftChosenGroups.Any())
            {
                Messages.Message("VREA.MessageConflictingComponentPresent".Translate(), null, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            string text = xenotypeName;
            if (text != null && text.Trim().Length == 0)
            {
                Messages.Message("VREA.AndroidNameCannotBeEmpty".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            if (!WithinAcceptableBiostatLimits(showMessage: true))
            {
                return false;
            }
            List<GeneDef> selectedGenes = SelectedGenes;
            foreach (GeneDef selectedGene in SelectedGenes)
            {
                if (selectedGene.prerequisite != null && !selectedGenes.Contains(selectedGene.prerequisite))
                {
                    Messages.Message("VREA.MessageComponentMissingPrerequisite".Translate(selectedGene.label).CapitalizeFirst() + ": " + selectedGene.prerequisite.LabelCap, null, MessageTypeDefOf.RejectInput, historical: false);
                    return false;
                }
            }
            return true;
        }
        public void AcceptInner()
        {
            CustomXenotype customXenotype = new CustomXenotype();
            customXenotype.name = xenotypeName?.Trim();
            customXenotype.inheritable = false;
            customXenotype.iconDef = iconDef;
            AndroidMakerPatch.ApplyXenotype(newAndroid, selectedGenes, false);
            station.pawnBeingCrafted = newAndroid;
            HardwareIntegration hi = null;
            foreach (GeneDef gene in selectedGenes)
            {
                station.pawnBeingCrafted.genes.AddGene(gene, false);
                customXenotype.genes.Add(gene);
                if (gene is A2GeneDef geneAndroid)
                {
                    finalExtraPrintingTimeCost += geneAndroid.timeCost;
                    station.requestedNutrition += geneAndroid.nutrition;
                    requestedItems.AddRange(geneAndroid.costList);
                    if (geneAndroid.GetModExtension<HardwareIntegration>() is HardwareIntegration extension)
                    {
                        hi = extension;
                    }
                }
            }
            station.orderProcessor.requestedItems = requestedItems;
            station.crafterStatus = CrafterStatus.Filling;
            station.recipe = A2_Defof.A2_Synth;
            station.recipe.customXenotype = customXenotype;
            station.recipe.costList = A2_Defof.A2_Synth.costList;
            station.recipe.costList.AddRange(requestedItems);
            station.recipe.timeCost += finalExtraPrintingTimeCost;
            if (hi != null)
            {
                station.recipe.timeCost = (int)(station.recipe.timeCost * hi.timeMult);
                station.requestedNutrition = (int)(station.requestedNutrition * hi.costMult);
                foreach (var item in station.recipe.costList)
                {
                    item.amount = (int)(item.amount * hi.costMult);
                }
            }
        }

        public override void UpdateSearchResults()
        {
            quickSearchWidget.noResultsMatched = false;
            matchingGenes.Clear();
            matchingCategories.Clear();
            if (!quickSearchWidget.filter.Active)
            {
                return;
            }
            foreach (GeneDef item in GeneUtility.GenesInOrder)
            {
                if (!selectedGenes.Contains(item))
                {
                    if (quickSearchWidget.filter.Matches(item.label))
                    {
                        matchingGenes.Add(item);
                    }
                    if (quickSearchWidget.filter.Matches(item.displayCategory.label))
                    {
                        matchingCategories.Add(item.displayCategory);
                    }
                }
            }
            quickSearchWidget.noResultsMatched = !matchingGenes.Any() && !matchingCategories.Any();
        }

        public void PickTraitMenu(Trait oldTrait)
        {
            //Populate available traits.
            allTraits.Clear();

            foreach (TraitDef def in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                foreach (TraitDegreeData degree in def.degreeDatas)
                {
                    Trait trait = new Trait(def, degree.degree, false);
                    allTraits.Add(trait);
                }
            }

            //Filter out traits we already got.
            //Filter out conflicting traits.
            foreach (Trait trait in newAndroid.story.traits.allTraits)
            {
                //Same traits.
                //allTraits.RemoveAll(aTrait => aTrait.def == trait.def && aTrait.Degree == trait.Degree);
                allTraits.RemoveAll(aTrait => aTrait.def == trait.def);

                //Conflicting traits.
                allTraits.RemoveAll(aTrait => trait.def.conflictingTraits.Contains(aTrait.def));
            }

            FloatMenuUtility.MakeMenu<Trait>(allTraits,
                delegate (Trait labelTrait)
                {
                    return labelTrait.LabelCap;

                },
                (Trait theTrait) =>
                    delegate ()
                    {
                        Trait oldOldTrait = oldTrait;
                        replacedTrait = oldOldTrait;
                        newTrait = theTrait;
                        //Log.Message("theTrait: " + theTrait?.LabelCap ?? "No trait!!");
                    });
        }


        public Pawn GetNewPawn(Gender gender = Gender.Female)
        {
            //Make base pawn.
            Pawn pawn;

            pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, station.Faction, RimWorld.PawnGenerationContext.NonPlayer, fixedGender: gender, allowPregnant: false, allowAddictions: false, forceGenerateNewPawn: true));


            //Destroy all equipment and items in inventory.
            pawn?.equipment.DestroyAllEquipment();
            pawn?.inventory.DestroyAll();

            //Strip off clothes and replace with bandages.
            pawn.apparel.DestroyAll();
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }

            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty();
            }

            return pawn;
        }

    }
}
