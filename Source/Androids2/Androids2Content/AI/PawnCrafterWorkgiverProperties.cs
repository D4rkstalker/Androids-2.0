using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Androids2
{
    public class PawnCrafterWorkgiverProperties : DefModExtension
    {
        /// <summary>
        /// ThingDef to scan for.
        /// </summary>
        public ThingDef defToScan;

        /// <summary>
        /// Fill Job to give.
        /// </summary>
        public JobDef fillJob;
    }
}
