//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;
//using RimWorld;
//using VREAndroids;
//using Androids2.VREPatches;
//using System.Runtime.CompilerServices;

//namespace Androids2.Extensions
//{
//    public static class AndroidCreationStationExtensions
//    {
//        public class ExtraFields
//        {
//            public Pawn generatedAndroid;
//        }

//        private static ConditionalWeakTable<Building_AndroidCreationStation, ExtraFields> data =
//            new ConditionalWeakTable<Building_AndroidCreationStation, ExtraFields>();
//        public static ExtraFields GetExtra(this Building_AndroidCreationStation station)
//        {
//            return data.GetOrCreateValue(station);
//        }
//    }
//}
