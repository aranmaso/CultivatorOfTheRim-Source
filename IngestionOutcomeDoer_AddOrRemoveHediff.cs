using RimWorld;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;

namespace CultivatorOfTheRim
{
    public class IngestionOutcomeDoer_AddOrRemoveHediff : IngestionOutcomeDoer
    {

        public HediffDef hediffDef;

        public HediffDef hediffDefToRemove;

        public float baseDuration;

        public FloatRange severity = new FloatRange(0.1f,0.1f);

        public bool isRemoveAllAtOnce;
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
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
            if (hediffDef != null)
            {
                if(pawn.health.hediffSet.HasHediff(hediffDef))
                {
                    Hediff hediff = Cultivation_Utility.GetFirstHediffOfDef(pawn, hediffDef);
                    hediff.Severity += severity.RandomInRange * num;
                    baseDuration *= num;
                    if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                    {
                        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(baseDuration);
                    }
                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                    hediff.Severity = severity.RandomInRange * num;                    
                    baseDuration *= num;
                    if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                    {
                        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(baseDuration);
                    }
                    pawn.health.AddHediff(hediff);
                }
                
            }
            if(hediffDefToRemove != null)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDefToRemove);
                if(gc >= PillGrade.Divine)
                {
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
                else
                {
                    hediff.Severity -= (0.1f * ((int)gc + 1));
                }
                
            }
        }
    }
}
