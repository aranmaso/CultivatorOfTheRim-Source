using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffComp_TimeOfDayEffect : HediffComp
    {
        public HediffCompProperties_TimeOfDayEffect Props => (HediffCompProperties_TimeOfDayEffect)props;
        //public bool isDayTime => GenLocalDate.DayPercent(Pawn.Map) >= 0.25f && GenLocalDate.DayPercent(Pawn.Map) <= 0.75f;
        public bool isDayTime
        {
            get
            {
                if(Pawn.Map != null)
                {
                    if(Pawn.Map.skyManager.CurSkyGlow <= 0f)
                    {
                        return false;
                    }
                    if (GenLocalDate.DayPercent(Pawn.Map) >= 0.25f && GenLocalDate.DayPercent(Pawn.Map) <= 0.75f)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(250))
            {
                if (isDayTime)
                {
                    parent.Severity = Props.severityAtDay;
                }
                else
                {
                    parent.Severity = Props.severityAtNight;
                }

            }
        }
    }
}
