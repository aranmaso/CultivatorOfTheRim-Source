using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class Hediff_CultivationLevel : HediffWithComps
    {

        public void Cultivation_Advance(bool getTrib,bool guaranteedTrib, HediffDef curHediff, HediffDef nextHediff,int duration,int strikeInterval = 250,SimpleCurve curve = null)
        {
            if (!pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughCounter))
            {
                Hediff hediff2 = Cultivation_Utility.CreateHediffNoDuration(pawn, CTR_DefOf.CTR_BreakthroughCounter);
                pawn.health.AddHediff(hediff2);
            }
            if (!pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
            {
                Hediff hediff2 = Cultivation_Utility.CreateHediff(pawn, CTR_DefOf.CTR_BreakthroughProcess, duration);
                hediff2.TryGetComp<HediffComp_BreakthroughProcess>().curLevel = curHediff;
                hediff2.TryGetComp<HediffComp_BreakthroughProcess>().nextLevel = nextHediff;
                pawn.health.AddHediff(hediff2);
            }
            if (getTrib)
            {
                float heavenJealous = pawn.GetStatValue(CTR_DefOf.TribulationChance);
                /*if (curve != null)
                {
                    int tickSinceBreakthrough = pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughCounter).TryGetComp<HediffComp_BreakthroughtCounter>().tickSinceLastBreakthrought;
                    int daySinceBreakthrough = tickSinceBreakthrough / 60000;
                    heavenJealous += curve.Evaluate(daySinceBreakthrough);
                }*/
                if(guaranteedTrib)
                {
                    heavenJealous = 1f;
                }
                if(Rand.Value <= heavenJealous)
                {
                    TribulationInfo tinfo = new TribulationInfo();
                    tinfo = Cultivation_Utility.RandomizeTribulation();                    
                    Hediff hediff = Cultivation_Utility.CreateHediff(pawn, CTR_DefOf.CTR_Tribulation, duration);
                    hediff.Severity = tinfo.TribulationType;
                    hediff.TryGetComp<HediffComp_Tribulation>().OriginalDuration = duration;
                    hediff.TryGetComp<HediffComp_Tribulation>().Duration = duration;
                    hediff.TryGetComp<HediffComp_Tribulation>().StrikeInterval = strikeInterval;
                    hediff.TryGetComp<HediffComp_Tribulation>().Severity = tinfo.TribulationType;
                    hediff.TryGetComp<HediffComp_Tribulation>().curLevel = curHediff;
                    hediff.TryGetComp<HediffComp_Tribulation>().nextlevel = nextHediff;
                    pawn.health.AddHediff(hediff);                    
                    string text = "Heavenly Tribulation!";
                    string text2 = pawn.LabelShort + " " + "has attract the jealousy of heaven, and attract a " + hediff.CurStage.label + " Stage.";
                    Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.NegativeEvent,pawn);
                }  
                else
                {
                    pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().curLevel = curHediff;
                    pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().nextLevel = nextHediff;                    
                }
            }      
            else
            {
                pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().curLevel = curHediff;
                pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().nextLevel = nextHediff;                                            
            }                        

        }
    }
}
