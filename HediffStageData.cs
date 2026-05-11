using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffStageData : IExposable
    {
        public string label;

        public float vomitMtbDays = -1f;

        public float deathMtbDays = -1f;

        public bool mtbDeathDestroysBrain;

        public float painFactor = 1f;

        public float painOffset;

        public float totalBleedFactor = 1f;

        public float naturalHealingFactor = -1f;

        public float regeneration = -1f;

        public bool showRegenerationStat = true;

        public float forgetMemoryThoughtMtbDays = -1f;

        public float pctConditionalThoughtsNullified;

        public float pctAllThoughtNullification;

        public float opinionOfOthersFactor = 1f;

        public float fertilityFactor = 1f;

        public float hungerRateFactor = 1f;

        public float hungerRateFactorOffset;

        public float restFallFactor = 1f;

        public float restFallFactorOffset;

        public float socialFightChanceFactor = 1f;

        public float foodPoisoningChanceFactor = 1f;

        public float mentalBreakMtbDays = -1f;

        public string mentalBreakExplanation;

        public bool blocksMentalBreaks;

        public bool blocksInspirations;

        public float overrideMoodBase = -1f;

        public float severityGainFactor = 1f;

        public List<HediffDef> makeImmuneTo;

        public Dictionary<StatDef, float> statOffsets = new Dictionary<StatDef, float>();

        public Dictionary<StatDef, float> statFactors = new Dictionary<StatDef, float>();

        public Dictionary<PawnCapacityDef, float> capModsFactor = new Dictionary<PawnCapacityDef, float>();

        public Dictionary<PawnCapacityDef, float> capModsOffset = new Dictionary<PawnCapacityDef, float>();

        public bool multiplyStatChangesBySeverity;

        public StatDef statOffsetEffectMultiplier;

        public StatDef statFactorEffectMultiplier;

        public StatDef capacityFactorEffectMultiplier;

        public WorkTags disabledWorkTags;

        public float partEfficiencyOffset;

        public void ExposeData()
        {
            Scribe_Values.Look(ref label, "label");
            Scribe_Values.Look(ref vomitMtbDays, "vomitMtbDays");
            Scribe_Values.Look(ref deathMtbDays, "deathMtbDays");
            Scribe_Values.Look(ref mtbDeathDestroysBrain, "mtbDeathDestroysBrain");
            Scribe_Values.Look(ref painFactor, "painFactor");
            Scribe_Values.Look(ref painOffset, "painOffset");
            Scribe_Values.Look(ref totalBleedFactor, "totalBleedFactor");
            Scribe_Values.Look(ref naturalHealingFactor, "naturalHealingFactor");
            Scribe_Values.Look(ref showRegenerationStat, "showRegenerationStat");
            Scribe_Values.Look(ref forgetMemoryThoughtMtbDays, "forgetMemoryThoughtMtbDays");
            Scribe_Values.Look(ref pctConditionalThoughtsNullified, "pctConditionalThoughtsNullified");
            Scribe_Values.Look(ref pctAllThoughtNullification, "pctAllThoughtNullification");
            Scribe_Values.Look(ref opinionOfOthersFactor, "opinionOfOthersFactor");
            Scribe_Values.Look(ref fertilityFactor, "fertilityFactor");
            Scribe_Values.Look(ref hungerRateFactor, "hungerRateFactor");
            Scribe_Values.Look(ref hungerRateFactorOffset, "hungerRateFactorOffset");
            Scribe_Values.Look(ref restFallFactor, "restFallFactor");
            Scribe_Values.Look(ref restFallFactorOffset, "restFallFactorOffset");
            Scribe_Values.Look(ref socialFightChanceFactor, "socialFightChanceFactor");
            Scribe_Values.Look(ref foodPoisoningChanceFactor, "foodPoisoningChanceFactor");
            Scribe_Values.Look(ref mentalBreakMtbDays, "mentalBreakMtbDays");
            Scribe_Values.Look(ref mentalBreakExplanation, "mentalBreakExplanation");
            Scribe_Values.Look(ref blocksMentalBreaks, "blocksMentalBreaks");
            Scribe_Values.Look(ref blocksInspirations, "blocksInspirations");
            Scribe_Values.Look(ref overrideMoodBase, "overrideMoodBase");
            Scribe_Values.Look(ref severityGainFactor, "severityGainFactor");
            Scribe_Collections.Look(ref makeImmuneTo, "makeImmuneTo", LookMode.Def);
            Scribe_Collections.Look(ref statOffsets, "statOffsets",LookMode.Def,LookMode.Value);
            Scribe_Collections.Look(ref statFactors, "statFactors", LookMode.Def,LookMode.Value);
            Scribe_Collections.Look(ref capModsFactor, "capModFactorCached", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref capModsOffset, "capModOffsetCached", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref multiplyStatChangesBySeverity, "multiplyStatChangesBySeverity");
            Scribe_Defs.Look(ref statOffsetEffectMultiplier, "statOffsetEffectMultiplier");
            Scribe_Defs.Look(ref statFactorEffectMultiplier, "statFactorEffectMultiplier");
            Scribe_Defs.Look(ref capacityFactorEffectMultiplier, "capacityFactorEffectMultiplier");
            Scribe_Values.Look(ref disabledWorkTags, "disabledWorkTags");
            Scribe_Values.Look(ref partEfficiencyOffset, "partEfficiencyOffset");
        }
    }
}
