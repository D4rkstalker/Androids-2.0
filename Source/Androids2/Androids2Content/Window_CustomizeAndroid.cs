using Androids2.Extensions;
using Androids2.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2.Androids2Content
{
    public class Window_CustomizeAndroid : Window
    {
        //Variables
        public Building_AndroidCreationStation acs;
        public Pawn newAndroid;
        public int finalExtraPrintingTimeCost = 0;
        public bool refreshAndroidPortrait = false;
        public Vector2 upgradesScrollPosition = new Vector2();
        public Vector2 traitsScrollPosition = new Vector2();
        List<Trait> allTraits = new List<Trait>();

        //Customization
        public PawnKindDef currentPawnKindDef;
        public BackstoryDef newChildhoodBackstory;
        public BackstoryDef newAdulthoodBackstory;
        public Trait replacedTrait;
        public Trait newTrait;

        //Static Values
        public override Vector2 InitialSize => new Vector2(898f, 608f);
        public static readonly float upgradesOffset = 640f;
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


        public Window_CustomizeAndroid(Building_AndroidCreationStation acs)
        {
            this.acs = acs;
            currentPawnKindDef = VREA_DefOf.VREA_AndroidBasic;
            newAndroid = GetNewPawn();
            draggable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            //Detect changes
            if (refreshAndroidPortrait)
            {
                newAndroid.Drawer.renderer.SetAllGraphicsDirty();

                refreshAndroidPortrait = false;
            }

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

            Rect pawnRect = new Rect(inRect);
            pawnRect.width = PawnPortraitSize.x + 16f;
            pawnRect.height = PawnPortraitSize.y + 16f;
            pawnRect = pawnRect.CenteredOnXIn(inRect);
            pawnRect = pawnRect.CenteredOnYIn(inRect);
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
                Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AndroidCustomization".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;

                //Name
                float row = 32f;
                {
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
                }

                //Hair customization
                float finalPawnCustomizationWidthOffset = (pawnRect.x + pawnRect.width + 16f + (inRect.width - upgradesOffset));

                {
                    Rect rowRect = new Rect(pawnRect.x + pawnRect.width + 16f, pawnRect.y, inRect.width - finalPawnCustomizationWidthOffset, 24f);

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
                }

                //Print button
                {
                    Rect rowRect = new Rect(pawnRect.x + pawnRect.width + 16f, pawnRect.y + 32f, inRect.width - finalPawnCustomizationWidthOffset, 32f);
                    Text.Font = GameFont.Medium;
                    if (Widgets.ButtonText(rowRect, "AndroidCustomizationPrint".Translate()))
                    {
                        var extra = acs.GetExtra();
                        extra.generatedAndroid = newAndroid;
                        Close();
                    }
                    Text.Font = GameFont.Small;
                }

                //Race selector (If possible)
                if (AlienRaceCompat.AlienRaceKinds.Count() > 1)
                {
                    Rect rowRect = new Rect(32 + 16f + 256f, row, 256f - 16f, 24f);
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

                //Generate new pawn
                {
                    Rect rowRect = new Rect(32 + 16f + 256f, row, 128f - 8f, 24f);

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

                //Backstories
                row += 26f;
                {
                    Rect rowRect = new Rect(32f, row, 256f - 16f, 24f);

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
                                                                where (backstory.spawnCategories.Any(category => (currentPawnKindDef.backstoryCategories != null && currentPawnKindDef.backstoryCategories.Any(subCategory => subCategory == category))) || backstory.spawnCategories.Contains("ChjAndroid") || backstory.spawnCategories.Contains("ATR_Inorganic") || backstory.spawnCategories.Contains("ATR_Drone") || backstory.spawnCategories.Contains("ATR_GeneralAndroids") || backstory.spawnCategories.Contains("ATR_ViolentAndroids")) && backstory.slot == BackstorySlot.Childhood
                                                                select backstory;
                        FloatMenuUtility.MakeMenu<BackstoryDef>(backstories, backstory => backstory.TitleCapFor(newAndroid.gender), (BackstoryDef backstory) => delegate
                        {
                            newChildhoodBackstory = backstory;
                        });
                    }

                    if (newAndroid.story.Childhood != null)
                        TooltipHandler.TipRegion(rowRect, newAndroid.story.Childhood.FullDescriptionFor(newAndroid));
                }

                {
                    Rect rowRect = new Rect(32 + 16f + 256f, row, 256f - 16f, 24f);

                    Widgets.DrawBox(rowRect);
                    Widgets.DrawHighlightIfMouseover(rowRect);

                    string label = "";

                    if (newAndroid.story.Adulthood != null)
                        label = "AndroidCustomizationSecondIdentity".Translate() + " " + newAndroid.story.Adulthood.TitleCapFor(newAndroid.gender);
                    else
                        label = "AndroidCustomizationSecondIdentity".Translate() + " " + "AndroidNone".Translate();

                    if (Widgets.ButtonText(rowRect, label))
                    {
                        IEnumerable<BackstoryDef> backstories = from backstory in (from backstoryDef in DefDatabase<BackstoryDef>.AllDefs.ToList()
                                                                                   select backstoryDef)
                                                                where (backstory.spawnCategories.Any(category => currentPawnKindDef.backstoryCategories != null && currentPawnKindDef.backstoryCategories.Any(subCategory => subCategory == category)) || backstory.spawnCategories.Contains("ChjAndroid") ) && backstory.slot == BackstorySlot.Adulthood
                                                                select backstory;
                        FloatMenuUtility.MakeMenu<BackstoryDef>(backstories, backstory => backstory.TitleCapFor(newAndroid.gender), (BackstoryDef backstory) => delegate
                        {
                            newAdulthoodBackstory = backstory;
                        });
                    }

                    if (newAndroid.story.Adulthood != null)
                        TooltipHandler.TipRegion(rowRect, newAndroid.story.Adulthood.FullDescriptionFor(newAndroid));
                }

                //Skills

                //Traits
                row += 32f;

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
                outerTraitsRect.height = inRect.height - outerTraitsRect.y;
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

            Text.Anchor = TextAnchor.UpperLeft;
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

            //Filter out traits the race can NEVER get. 
            //AlienComp alienComp = newAndroid.TryGetComp<AlienComp>();
            //if (newAndroid.def is ThingDef_AlienRace alienRaceDef)
            //{
            //    List<RimWorld.TraitDef> disallowedTraits = alienRaceDef?.alienRace?.generalSettings?.disallowedTraits?.Select(trait => trait.entry.def).ToList();

            //    if (disallowedTraits != null)
            //    {
            //        foreach (RimWorld.TraitDef trait in disallowedTraits)
            //        {
            //            allTraits.RemoveAll(thisTrait => trait.defName == thisTrait.def.defName);
            //        }
            //    }
            //}

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

                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, acs.Faction, RimWorld.PawnGenerationContext.NonPlayer,
                -1, true, false, false, false, false, 0f, false, false, true, true, false, false, false, true, fixedGender: gender, fixedBiologicalAge: 20, fixedChronologicalAge: 20));
            

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
