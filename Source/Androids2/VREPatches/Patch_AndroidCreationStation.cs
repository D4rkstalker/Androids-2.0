using HarmonyLib;
using System.Linq;
using Verse;
using RimWorld;
using VREAndroids;
using UnityEngine;
using Androids2.Androids2Content;
using Androids2.Utils;


namespace Androids2.VREPatches
{
    [HarmonyPatch(typeof(Building_AndroidCreationStation), "CallAndroidCreationWindow")]
    public static class Patch_CallAndroidCreationWindow
    {
        public static bool Prefix(Building_AndroidCreationStation __instance, Pawn creator)
        {
            //Debug.LogError("Androids2: Patching CallAndroidCreationWindow to use custom window.");
            Find.WindowStack.Add(new Window_CustomizeAndroid(__instance, creator, null));
            return false;
        }
    }
    //[HarmonyPatch(typeof(Building_AndroidCreationStation), "FinishAndroidProject")]
    //public static class Patch_FinishAndroidProject
    //{
    //    public static bool Prefix(Building_AndroidCreationStation __instance)
    //    {
    //        var newPawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(VREA_DefOf.VREA_AndroidBasic, Faction.OfPlayer,
    //            allowDowned: true, allowAddictions: false));
    //        AndroidMakerPatch.ApplyXenotype(newPawn, __instance.curAndroidProject.genes);

    //        __instance.curAndroidProject = null;
    //        //if(extra == null)
    //        //{
    //        //    Log.Warning("extra is null");
    //        //}
    //        //Log.Warning(extra.generatedAndroid.ToString() + "");
    //        //Log.Warning(extra.generatedAndroid.needs + "");
    //        //Log.Warning(extra.generatedAndroid.needs.TryGetNeed(VREA_DefOf.VREA_ReactorPower).CurLevelPercentage + "");
    //        GenSpawn.Spawn(newPawn, __instance.Position, __instance.Map);
    //        __instance.currentWorkAmountDone = 0;
    //        __instance.totalWorkAmount = 0;

    //        __instance.unfinishedAndroid?.Destroy();
    //        __instance.unfinishedAndroid = null;

    //        return false; // skip original method
    //    }
    //}

}
