using Verse;
using RimWorld;
using System.Text;
using UnityEngine;

namespace CultivatorOfTheRim
{
    public class HediffComp_BreakthroughProcess : HediffComp
    {
        public HediffDef curLevel;

        public HediffDef nextLevel;

        /*public float breakthroughChance()
        {
            if(Pawn.RaceProps.Humanlike)
            {
                float baseNum = 1f;
                float num = Pawn.health.summaryHealth.SummaryHealthPercent;
                float num2 = Pawn.needs.mood.CurLevelPercentage;
                float num3 = Mathf.Clamp(Pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness), 0.01f, 1f);
                float final = baseNum * num * num2 * num3;
                return final;
            }
            return 1f;
        }*/
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Defs.Look(ref curLevel, "curLevel");
            Scribe_Defs.Look(ref nextLevel, "nextLevel");
        }
        public override void CompPostPostRemoved()
        {
            //base.CompPostPostRemoved();
            if(CultivatorOfTheRimMod.settings.isBreakthroughCanFailForHumanlike)
            {
                float chance = Cultivation_Utility.GetBreakthroughChance(Pawn,nextLevel);
                
                if (Rand.Value <= chance)
                {
                    string text = "Breakingthrough!";
                    string text2 = Pawn.LabelShort + " " + "has breakthrough from " + " " + curLevel.LabelCap + " to " + " " + nextLevel.LabelCap;
                    Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent, parent.pawn);

                    Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(curLevel));
                    Pawn.health.AddHediff(nextLevel);

                    if (Pawn.Map != null)
                    {
                        Pawn.Map?.weatherManager?.eventHandler?.AddEvent(new WeatherEvent_LightningStrikeTribulation(Pawn.Map, Pawn.Position, 0, 3));
                    }
                    Pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughCounter).Severity = 1.1f;
                }
                else
                {
                    float baseNum = 1f;
                    float num = Pawn.health.summaryHealth.SummaryHealthPercent;
                    float num2 = Pawn.needs.mood.CurLevelPercentage;
                    float num3 = Mathf.Clamp(Pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness), 0.01f, 1f);
                    float final = baseNum * num * num2 * num3;
                    string text = "Breakingthrough failed!";
                    StringBuilder text2 = new StringBuilder();
                    text2.Append(Pawn.LabelShort + " " + "has failed to breakthrough the " + curLevel.LabelCap + " realm");
                    text2.AppendInNewLine("breakthrough chance: " + chance.ToStringPercent("0.00"));
                    text2.AppendLine();
                    text2.AppendLine("base chance: " + baseNum.ToStringPercent("0"));
                    text2.AppendLine("current health: " + num.ToStringPercent("0"));
                    text2.AppendLine("current mood: " + num2.ToStringPercent("0"));
                    text2.AppendLine("current consciousness: " + num3.ToStringPercent("0"));
                    if(nextLevel == CTR_DefOf.CTR_OutsidetheDomain)
                    {
                        float num4 = Pawn.GetStatValue(CTR_DefOf.TribulationChance);
                        float num5 = 1f - num4;
                        text2.AppendLine("tribulation chance: " + num5.ToStringPercent());
                    }
                    text2.AppendLine("global modifier: " + CultivatorOfTheRimMod.settings.breakthroughSuccessOverallModifier.ToStringPercent("0.00"));
                    if (nextLevel == CTR_DefOf.CTR_OutsidetheDomain)
                    {
                        float num4 = Pawn.GetStatValue(CTR_DefOf.TribulationChance);
                        float num5 = 1f - num4;
                        text2.AppendLine("(" + baseNum.ToStringPercent("0") + "*" + num.ToStringPercent("0") + "*" + num2.ToStringPercent("0") + "*" + num3.ToStringPercent("0") + "*" + num5.ToStringPercent("0") + "+" + CultivatorOfTheRimMod.settings.breakthroughSuccessOverallModifier.ToStringPercent("0") + "=" + final.ToStringPercent("0.00") + ")");
                    }
                    else
                    {
                        text2.AppendLine("(" + baseNum.ToStringPercent("0") + "*" + num.ToStringPercent("0") + "*" + num2.ToStringPercent("0") + "*" + num3.ToStringPercent("0") + "*"+ CultivatorOfTheRimMod.settings.breakthroughSuccessOverallModifier.ToStringPercent("0") + "=" + final.ToStringPercent("0.00") + ")");
                    }
                    Find.LetterStack.ReceiveLetter(text, text2.ToString(), LetterDefOf.NegativeEvent, Pawn);
                    if (Pawn.Map != null)
                    {
                        Pawn.Map?.weatherManager?.eventHandler?.AddEvent(new WeatherEvent_LightningStrikeTribulation(Pawn.Map, Pawn.Position, 0, 3));
                    }
                    Pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughCounter).Severity = 0.5f;
                }
            }
            else
            {
                string text = "Breakingthrough!";
                string text2 = Pawn.LabelShort + " " + "has breakthrough from " + " " + curLevel.LabelCap + " to " + " " + nextLevel.LabelCap;
                Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.PositiveEvent, parent.pawn);

                Pawn.health.RemoveHediff(Pawn.health.hediffSet.GetFirstHediffOfDef(curLevel));
                Pawn.health.AddHediff(nextLevel);

                if (Pawn.Map != null)
                {
                    Pawn.Map?.weatherManager?.eventHandler?.AddEvent(new WeatherEvent_LightningStrikeTribulation(Pawn.Map, Pawn.Position, 0, 3));
                }
                Pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughCounter).Severity = 1.1f;
            }
        }
    }
}
