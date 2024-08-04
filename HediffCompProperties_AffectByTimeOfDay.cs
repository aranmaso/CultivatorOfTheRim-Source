using RimWorld;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_AffectByTimeOfDay : HediffCompProperties
    {
        public bool isNightOnly;

        public bool isDayOnly;

        public HediffCompProperties_AffectByTimeOfDay()
        {
            compClass = typeof(HediffComp_AffectByTimeOfDay);
        }
    }
}
