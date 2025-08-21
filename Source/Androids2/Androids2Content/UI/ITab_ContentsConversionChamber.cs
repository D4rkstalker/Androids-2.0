using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Androids2
{
    // Token: 0x02000007 RID: 7
    public class ITab_ContentsConversionChamber : ITab_ContentsBase
    {
        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000016 RID: 22 RVA: 0x00003338 File Offset: 0x00001538
        public override IList<Thing> container
        {
            get
            {
                Building_ConversionChamber building_ConversionChamber = base.SelThing as Building_ConversionChamber;
                this.listInt.Clear();
                if (building_ConversionChamber != null && building_ConversionChamber.ContainedThing != null)
                {
                    this.listInt.Add(building_ConversionChamber.ContainedThing);
                }
                return this.listInt;
            }
        }

        // Token: 0x06000017 RID: 23 RVA: 0x0000337E File Offset: 0x0000157E
        public ITab_ContentsConversionChamber()
        {
            this.labelKey = "TabConversionChamberContents";
            this.containedItemsKey = "ContainedItems";
            this.canRemoveThings = false;
        }

        // Token: 0x0400002F RID: 47
        private List<Thing> listInt = new List<Thing>();
    }
}
