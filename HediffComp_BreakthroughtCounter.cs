using Verse;
using RimWorld;

namespace CultivatorOfTheRim
{
    public class HediffComp_BreakthroughtCounter : HediffComp
    {
        public int tickSinceLastBreakthrought = 0;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref tickSinceLastBreakthrought, "tickSinceLastBreakthrought", 0);
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (tickSinceLastBreakthrought >= 0)
                {
                    return base.CompLabelInBracketsExtra + tickSinceLastBreakthrought.ToStringTicksToPeriod(true,true,true,true);
                }
                return base.CompLabelInBracketsExtra;
            }
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            tickSinceLastBreakthrought++;
        }
    }
}
