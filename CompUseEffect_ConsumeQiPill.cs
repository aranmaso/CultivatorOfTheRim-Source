using RimWorld;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim;

public class CompUseEffect_ConsumeQiPill : CompUseEffect
{
    public CompProperties_UseEffectConsumeQiPill Props => (CompProperties_UseEffectConsumeQiPill)props;
    
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
        Hediff level = null;
        if (Props.allowBodyCultivator)
        {
            if (Props.onlyBodyCultivator)
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
            if (!Props.minimumCultivationLevel.NullOrEmpty() && Props.minimumCultivationLevel.Any(x => level.def.tags.Contains(x)))
            {
                PillGrade gc = GetItemGrade(pawn,out var num);
                float effect = Props.severity.RandomInRange;
                effect /= pawn.BodySize;
                level.Severity += effect;
                if(Props.hediffDef != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
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
                if(Props.backlashHediffDef != null && Rand.Chance(0.75f))
                {
                    Messages.Message(pawn.LabelShort + " cultivation level too low, there might be some backlash from consuming pill far beyond current level", MessageTypeDefOf.NegativeEvent);
                    Backlash(pawn);
                }										
            }
        }						
        else if(parent.def != CTR_DefOf.CTR_JunkPill)
        {
            if (Props.notForMortal)
            {
                if(Props.lethalForMortal && Rand.Value < 0.75f)
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
            else if(Rand.Chance(Props.Hediffchance))
            {                    
                if(parent.def == CTR_DefOf.CTR_TribulationRemnantPill || parent.def == CTR_DefOf.CTR_QiCondensingPill)
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
                if (parent.def == CTR_DefOf.CTR_BodyTemperingPill)
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
        if (pawn.health.hediffSet.HasHediff(Props.backlashHediffDef))
        {
            pawn.health.hediffSet.GetFirstHediffOfDef(Props.backlashHediffDef).Severity += Props.severity.RandomInRange;
        }
        else
        {
            Hediff hediff = HediffMaker.MakeHediff(Props.backlashHediffDef, pawn);
            hediff.Severity = Props.severity.RandomInRange;
            pawn.health.AddHediff(hediff);
        }
    }
}