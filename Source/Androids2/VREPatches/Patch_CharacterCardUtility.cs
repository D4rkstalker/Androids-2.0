using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2
{
    [StaticConstructorOnStartup]
    public class Patch_CharacterCardUtility
    {
        static Patch_CharacterCardUtility()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                UnpatchSpecificPrefix();
            });
        }

        public static MethodBase ResolveDrawerLambda()
        {
            foreach (var type in typeof(CharacterCardUtility).GetNestedTypes(AccessTools.all))
            {
                foreach (var method in type.GetMethods(AccessTools.all))
                {
                    if (method.Name.Contains("<DoLeftSection>") && method.GetParameters().Length == 1
                        && method.GetParameters()[0].ParameterType == typeof(Rect))
                    {
                        return method;
                    }
                }
            }
            return null;
        }

        public static void UnpatchSpecificPrefix()
        {
            var target = ResolveDrawerLambda();
            if (target == null)
            {
                Log.Warning("[your mod] Could not find DoLeftSection drawer lambda to unpatch.");
                return;
            }

            Log.Warning("Unpatching backstories");
            Androids2.harmony.Unpatch(target, HarmonyPatchType.Prefix, "VREAndroidsMod");


        }

    }

    [HarmonyPatch]
    public static class A2CharacterCardUtility_DoLeftSection_Patch
    {
        public static MethodBase TargetMethod()
        {
            foreach (var type in typeof(CharacterCardUtility).GetNestedTypes(AccessTools.all))
            {
                foreach (var method in type.GetMethods(AccessTools.all))
                {
                    if (method.Name.Contains("<DoLeftSection>") && method.GetParameters().Length == 1
                        && method.GetParameters()[0].ParameterType == typeof(Rect))
                    {
                        return method;
                    }
                }
            }
            return null;
        }
        [HarmonyPriority(int.MaxValue)]
        public static bool Prefix(Pawn ___pawn, Rect ___leftRect, Rect sectionRect)
        {
            if (___pawn.IsAndroid() && !___pawn.IsDroid())
            {
                float num8 = sectionRect.y;
                Text.Font = GameFont.Small;
                foreach (BackstorySlot value6 in Enum.GetValues(typeof(BackstorySlot)))
                {
                    BackstoryDef backstory = ___pawn.story.GetBackstory(value6);
                    if (backstory != null)
                    {
                        Rect rect7 = new Rect(sectionRect.x, num8, ___leftRect.width, 22f);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(rect7, (value6 == BackstorySlot.Adulthood) ? "Baseline".Translate() : "Model".Translate());
                        Text.Anchor = TextAnchor.UpperLeft;
                        string text = backstory.TitleCapFor(___pawn.gender);
                        Rect rect8 = new Rect(rect7);
                        rect8.x += 90f;
                        rect8.width = Text.CalcSize(text).x + 10f;
                        Color color4 = GUI.color;
                        GUI.color = CharacterCardUtility.StackElementBackground;
                        GUI.DrawTexture(rect8, BaseContent.WhiteTex);
                        GUI.color = color4;
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Widgets.Label(rect8, text.Truncate(rect8.width));
                        Text.Anchor = TextAnchor.UpperLeft;
                        if (Mouse.IsOver(rect8))
                        {
                            Widgets.DrawHighlight(rect8);
                        }

                        if (Mouse.IsOver(rect8))
                        {
                            TooltipHandler.TipRegion(rect8, backstory.FullDescriptionFor(___pawn).Resolve());
                        }

                        num8 += rect7.height + 4f;
                    }
                }

                if (___pawn.story != null && ___pawn.story.title != null)
                {
                    Rect rect9 = new Rect(sectionRect.x, num8, ___leftRect.width, 22f);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(rect9, "BackstoryTitle".Translate() + ":");
                    Text.Anchor = TextAnchor.UpperLeft;
                    Rect rect10 = new Rect(rect9);
                    rect10.x += 90f;
                    rect10.width -= 90f;
                    Widgets.Label(rect10, ___pawn.story.title);
                    num8 += rect9.height;
                }
                return false;
            }
            return true;
        }
    }

}
