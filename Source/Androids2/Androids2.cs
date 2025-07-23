using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Androids2
{
    public class Androids2 : Mod
    {
        private static Androids2 _instance;
        public static Androids2 Instance => _instance;

        public Androids2(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("firedragonmc.androids2");
            harmony.PatchAll();

            _instance = this;
        }

    }
}
