using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class CompProperties_GiveHediffTechnique : CompProperties_AbilityEffect
    {
        public HediffDef hediffDef;

        public bool havePenaltyStat;

        public StatDef penaltyStat;

        public float penaltyThreshold;

        public float severity;

        public float penaltySeverity;

        public int duration;
        public CompProperties_GiveHediffTechnique() 
        {
            compClass = typeof(CompAbilityEffect_GiveHediffTechnique);
        }
    }
}
