using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VREAndroids;

namespace Androids2
{
    public class A2GeneDef : AndroidGeneDef
    {
        public bool removable;
        public List<ThingOrderRequest> costList = new List<ThingOrderRequest>();
        public int timeCost = 0;
        public float nutrition = 0;
    }
}
