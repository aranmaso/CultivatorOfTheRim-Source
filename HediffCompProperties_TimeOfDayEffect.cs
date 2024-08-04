using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_TimeOfDayEffect : HediffCompProperties
    {
        public float severityAtNight;

        public float severityAtDay;

        public HediffCompProperties_TimeOfDayEffect()
        {
            compClass = typeof(HediffComp_TimeOfDayEffect);
        }
    }
}
