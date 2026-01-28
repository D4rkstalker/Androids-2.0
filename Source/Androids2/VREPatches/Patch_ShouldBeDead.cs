using HarmonyLib;
using Verse;
using VREAndroids;

namespace Androids2.VREPatches
{
    public class Patch_ShouldBeDead
    {
        static Patch_ShouldBeDead()
        {
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                TryUnpatchVREAndroids();
            });

        }
        private static void TryUnpatchVREAndroids()
        {
            var original = AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDead));

            Helpers.Unpatch(original, "VREAndroids.Pawn_HealthTracker_ShouldBeDead_Patch");

        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
    public static class Pawn_HealthTracker_ShouldBeDead_Patch
    {
        [HarmonyPriority(int.MinValue)]
        public static void Postfix(ref bool __result, Pawn_HealthTracker __instance, Pawn ___pawn)
        {
            if (PawnCapacityUtility.CalculatePartEfficiency(__instance.hediffSet, ___pawn.RaceProps.body.corePart) <= 0.0001f)
            {
                __result = true;
                return;
            }
            if (__result && ___pawn.IsAndroid())
            {
                if (___pawn.health.hediffSet.GetBrain() != null)
                {
                    __result = false;
                }

            }
        }
    }

}
