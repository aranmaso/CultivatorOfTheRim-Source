using RimWorld;
using System.Collections.Generic;
using Verse;
namespace CultivatorOfTheRim
{
    public class HediffGiver_PillBacklash : HediffGiver
    {

        public FloatRange severityAmount;

        public int tickInterval;
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (pawn.IsHashIntervalTick(tickInterval))
            {
                HediffGiverUtility.TryApply(pawn, hediff, partsToAffect, canAffectAnyLivePart, countToAffect);
                pawn.health.hediffSet.GetFirstHediffOfDef(hediff).Severity += severityAmount.RandomInRange;
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
