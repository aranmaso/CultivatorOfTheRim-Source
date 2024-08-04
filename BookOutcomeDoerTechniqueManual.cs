using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class BookOutcomeDoerTechniqueManual : BookOutcomeDoer
    {
        public new BookOutcomeProperties_TechniqueManual Props => (BookOutcomeProperties_TechniqueManual)props;

        public SimpleCurve qualityCurve => new SimpleCurve()
        {
            {0 , 0.1f},
            {1 , 0.25f},
            {2 , 0.5f},
            {3 , 0.75f},
            {4 , 1.0f},
            {5 , 1.25f},
            {6 , 1.50f}
        };
        public QualityCategory quality => Book.GetComp<CompQuality>().Quality;

        public float chanceCached;
        public float finalChance
        {
            get
            {
                if(chanceCached <= 0f)
                {
                    chanceCached = (Props.learnChance * qualityCurve.Evaluate((int)quality));
                }
                return chanceCached;
            }
        }

        public override bool DoesProvidesOutcome(Pawn reader)
        {
            return true;
        }

        public override void OnReadingTick(Pawn reader, float factor)
        {
            base.OnReadingTick(reader, factor);   
            if(reader.IsHashIntervalTick(250))
            {
                float num = Rand.Value;
                if (num <= finalChance)
                {
                    //Log.Message("Pass");
                    if (reader.health.hediffSet.HasHediff(Props.hediffDef))
                    {
                        //Messages.Message("AlreadyLearned".Translate(reader.LabelShort, Props.benefitString, reader.Named("USER")), reader, MessageTypeDefOf.PositiveEvent);
                        MoteMaker.ThrowText(reader.DrawPos, reader.Map, "already learned: " + Props.benefitString + " " + "Technique",Color.red);
                        return;
                    }
                    Hediff hediff = Cultivation_Utility.CreateHediffNoDuration(reader, Props.hediffDef);
                    reader.health.AddHediff(hediff);
                    string text = Props.benefitString + " Learned!";
                    string text2 = reader.LabelShort + " " + "has learn the " + Props.benefitString + " " + "Technique";
                    Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent, reader);
                    MoteMaker.ThrowText(reader.DrawPos,reader.Map,"learned: " + Props.benefitString + " " + "Technique");
                }
                MoteMaker.ThrowText(reader.DrawPos, reader.Map, "failed to learn: " + num);
                //Log.Message("fail");
            }            
        }

        public override string GetBenefitsString(Pawn reader = null)
        {
            return Props.benefitString + " = " + finalChance.ToStringPercent("0.00");
        }
    }
}
