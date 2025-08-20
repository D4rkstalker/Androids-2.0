using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Androids2.VREPatches
{
    using HarmonyLib;
    using System.Linq;
    using Verse;
    using RimWorld;
    using VREAndroids;
    using global::Androids2.Extensions;
    using UnityEngine;
    using global::Androids2.Androids2Content;
    using global::Androids2.Utils;

    namespace Androids2.HarmonyPatches
    {
        [HarmonyPatch(typeof(Building_AndroidCreationStation), "CallAndroidCreationWindow")]
        public static class Patch_CallAndroidCreationWindow
        {
            public static void Postfix(Building_AndroidCreationStation __instance, Pawn creator)
            {
                Debug.LogError("Androids2: Patching CallAndroidCreationWindow to use custom window.");
                Find.WindowStack.Add(new Window_CustomizeAndroid(__instance));
            }
        }
        [HarmonyPatch(typeof(Building_AndroidCreationStation), "FinishAndroidProject")]
        public static class Patch_FinishAndroidProject
        {
            public static bool Prefix(Building_AndroidCreationStation __instance)
            {
                var extra = __instance.GetExtra();

                AndroidMakerPatch.ApplyXenotype(extra.generatedAndroid, __instance.curAndroidProject.genes); 

                __instance.curAndroidProject = null;
                GenSpawn.Spawn(extra.generatedAndroid, __instance.Position, __instance.Map);
                Log.Warning(extra.generatedAndroid.needs.TryGetNeed(VREA_DefOf.VREA_ReactorPower).CurLevelPercentage +"");
                __instance.currentWorkAmountDone = 0;
                __instance.totalWorkAmount = 0;

                __instance.unfinishedAndroid?.Destroy();
                __instance.unfinishedAndroid = null;

                return false; // skip original method
            }
        }
    }
}
