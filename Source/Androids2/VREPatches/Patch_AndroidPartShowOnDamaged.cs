//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;

//namespace Androids2.VREPatches
//{
//    public class Patch_AndroidPartShowOnDamaged
//    {
//        [HarmonyPatch(typeof(VREAndroids.Hediff_AndroidPart))]
//        [HarmonyPatch(nameof(VREAndroids.Hediff_AndroidPart.PostAdd))]
//        [HarmonyPatch(new[] { typeof(DamageInfo?) })] // match the nullable parameter exactly
//        public static class Patch_Hediff_AndroidPart_PostAdd
//        {
//            // Runs after the original PostAdd
//            public static void Postfix(VREAndroids.Hediff_AndroidPart __instance, DamageInfo? dinfo)
//            {
//                if(__instance.part)
//            }
//        }
//    }
//}
