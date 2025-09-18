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

namespace Androids2.Androids2Content
{
    public class Window_CustomizeAndroid : Window_CreateAndroidBase
    {
        public Building_AndroidCreationStation station;
        public Pawn creator;

        public Window_CustomizeAndroid(Building_AndroidCreationStation station, Pawn creator, Action callback) : base(callback)
        {
            this.station = station;
            this.creator = creator;
            selectedGenes = VREAndroids.Utils.AndroidGenesGenesInOrder.Where(x => x.CanBeRemovedFromAndroid() is false).ToList();
            OnGenesChanged();

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
            DrawSection_Filtered(rect, VREAndroids.Utils.AndroidGenesGenesInOrder.Where(GeneValidator).ToList(), null, ref curY, ref unselectedHeight, adding: true, containingRect, ref collapsed);
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
        public void DrawSection_Filtered(Rect rect, List<GeneDef> genes, string label, ref float curY, ref float sectionHeight, bool adding, Rect containingRect, ref bool? collapsed)
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
                    if ((adding && quickSearchWidget.filter.Active && (!matchingGenes.Contains(geneDef) || selectedGenes.Contains(geneDef)) && !matchingCategories.Contains(geneDef.displayCategory)) || geneDef.displayCategory == A2_Defof.A2_Hardware)
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
                    if (base.DrawGene(geneDef, !adding, ref curX, curY, num2, containingRect, flag3))
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
                            xenotypeName = base.GetAndroidTypeName();
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




        public override string Header => "VREA.CreateAndroid".Translate();
        public override string AcceptButtonLabel => "VREA.CreateAndroid".Translate();
        public override void AcceptInner()
        {
            CustomXenotype customXenotype = new CustomXenotype();
            customXenotype.name = xenotypeName?.Trim();
            customXenotype.genes.AddRange(selectedGenes);
            customXenotype.inheritable = false;
            customXenotype.iconDef = iconDef;
            station.curAndroidProject = customXenotype;
            station.totalWorkAmount = selectedGenes.Sum(x => x.biostatCpx * 2000);
            station.currentWorkAmountDone = 0;
            station.requiredItems = requiredItems;
            if (creator != null)
            {
                var workgiver = new WorkGiver_CreateAndroid();
                var job = workgiver.JobOnThing(creator, station);
                if (job != null)
                {
                    creator.jobs.TryTakeOrderedJob(job);
                }
            }
        }
        public override TaggedString AndroidName()
        {
            return "VREA.AndroidtypeName".Translate();
        }
        public override void DrawSearchRect(Rect rect)
        {
            base.DrawSearchRect(rect);
            if (Widgets.ButtonText(new Rect(rect.xMax - ButSize.x, rect.y, ButSize.x, ButSize.y), "VREA.SaveAndroidtype".Translate()))
            {
                CustomXenotype customXenotype = new CustomXenotype();
                customXenotype.name = xenotypeName?.Trim();
                customXenotype.genes.AddRange(selectedGenes);
                customXenotype.inheritable = false;
                customXenotype.iconDef = iconDef;
                Find.WindowStack.Add(new Dialog_AndroidProjectList_Save(customXenotype));
            }
            if (Widgets.ButtonText(new Rect(rect.xMax - ButSize.x * 2f - 4f, rect.y, ButSize.x, ButSize.y), "VREA.LoadAndroidtype".Translate()))
            {
                Find.WindowStack.Add(new Dialog_AndroidProjectList_Load(delegate (CustomXenotype xenotype)
                {
                    xenotypeName = xenotype.name;
                    xenotypeNameLocked = true;
                    selectedGenes.Clear();
                    selectedGenes = VREAndroids.Utils.AndroidGenesGenesInOrder.Where(x => x.CanBeRemovedFromAndroid() is false).ToList();
                    selectedGenes.AddRange(xenotype.genes);
                    selectedGenes = selectedGenes.Distinct().ToList();
                    iconDef = xenotype.IconDef;
                    OnGenesChanged();
                }));
            }
        }

        public override void OnGenesChanged()
        {
            base.OnGenesChanged();
            requiredItems = new List<ThingDefCount>
            {
                new ThingDefCount(VREA_DefOf.VREA_PersonaSubcore, 1),
                new ThingDefCount(ThingDefOf.Plasteel, 125),
                new ThingDefCount(ThingDefOf.Uranium, 30),
                new ThingDefCount(ThingDefOf.ComponentSpacer, 7),
            };
        }

    }
}
