
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace Androids2.Utils
{
    /// <summary>
    /// Helps in dealing with races.
    /// </summary>
    public static class AlienRaceCompat
    {
        private static List<PawnKindDef> alienRaceKindsint = new List<PawnKindDef>();
        private static bool alienRaceKindSearchDoneint = false;
        private static bool alienRacesFoundint = false;

        public static bool HasAlienRace()
        {
            return LoadedModManager.RunningModsListForReading
                .Any(m => m.Name == "Humanoid Alien Races" || m.PackageId == "erdelf.humanoidalienraces");
        }

        public static bool AlienRacesExist => alienRacesFoundint;

        public static IEnumerable<PawnKindDef> AlienRaceKinds
        {
            get
            {
                if (!alienRaceKindSearchDoneint)
                {
                    if (!HasAlienRace())
                    {
                        Log.Warning("[Androids2] Humanoid Alien Races mod not found; skipping AlienRaceKinds.");
                        alienRaceKindSearchDoneint = true;
                        return alienRaceKindsint;
                    }

                    // Dynamically load ThingDef_AlienRace to avoid hard dependency
                    var alienRaceType = AccessTools.TypeByName("AlienRace.ThingDef_AlienRace");
                    if (alienRaceType == null)
                    {
                        Log.Warning("[Androids2] Could not find AlienRace.ThingDef_AlienRace type.");
                        alienRaceKindSearchDoneint = true;
                        return alienRaceKindsint;
                    }

                    var allDefs = GenDefDatabaseDefs(alienRaceType);
                    if (allDefs == null)
                    {
                        Log.Warning("[Androids2] Failed to get AlienRace defs.");
                        alienRaceKindSearchDoneint = true;
                        return alienRaceKindsint;
                    }

                    foreach (var alienDef in allDefs)
                    {
                        var raceDef = alienDef as ThingDef;
                        if (raceDef == null) continue;

                        PawnKindDef bestKindDef = DefDatabase<PawnKindDef>.AllDefs
                            .FirstOrDefault(def => def.race == raceDef);

                        if (bestKindDef != null)
                        {
                            alienRaceKindsint.Add(bestKindDef);
                        }
                    }

                    alienRaceKindsint.RemoveAll(def => def.race.defName == "Human");

                    if (alienRaceKindsint.Count > 1)
                        alienRacesFoundint = true;

                    alienRaceKindSearchDoneint = true;
                }

                return alienRaceKindsint;
            }
        }

        // Helper: dynamically get DefDatabase<T>.AllDefs for unknown type
        private static IEnumerable<object> GenDefDatabaseDefs(System.Type defType)
        {
            var dbType = typeof(DefDatabase<>).MakeGenericType(defType);
            var prop = dbType.GetProperty("AllDefs", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return prop?.GetValue(null) as IEnumerable<object>;
        }
    }
}
