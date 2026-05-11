using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace CultivatorOfTheRim
{
    public class Hediff_CultivationLevel : Hediff_CultivationBase
    {
        public bool canGetTrib = false;

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
            canGetTrib = this.TryGetComp<HediffComp_Cultivation>().Props.shouldGetTribulation;
        }
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            canGetTrib = this.TryGetComp<HediffComp_Cultivation>().Props.shouldGetTribulation;
        }
        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }

        protected override void OnStageIndexChanged(int stageIndex)
        {
            base.OnStageIndexChanged(stageIndex);
            if (pawn.Faction == Faction.OfPlayerSilentFail)
            {
                Messages.Message($"{pawn.LabelShort} has reached stage {CurStage.label}", MessageTypeDefOf.NeutralEvent, false);
                if (stageIndex == def.stages.Count - 1 && nextRealm != cultivationDef)
                {
                    string label = $"{pawn.LabelShort} ready for breakthrough";
                    string desc = $"{pawn.LabelShort} has reach the peak for current realm and is ready for breakthrough.";
                    if (this.TryGetComp<HediffComp_Cultivation>().Props.shouldGetTribulation)
                    {
                        desc += $"\nif breakthrough now, pawn have {pawn.GetStatValue(CTR_DefOf.TribulationChance).ToStringPercent("0")} chance of tribulation." +
                            $"\nif the risk is too high, consider waiting until the tribulation chance is lower, or prepare the best defense.";
                    }
                    Find.LetterStack.ReceiveLetter(label, desc, LetterDefOf.PositiveEvent, pawn);
                }
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                IEnumerable<Gizmo> enumerable = comps[i].CompGetGizmos();
                if (enumerable == null)
                {
                    continue;
                }

                foreach (Gizmo item in enumerable)
                {
                    yield return item;
                }
            }
        }
        public override void Cultivation_Advance(bool getTrib, CultivationHediffDef curHediff, CultivationHediffDef nextHediff,int duration,int strikeInterval = 250)
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
                if(Rand.Chance(heavenJealous) || nextHediff.guaranteedTribulation)
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
