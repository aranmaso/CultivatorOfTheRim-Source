using RimWorld;
using Verse;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class HediffComp_AbsorbingPill : HediffComp
    {
        public HediffCompProperties_AbsorbingPill Props => (HediffCompProperties_AbsorbingPill)props;

        public PillGrade pillGrade = PillGrade.Spirit;
        public float multiplier
        {
            get
            {
                switch(pillGrade)
                {
                    case PillGrade.Spirit:
                        return 1f;
                    case PillGrade.Earth:
                        return 1.15f;
                    case PillGrade.Heaven:
                        return 1.2f;
                    case PillGrade.Mysterious:
                        return 1.25f;
                    case PillGrade.Divine:
                        return 1.3f;
                    case PillGrade.Emperor:
                        return 1.35f;
                    default:
                        return 1f;
                }            
            }
            set
            {
                multiplier = value;
            }
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref pillGrade, "pillGrade",PillGrade.Spirit);
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if(Pawn.IsHashIntervalTick(Props.tickInterval))
            {
                Hediff_CultivationLevel level = Cultivation_Utility.FindCultivationLevel(Pawn);
                if (level != null)
                {
                    level.Severity += Props.severityAmount.RandomInRange * Pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
                }
            }
        }
        public override string CompLabelInBracketsExtra
        {
            get
            {                
                return base.CompLabelInBracketsExtra + pillGrade.ToString();
            }
        }
    }
}
