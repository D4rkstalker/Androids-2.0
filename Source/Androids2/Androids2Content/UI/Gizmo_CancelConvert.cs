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
    public class Gizmo_CancelConvert : Command
    {
        // Token: 0x06000013 RID: 19 RVA: 0x00003258 File Offset: 0x00001458
        public Gizmo_CancelConvert(Building_Converter _converter)
        {
            converter = _converter;
            if (converter.currentPawn.IsAndroid())
            {
                defaultLabel = Translator.Translate(labelAbortMod);
                defaultDesc = Translator.Translate(descriptionAbortMod);
            }
            else
            {
                defaultLabel = Translator.Translate(labelAbortConvert);
                defaultDesc = Translator.Translate(descriptionAbortConvert);
            }
            icon = Gizmo_CancelConvert.initIcon;
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00003310 File Offset: 0x00001510
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            converter.EjectPawn();
        }

        // Token: 0x04000029 RID: 41
        public Building_Converter converter;

        // Token: 0x0400002A RID: 42
        public static Texture2D initIcon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);

        // Token: 0x0400002B RID: 43
        public string labelAbortMod = "AndroidGizmoAbortModLabel";

        // Token: 0x0400002C RID: 44
        public string labelAbortConvert = "AndroidGizmoAbortModLabel";

        // Token: 0x0400002D RID: 45
        public string descriptionAbortMod = "AndroidGizmoAbortModDescription";

        // Token: 0x0400002E RID: 46
        public string descriptionAbortConvert = "AndroidGizmoAbortModDescription";
    }
}
