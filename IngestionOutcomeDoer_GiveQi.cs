using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class IngestionOutcomeDoer_GiveQi : IngestionOutcomeDoer
    {
		public FloatRange severity = new FloatRange(0.01f,0.1f);

		public List<string> minimumCultivationLevel;
        //public IEnumerable<Hediff> h2list;

        public HediffDef hediffDef;
		public HediffDef backlashHediffDef;
		public bool notForMortal = false;
		public bool lethalForMortal = false;
        public bool allowBodyCultivator = false;
        public bool onlyBodyCultivator = false;
		public float Hediffchance = 1f;
		
        public PillGrade pillGrade = PillGrade.Spirit;        
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            Hediff level = null;
            if (allowBodyCultivator)
            {
                if (onlyBodyCultivator)
                {
                    level = pawn.FindBodyCultivationLevel();
                }
                else
                {
                    level = pawn.FindAnyCultivationLevel();
                }
            }
            else
            {
                level = pawn.FindCultivationLevel();
            }

			if(level != null)
            {
                if (!minimumCultivationLevel.NullOrEmpty() && minimumCultivationLevel.Any(x => level.def.tags.Contains(x)))
                {
                    ingested.TryGetPillGrade(out var gc);
                    float num = 1f;
                    switch (gc)
                    {
                        case PillGrade.Spirit:
                            num = 1f;
                            break;
                        case PillGrade.Earth:
                            num = 1.1f;
                            break;
                        case PillGrade.Heaven:
                            num = 1.2f;
                            break;
                        case PillGrade.Mysterious:
                            num = 1.3f;
                            break;
                        case PillGrade.Divine:
                            num = 1.4f;
                            break;
                        case PillGrade.Emperor:
                            num = 1.5f;
                            break;
                        default:
                            num = 1f;
                            break;
                    }
					float effect = severity.RandomInRange;
					effect /= pawn.BodySize;
					level.Severity += effect;
					if(hediffDef != null)
					{
                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = 0.001f;
                        hediff.TryGetComp<HediffComp_AbsorbingPill>().pillGrade = gc;
                        if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                        {
                            int duration = hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear;
                            duration = Mathf.RoundToInt(duration * num);
                            hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = duration;
                        }
                        pawn.health.AddHediff(hediff);
                    }					

				}
				else
                {
					if(backlashHediffDef != null && Rand.Chance(0.75f))
                    {
                        Messages.Message(pawn.LabelShort + " cultivation level too low, there might be some backlash from consuming pill far beyond current level", MessageTypeDefOf.NegativeEvent);
                        Backlash(pawn);
					}										
				}
			}						
			else if(ingested.def != CTR_DefOf.CTR_JunkPill)
            {
                if (notForMortal)
                {
					if(lethalForMortal && Rand.Value < 0.75f)
					{
						pawn.Kill(new DamageInfo(CTR_DefOf.CTR_Qi_Injury,99999,999,hitPart: pawn.health.hediffSet.GetBrain()));
					}
					else
					{
                        Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 240000;
                        pawn.health.AddHediff(hediff);
                    }                    
                }				
				else if(Rand.Chance(Hediffchance))
				{                    
					if(ingested.def == CTR_DefOf.CTR_TribulationRemnantPill || ingested.def == CTR_DefOf.CTR_QiCondensingPill)
					{
                        if (!pawn.HaveBodyCultivation())
                        {
                            Hediff hediff = Cultivation_Utility.CreateHediffNoDuration(pawn, CTR_DefOf.CTR_BodyTempering);
                            hediff.Severity = 0.001f;
                            pawn.health.AddHediff(hediff);
                            pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(pawn.Map, pawn.Position, 0, 5));
                            string text = "Become Cultivator!";
                            string text2 = pawn.LabelShort + " " + "has consume a spirit item and step onto the path of cultivation!";
                            Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent);
                        }
                        else
                        {
                            Messages.Message(pawn.LabelShort + " is already a body cultivator", MessageTypeDefOf.NegativeEvent);
                        }
                    }
                    if (ingested.def == CTR_DefOf.CTR_BodyTemperingPill)
                    {
                        if (!pawn.HaveCultivation())
                        {
                            Hediff hediff = Cultivation_Utility.CreateHediffNoDuration(pawn, CTR_DefOf.CTR_MartialApprentice);
                            hediff.Severity = 0.001f;
                            pawn.health.AddHediff(hediff);
                            pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(pawn.Map, pawn.Position, 0, 5));
                            string text = "Become Body Cultivator!";
                            string text2 = pawn.LabelShort + " " + "has consume a body tempering pill and step onto the path of body cultivation!";
                            Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent);
                        }
                        else
                        {
                            Messages.Message(pawn.LabelShort + " is already a qi cultivator", MessageTypeDefOf.NegativeEvent);
                        }
                    }
                }                
            }
		}

		public void Backlash(Pawn pawn)
        {
			if (pawn.health.hediffSet.HasHediff(backlashHediffDef))
			{
				pawn.health.hediffSet.GetFirstHediffOfDef(backlashHediffDef).Severity += severity.RandomInRange;
			}
			else
			{
                Hediff hediff = HediffMaker.MakeHediff(backlashHediffDef, pawn);
                hediff.Severity = severity.RandomInRange;
                pawn.health.AddHediff(hediff);
            }
        }
	}
}
