using Androids2.Androids2Content;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2.VREPatches
{
    [HotSwappable]

    public class Patch_AdvancedAndroidCreation : Window_AndroidCreation
    {
        public Patch_AdvancedAndroidCreation(Building_AndroidCreationStation station, Pawn creator, Action callback) : base(station, creator, callback)
        {
        }

        public override void DrawSearchRect(Rect rect)
        {
            base.DrawSearchRect(rect);
            if (Widgets.ButtonText(new Rect(rect.xMax - ButSize.x * 2f - 4f, rect.y, ButSize.x, ButSize.y), "androids2.CustomizeAndroid".Translate()))
            {
                Find.WindowStack.Add(new Window_CustomizeAndroid(station));
            }

        }
    }
}
