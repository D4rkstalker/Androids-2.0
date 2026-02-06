//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;
//using VREAndroids;

//namespace Androids2.VREPatches
//{
//    [HarmonyPatch(typeof(WorkGiver_RepairAndroid), nameof(WorkGiver_RepairAndroid.HasJobOn))]
//    public static class Patch_WorkGiver_Repair_HasJobOn
//    {
//        public static void Postfix(Pawn pawn, Thing t, bool forced, ref bool __result)
//        {
//            if (!__result)
//            {
//                return;
//            }
//            __result = !Helpers.CanNaturallyRepair((Pawn)t);
//        }
//    }
//}
