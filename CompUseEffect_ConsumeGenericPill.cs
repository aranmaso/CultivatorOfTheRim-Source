using RimWorld;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim;

public class CompUseEffect_ConsumeGenericPill : CompUseEffect
{
    public CompProperties_UseEffectConsumeGenericPill Props => (CompProperties_UseEffectConsumeGenericPill)props;

    public PillGrade GetItemGrade(Pawn pawn, out float num)
    {
        parent.TryGetPillGrade(out var gc);
        num = 1f;
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
        return gc;
    }
    
    public override void DoEffect(Pawn pawn)
    {
        base.DoEffect(pawn);
        PillGrade gc = GetItemGrade(pawn, out var num);
        if (Props.hediffDef != null)
        {
            if(pawn.health.hediffSet.HasHediff(Props.hediffDef))
            {
                Hediff hediff = Cultivation_Utility.GetFirstHediffOfDef(pawn, Props.hediffDef);
                hediff.Severity += Props.severity.RandomInRange * num;
                float duration = Props.baseDuration;
                duration *= num;
                if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                {
                    hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(duration);
                }                    
            }
            else
            {
                Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
                hediff.Severity = Props.severity.RandomInRange * num;
                float duration = Props.baseDuration;
                duration *= num;
                if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                {
                    hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(Props.baseDuration);
                }
                pawn.health.AddHediff(hediff);
            }
        }
        if (Props.specialHediffDef != null)
        {
            if (pawn.gender == Props.genderRequirement)
            {
                if(pawn.health.hediffSet.HasHediff(Props.specialHediffDef))
                {
                    Hediff hediff = Cultivation_Utility.GetFirstHediffOfDef(pawn, Props.specialHediffDef);
                    hediff.Severity += Props.severity.RandomInRange * num;
                    float duration = Props.baseDuration;
                    duration *= num;
                    if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                    {
                        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(duration);
                    }                    
                }
                else
                {
                    BodyPartRecord br = pawn.health.hediffSet.GetBodyPartRecord(Props.part ?? CTR_DefOf.Pelvis);
                    Hediff hediff = HediffMaker.MakeHediff(Props.specialHediffDef, pawn,br);
                    hediff.Severity = Props.severity.RandomInRange * num;
                    float duration = Props.baseDuration;
                    duration *= num;
                    if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                    {
                        hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(duration);
                    }
                    pawn.health.AddHediff(hediff,br);
                }
            }
        }
        if(Props.hediffDefToRemove != null)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDefToRemove);
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