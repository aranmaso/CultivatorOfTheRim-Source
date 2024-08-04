using RimWorld;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffComp_AffectByTimeOfDay : HediffComp
    {

        public bool isDayTime => GenLocalDate.DayPercent(Pawn.Map) >= 0.25f && GenLocalDate.DayPercent(Pawn.Map) <= 0.75f;

        public HediffCompProperties_AffectByTimeOfDay Props => (HediffCompProperties_AffectByTimeOfDay)props;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if(Pawn.IsHashIntervalTick(250))
            {
                if(Props.isDayOnly)
                {
                    if(isDayTime)
                    {
                        parent.Severity = 2;
                    }
                    else
                    {
                        parent.Severity = 1;
                    }
                }
                if (Props.isNightOnly)
                {
                    if(!isDayTime)
                    {
                        parent.Severity = 2;
                    }
                    else
                    {
                        parent.Severity = 1;
                    }
                }                
            }               
        }
    }
}
