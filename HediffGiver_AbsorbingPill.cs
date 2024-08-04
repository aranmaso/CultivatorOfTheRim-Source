using RimWorld;
using System.Collections.Generic;
using Verse;
namespace CultivatorOfTheRim
{
    public class HediffGiver_AbsorbingPill : HediffGiver
    {
        public FloatRange severityAmount;

        public int tickInterval;
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if(pawn.IsHashIntervalTick(tickInterval))
            {
                Hediff_CultivationLevel level = Cultivation_Utility.FindCultivationLevel(pawn);
                if(level != null)
                {
                    level.Severity += severityAmount.RandomInRange * pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
                }                
            }
        }
        public override IEnumerable<string> ConfigErrors()
        {
            if (float.IsNaN(severityAmount.RandomInRange))
            {
                yield return "severityAmount is not defined";
            }
        }
    }
}
