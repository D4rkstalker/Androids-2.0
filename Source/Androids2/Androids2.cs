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

        public static Harmony harmony;

        public Androids2(ModContentPack content) : base(content)
        {
            harmony = new Harmony("firedragonmc.androids2");
            harmony.PatchAll();
            _instance = this;
        }

    }
}
