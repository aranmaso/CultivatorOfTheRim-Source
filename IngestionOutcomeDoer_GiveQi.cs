using System.Collections.Generic;
using System.Linq;
using RimWorld;
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
		public float Hediffchance = 1f;

		/*public SimpleCurve curves = new SimpleCurve() {
			new CurvePoint(1,1),
			new CurvePoint(2,2),
			new CurvePoint(2,2),
			new CurvePoint(2,2),
			new CurvePoint(2,2),
			new CurvePoint(2,2),
			new CurvePoint(2,2),
		};*/
		

		public bool cultivationTooLow = false;
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
			Hediff_CultivationLevel level = Cultivation_Utility.FindCultivationLevel(pawn);				
			if(level != null)
            {

                if (!minimumCultivationLevel.NullOrEmpty() && minimumCultivationLevel.Any(x => level.def.tags.Contains(x)))
                {
					cultivationTooLow = false;
					float effect = severity.RandomInRange;
					effect /= pawn.BodySize;
					level.Severity += effect;
					if(hediffDef != null)
					{
                        Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                        hediff.Severity = 0.001f;
						ingested.TryGetPillGrade(out var gc);
                        hediff.TryGetComp<HediffComp_AbsorbingPill>().pillGrade = gc;
                        pawn.health.AddHediff(hediff);
                    }					

				}
				else
                {
					cultivationTooLow = true;
					if(backlashHediffDef != null && Rand.Value < 0.75f)
                    {
                        Messages.Message(pawn.LabelShort + " cultivation level too low, there might be some backlash from consuming pill far beyond current level", MessageTypeDefOf.NeutralEvent);
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
				else if(Rand.Value <= Hediffchance)
				{
                    Hediff hediff = Cultivation_Utility.CreateHediffNoDuration(pawn, CTR_DefOf.CTR_BodyTempering);
                    hediff.Severity = 0.001f;
                    pawn.health.AddHediff(hediff);
					if(ingested.def == CTR_DefOf.CTR_TribulationRemnantPill)
					{
                        pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(pawn.Map, pawn.Position, 0, 5));
                        string text = "Become Cultivator!";
                        string text2 = pawn.LabelShort + " " + "has consume a heavenly tribulation remnant and step onto the path of cultivation!";
                        Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent);
                    }
                }                
            }
		}

		public void Backlash(Pawn pawn)
        {
			Hediff hediff = HediffMaker.MakeHediff(backlashHediffDef, pawn);
			hediff.Severity = 0.001f;
			pawn.health.AddHediff(hediff);
        }
	}
}
