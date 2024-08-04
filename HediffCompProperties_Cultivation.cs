using Verse;
using RimWorld;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_Cultivation : HediffCompProperties
    {
        public FloatRange severityPerTriggerRange;

        public HediffDef currentLevel;

        public HediffDef nextLevel;

        public float TribulationMultiplier = 1;

        public bool shouldGetTribulation;

        public bool guaranteedTrib = false;

        public int tickInterval = 2500;

        public IntRange breakingThroughDuration;

        public int TribulationStrikeInterval = 250;

        public SimpleCurve curves;

        public string uiIcon;

        public List<CultivatorNeedInfo> changeToNeeds;

        public bool requireQiSource = true;
        public HediffCompProperties_Cultivation()
        {
            compClass = typeof(HediffComp_Cultivation);
        }
    }
}
