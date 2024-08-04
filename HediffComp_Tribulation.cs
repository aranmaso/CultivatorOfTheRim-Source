using RimWorld;
using Verse;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CultivatorOfTheRim
{
    public class HediffComp_Tribulation : HediffComp
    {
        public int OriginalDuration = 0;
        public int Duration = 0;
        public int Severity = 0;
        public int Mitigation = 0;
        public int StrikeInterval = 250;
        public HediffDef curLevel;
        public HediffDef nextlevel;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref Duration, "duration", 0);
            Scribe_Values.Look(ref Severity, "Severity", 0);
            Scribe_Values.Look(ref Mitigation, "Mitigation", 0);
            Scribe_Values.Look(ref StrikeInterval, "StrikeInterval", 250);
            Scribe_Defs.Look(ref curLevel, "curLevel");
            Scribe_Defs.Look(ref nextlevel, "nextlevel");
        }
        public override void CompPostTick(ref float severityAdjustment)
        {            
            while(Duration > OriginalDuration/2)
            {
                if (Pawn.IsHashIntervalTick(StrikeInterval))
                {                    
                    doStrike();
                }
                Duration--;
                return;
            }
            if (Duration < OriginalDuration/2)
            {
                if (Pawn.IsHashIntervalTick(Mathf.Min(StrikeInterval/2,60)))
                {
                    doStrike();
                }
            }
            Duration--;

        }
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if(CultivatorOfTheRimMod.settings.isTribulationChangeWeather)
            {
                Map map = Pawn.Map;
                map.weatherManager.curWeather = WeatherDef.Named("RainyThunderstorm");
                map.weatherManager.TransitionTo(WeatherDef.Named("RainyThunderstorm"));
            }
            
        }
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (!Pawn.Dead)
            {
                /*Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(curLevel));
                Pawn.health.AddHediff(nextlevel);
                Pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughCounter).Severity = 1.1f;*/
                string text = "Tribulation Survived!";
                //string text2 = Pawn.LabelShort + " " + "has survive a heavenly tribulation and breakthrough from" + curLevel.label + " to" + nextlevel.label + " Realm";
                string text2 = Pawn.LabelShort + " " + "has survive a heavenly tribulation";
                Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent);
            }
        }

        public void doStrike()
        {
            if(Pawn.Map == null) return;
            IEnumerable<IntVec3> validLoc = Pawn.Map.AllCells.Where(x => Cultivation_Utility.CellFilter(x,Pawn.Map));
            IEnumerable<IntVec3> validLoc2 = Pawn.Map.areaManager.Home.ActiveCells;
            int num = Rand.Range(1, Mathf.RoundToInt(parent.Severity));
            for(int i = 0; i < num; i++)
            {
                IntVec3 strikeLoc = Rand.Value <= 0.25 ? validLoc.RandomElement() : validLoc2.RandomElement();
                if (Rand.Value < 0.5f)
                {
                    strikeLoc = Pawn.Position;
                }
                Pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(Pawn.Map, strikeLoc, Mathf.RoundToInt(parent.Severity) * Rand.Range(1, 10), Rand.Range(1, 5)));
            }
            /*IntVec3 strikeLoc = validLoc.RandomElement();
            if (Rand.Value < 0.25f)
            {
                strikeLoc = Pawn.Position;
            }
            Pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(Pawn.Map, strikeLoc, Mathf.RoundToInt(parent.Severity) * Rand.Range(1,10), Rand.Range(1, 5)));
            //GenExplosion.DoExplosion(strikeLoc, Pawn.Map, 2f, DamageDefOf.Bomb, null, Mathf.RoundToInt(parent.Severity) * 10, 2f);*/
        }
        
    }
}
