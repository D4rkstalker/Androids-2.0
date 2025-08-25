using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VREAndroids;

namespace Androids2
{
    [StaticConstructorOnStartup]
    public class Gizmo_StartConvert : Command
    {
        // Token: 0x06000010 RID: 16 RVA: 0x00003178 File Offset: 0x00001378
        public Gizmo_StartConvert(Building_Converter _converter)
        {
            converter = _converter;
            if (converter.currentPawn.IsAndroid())
            {
                defaultLabel = Translator.Translate(labelInitMod);
                defaultDesc = Translator.Translate(descriptionInitMod);
            }
            else
            {
                defaultLabel = Translator.Translate(labelInitConvert);
                defaultDesc = Translator.Translate(descriptionInitConvert);
            }
            icon = Gizmo_StartConvert.initIcon;
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00003230 File Offset: 0x00001430
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            converter.InitiatePawnModing();
        }

        // Token: 0x04000023 RID: 35
        public Building_Converter converter;

        // Token: 0x04000024 RID: 36
        public static Texture2D initIcon = ContentFinder<Texture2D>.Get("Icons/Widgets/vitruvian-man", true);

        // Token: 0x04000025 RID: 37
        public string labelInitMod = "AndroidGizmoInitModLabel";

        // Token: 0x04000026 RID: 38
        public string labelInitConvert = "AndroidGizmoInitConvertLabel";

        // Token: 0x04000027 RID: 39
        public string descriptionInitMod = "AndroidGizmoInitModDescription";

        // Token: 0x04000028 RID: 40
        public string descriptionInitConvert = "AndroidGizmoInitConvertDescription";
    }
}
