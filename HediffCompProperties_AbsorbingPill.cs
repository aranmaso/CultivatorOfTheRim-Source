using RimWorld;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_AbsorbingPill : HediffCompProperties
    {
        public FloatRange severityAmount;

        public int tickInterval;

        public HediffCompProperties_AbsorbingPill()
        {
            compClass = typeof(HediffComp_AbsorbingPill);
        }
    }
}
