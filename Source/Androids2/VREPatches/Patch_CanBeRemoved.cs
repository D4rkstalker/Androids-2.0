using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VREAndroids;

namespace Androids2.VREPatches
{
    [HarmonyPatch(typeof(VREAndroids.Utils), "CanBeRemovedFromAndroid")]
    public static class Utils_CanBeRemovedFromAndroid_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(GeneDef geneDef, ref bool __result)
        {
            if(__result && geneDef is A2GeneDef a2g)
            {
                __result = a2g.moddable;
            }
        }
    }
}
