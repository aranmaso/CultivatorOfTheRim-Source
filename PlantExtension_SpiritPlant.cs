using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CultivatorOfTheRim
{
    public class PlantExtension_SpiritPlant : DefModExtension
    {
        public List<string> allowedTags;

        public List<string> allowedSpecialTags;

        public string allowedCultivation;

        public List<ThingDef> excludedThing;

        public float radius = 4f;

        public bool ignoreTemp;

        public bool isNeedResting;

        //plant that require source
        public string sourceName = "Qi Source";

        public string specialSourceName = "Special Source";

        public bool requireSource = false;

        public bool requireSpecialSource = false;

        public bool healSelf = true;

        public bool consumeSource = false;

        public bool destroyOnConsume = false;

        public int sourceDamagePerTrigger = 1;
        

        //speed up plant grow
        public bool speedUpNearbyPlant = false;

        public List<ThingDef> disallowedDef;

        public SoundDef soundDefOnSpeedUp;

        public bool onlyAffectSpiritPlant;

        public bool onlyAffectCommonPlant;

        public bool onlyWorkIfNoPlantOfSameTypeInRange;

        public FloatRange growthBoost;

        //night plant
        public bool isNightPlant;

        public bool onlyHarvestableAtNight;

        public bool onlyHarvestableAtDay;

        //Day/Night Harvest
        public bool differentYieldBasedOnTimeOfHarvest = false;

        public ThingDef dayTimeYield;

        public ThingDef nightTimeYield;

        //TimeOfMonth Harvest
        public bool differentEffectBasedOnTimeOfMonth = false;

        public bool differentGrowthRate = false;

        public SimpleCurve growthRateCurves;

        public bool endOfMonthHarvestable;

        public bool middleOfMonthHarvestable;

        //Season Effect
        public bool isAffectBySeason = false;

        public SimpleCurve seasonCurves;

        public List<Season> specificMonthHarvestable;

        //Effect if pawn get close
        public bool isAffectByPawnApproaching;

        //Only one in range
        public bool isStopGrowingIfThingInRange;

        public float forbiddenRange;

        public List<ThingDef> forbiddenThings;

        public List<string> forbiddenTags;
    }
}
