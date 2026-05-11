using RimWorld;
using Verse;

namespace CultivatorOfTheRim;

public class HediffComp_AbsorbingPill : HediffComp
{
    public PillGrade pillGrade = PillGrade.Spirit;

    public HediffCompProperties_AbsorbingPill Props => (HediffCompProperties_AbsorbingPill)props;

    public float multiplier
    {
        get
        {
            return pillGrade switch
            {
                PillGrade.Spirit => 1f, 
                PillGrade.Earth => 1.1f, 
                PillGrade.Heaven => 1.2f, 
                PillGrade.Mysterious => 1.3f, 
                PillGrade.Divine => 1.4f, 
                PillGrade.Emperor => 1.5f, 
                _ => 1f, 
            };
        }
        set
        {
            multiplier = value;
        }
    }

    public override string CompLabelInBracketsExtra => base.CompLabelInBracketsExtra + pillGrade;

    public override void CompExposeData()
    {
        base.CompExposeData();
        Scribe_Values.Look(ref pillGrade, "pillGrade", PillGrade.Spirit);
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
        base.CompPostTick(ref severityAdjustment);
        if (base.Pawn.IsHashIntervalTick(Props.tickInterval))
        {
            Hediff hediff = base.Pawn.FindCultivationLevel();
            if (hediff != null)
            {
                float num = Props.severityAmount.RandomInRange * base.Pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
                num *= multiplier;
                hediff.Severity += num;
            }
        }
    }
}