using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2.VREPatches
{
    public class Patch_GeneUIUtility_DrawGenesInfo_Patch
    {
        Patch_GeneUIUtility_DrawGenesInfo_Patch()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                var original = AccessTools.Method(typeof(GeneUIUtility), nameof(GeneUIUtility.DrawGenesInfo));

                Helpers.Unpatch(original, "VREAndroids.GeneUIUtility_DrawGenesInfo_Patch");
            });



        }

        [HotSwappable]
        [HarmonyPatch(typeof(GeneUIUtility), "DrawGenesInfo")]
        public static class GeneUIUtility_DrawGenesInfo_Patch
        {
            [HarmonyPriority(int.MaxValue)]
            public static bool Prefix(Rect rect, Thing target, float initialHeight, ref Vector2 size, ref Vector2 scrollPosition, GeneSet pregnancyGenes = null)
            {
                if (target is Pawn pawn && pawn.IsAndroid())
                {
                    DrawComponentsInfo(rect, target, initialHeight, ref size, ref scrollPosition, pregnancyGenes);
                    return false;
                }
                return true;
            }

            public static void DrawComponentsInfo(Rect rect, Thing target, float initialHeight, ref Vector2 size, ref Vector2 scrollPosition, GeneSet pregnancyGenes = null)
            {
                Rect rect2 = rect;
                Rect position = rect2.ContractedBy(10f);
                if (Prefs.DevMode)
                {
                    GeneUIUtility.DoDebugButton(new Rect(rect2.xMax - 18f - 125f, 5f, 115f, Text.LineHeight), target, pregnancyGenes);
                }
                GUI.BeginGroup(position);
                float num = AndroidStatsTable.HeightForBiostats(null);
                Rect rect3 = new Rect(0f, 0f, position.width, position.height - num - 12f);
                DrawGeneSections(rect3, target, pregnancyGenes, ref scrollPosition);
                Rect rect4 = new Rect(0f, rect3.yMax + 6f, position.width - 140f - 4f, num);
                rect4.yMax = rect3.yMax + num + 6f;
                if (!(target is Pawn))
                {
                    rect4.width = position.width;
                }
                AndroidStatsTable.Draw(rect4, GeneUIUtility.gcx, GeneUIUtility.met, null);
                TryDrawXenotype(target, rect4.xMax + 4f, rect4.y + Text.LineHeight / 2f);
                if (Event.current.type == EventType.Layout)
                {
                    float num2 = GeneUIUtility.endogenesHeight + GeneUIUtility.xenogenesHeight + num + 12f + 70f;
                    if (num2 > initialHeight)
                    {
                        size.y = Mathf.Min(num2, (float)(UI.screenHeight - 35) - 165f - 30f);
                    }
                    else
                    {
                        size.y = initialHeight;
                    }
                    GeneUIUtility.xenogenesHeight = 0f;
                    GeneUIUtility.endogenesHeight = 0f;
                }
                GUI.EndGroup();
            }

            private static void DrawGeneSections(Rect rect, Thing target, GeneSet genesOverride, ref Vector2 scrollPosition)
            {
                RecacheGenes(target, genesOverride);
                GUI.BeginGroup(rect);
                Rect rect2 = new Rect(0f, 0f, rect.width - 16f, GeneUIUtility.scrollHeight);
                float curY = 0f;
                Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, rect2);
                Rect containingRect = rect2;
                containingRect.y = scrollPosition.y;
                containingRect.height = rect.height;
                DrawSection(rect, "VREA.Subroutines", GeneUIUtility.xenogenes.Count, ref curY, ref GeneUIUtility.xenogenesHeight,
                    delegate (int i, Rect r)
                    {
                        GeneUIUtility.DrawGene(GeneUIUtility.xenogenes[i], r, GeneType.Xenogene);
                    }, containingRect);

                if (GeneUIUtility.endogenes.Any())
                {
                    DrawSection(rect, "VREA.HardwareComponents", GeneUIUtility.endogenes.Count, ref curY, ref GeneUIUtility.endogenesHeight,
                        delegate (int i, Rect r)
                        {
                            GeneUIUtility.DrawGene(GeneUIUtility.endogenes[i], r, GeneType.Endogene);
                        }, containingRect);
                    curY += 12f;
                }
                DrawSection(rect, "A2.Hardware", GeneUIUtility.endogenes.Count, ref curY, ref GeneUIUtility.endogenesHeight,
                delegate (int i, Rect r)
                {
                    GeneUIUtility.DrawGene(GeneUIUtility.endogenes[i], r, GeneType.Endogene);
                }, containingRect);
                            curY += 12f;

                if (Event.current.type == EventType.Layout)
                {
                    GeneUIUtility.scrollHeight = curY;
                }
                Widgets.EndScrollView();
                GUI.EndGroup();
            }

            private static void RecacheGenes(Thing target, GeneSet genesOverride)
            {
                GeneUIUtility.geneDefs.Clear();
                GeneUIUtility.xenogenes.Clear();
                GeneUIUtility.endogenes.Clear();
                GeneUIUtility.gcx = 0;
                GeneUIUtility.met = 0;
                GeneUIUtility.arc = 0;
                Pawn pawn = target as Pawn;
                GeneSet geneSet = (target as GeneSetHolderBase)?.GeneSet ?? genesOverride;
                if (pawn != null)
                {
                    foreach (Gene xenogene in pawn.genes.GenesListForReading.Where(x => x.def.IsHardware()))
                    {
                        if (!xenogene.Overridden)
                        {
                            AddBiostats(xenogene.def);
                        }
                        GeneUIUtility.xenogenes.Add(xenogene);
                    }
                    foreach (Gene endogene in pawn.genes.GenesListForReading.Where(x => x.def.IsSubroutine()))
                    {
                        if (endogene.def.endogeneCategory != EndogeneCategory.Melanin
                            || !pawn.genes.Endogenes.Any((Gene x) => x.def.skinColorOverride.HasValue))
                        {
                            if (!endogene.Overridden)
                            {
                                AddBiostats(endogene.def);
                            }
                            GeneUIUtility.endogenes.Add(endogene);
                        }
                    }
                    GeneUIUtility.xenogenes.SortGenes();
                    GeneUIUtility.endogenes.SortGenes();
                }
                else
                {
                    if (geneSet == null)
                    {
                        return;
                    }
                    foreach (GeneDef item in geneSet.GenesListForReading)
                    {
                        GeneUIUtility.geneDefs.Add(item);
                    }
                    GeneUIUtility.gcx = geneSet.ComplexityTotal;
                    GeneUIUtility.met = geneSet.MetabolismTotal;
                    GeneUIUtility.arc = geneSet.ArchitesTotal;
                    GeneUIUtility.geneDefs.SortGeneDefs();
                }
                static void AddBiostats(GeneDef gene)
                {
                    GeneUIUtility.gcx += gene.biostatCpx;
                    GeneUIUtility.met += gene.biostatMet;
                    GeneUIUtility.arc += gene.biostatArc;
                }
            }

            private static void DrawSection(Rect rect, string section, int count, ref float curY, ref float sectionHeight, Action<int, Rect> drawer, Rect containingRect)
            {
                Widgets.Label(10f, ref curY, rect.width, section.Translate().CapitalizeFirst(),
                    section.Translate());
                float num = curY;
                Rect rect2 = new Rect(rect.x, curY, rect.width, sectionHeight);

                Widgets.DrawMenuSection(rect2);
                float num2 = (rect.width - 12f - 630f - 36f) / 2f;
                curY += num2;
                int num3 = 0;
                int num4 = 0;
                for (int i = 0; i < count; i++)
                {
                    if (num4 >= 6)
                    {
                        num4 = 0;
                        num3++;
                    }
                    else if (i > 0)
                    {
                        num4++;
                    }
                    Rect rect3 = new Rect(num2 + (float)num4 * 90f + (float)num4 * 6f, curY + (float)num3 * 90f + (float)num3 * 6f, 90f, 90f);
                    if (containingRect.Overlaps(rect3))
                    {
                        drawer(i, rect3);
                    }
                }
                curY += (float)(num3 + 1) * 90f + (float)num3 * 6f + num2;

                if (Event.current.type == EventType.Layout)
                {
                    sectionHeight = curY - num;
                }
            }
            private static void TryDrawXenotype(Thing target, float x, float y)
            {
                Pawn sourcePawn = target as Pawn;
                if (sourcePawn == null)
                {
                    return;
                }
                Rect rect = new Rect(x, y, 140f, Text.LineHeight);
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect, sourcePawn.genes.XenotypeLabelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                Rect position = new Rect(rect.center.x - 17f, rect.yMax + 4f, 34f, 34f);
                GUI.color = XenotypeDef.IconColor;
                GUI.DrawTexture(position, sourcePawn.genes.XenotypeIcon);
                GUI.color = Color.white;
                rect.yMax = position.yMax;
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                    TooltipHandler.TipRegion(rect, () => ("VREA.Android".Translate() + ": " + sourcePawn.genes.XenotypeLabelCap).Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + sourcePawn.genes.XenotypeDescShort, 883938493);
                }
                if (Widgets.ButtonInvisible(rect) && !sourcePawn.genes.UniqueXenotype)
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(sourcePawn.genes.Xenotype));
                }
            }
        }

    }
}
