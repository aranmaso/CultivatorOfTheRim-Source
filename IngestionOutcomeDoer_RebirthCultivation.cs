using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class IngestionOutcomeDoer_RebirthCultivation : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (pawn.HaveCultivationOutHediff(out var hediff_Qi))
            {
                if (hediff_Qi?.cultivationDef == CTR_DefOf.CTR_OutsidetheDomain ||
                    hediff_Qi?.cultivationDef == CTR_DefOf.CTR_Creation_Realm)
                {
                    Messages.Message($"Ineffective for cultivation of {hediff_Qi.cultivationDef.LabelCap} realm.",pawn,MessageTypeDefOf.NegativeEvent,false);
                    return;
                }
                if (hediff_Qi.cultivationDef.realmPower >= 4 && hediff_Qi.Severity >= hediff_Qi.def.maxSeverity)
                {
                    Hediff newCultivation = HediffMaker.MakeHediff(CTR_DefOf.CTR_Qi_Gathering,pawn);
                    Hediff_DynamicStage rebirth = (Hediff_DynamicStage)pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_RebirthCultivation);
                    if (rebirth == null)
                    {
                        rebirth = (Hediff_DynamicStage)HediffMaker.MakeHediff(CTR_DefOf.CTR_RebirthCultivation, pawn);
                    }
                    AddRebirthStat(pawn, rebirth, hediff_Qi);
                    newCultivation.Severity = newCultivation.def.stages.FirstOrDefault().minSeverity;
                    pawn.health.RemoveHediff(hediff_Qi);
                    pawn.health.AddHediff(newCultivation);
                    pawn.health.AddHediff(rebirth);
                    Messages.Message($"{pawn.LabelShort} has been rebirth back to Qi Gathering realm",MessageTypeDefOf.NeutralEvent,false);
                }
                else
                {
                    Messages.Message($"{pawn.LabelShort} need to reach the peak of current realm first",MessageTypeDefOf.NeutralEvent,false);
                }
            }
            else if (pawn.HaveBodyCultivationOutHediff(out var hediff_body))
            {
                if (hediff_body.cultivationDef.realmPower >= 1 && hediff_body.Severity >= hediff_Qi.def.maxSeverity)
                {
                    Hediff newCultivation = HediffMaker.MakeHediff(CTR_DefOf.CTR_MartialApprentice,pawn);
                    Hediff_DynamicStage rebirth = (Hediff_DynamicStage)pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_RebirthCultivation);
                    if (rebirth == null)
                    {
                        rebirth = (Hediff_DynamicStage)HediffMaker.MakeHediff(CTR_DefOf.CTR_RebirthCultivation, pawn);
                    }
                    AddRebirthStat(pawn, rebirth, hediff_body);
                    newCultivation.Severity = newCultivation.def.stages.FirstOrDefault().minSeverity;
                    pawn.health.RemoveHediff(hediff_body);
                    pawn.health.AddHediff(newCultivation);
                    pawn.health.AddHediff(rebirth);
                    Messages.Message($"{pawn.LabelShort} has been rebirth back to Martial Apprentice realm", MessageTypeDefOf.NeutralEvent, false);
                }
                else
                {
                    Messages.Message($"{pawn.LabelShort} need to reach the peak of current realm first", MessageTypeDefOf.NeutralEvent, false);
                }
            }
        }

        public void AddRebirthStat(Pawn pawn, Hediff_DynamicStage hediff,Hediff original)
        {
            hediff.rebirthCount++;
            if (original.CurStage.statOffsets.NotNullOrEmpty())
            {
                foreach (var item in original.CurStage.statOffsets)
                {
                    StatModifier statMod = new StatModifier();
                    statMod.stat = item.stat;
                    statMod.value = item.value * 0.1f;
                    hediff.AddOrEditToStat(statMod, true, false,false);
                }
            }
            
            if (original.CurStage.statFactors.NotNullOrEmpty())
            {
                foreach (var item in original.CurStage.statFactors)
                {
                    if (item.value == 1f) continue;
                    StatModifier statMod = new StatModifier();
                    statMod.stat = item.stat;
                    statMod.value = item.value * 0.1f;
                    if (item.value < 1f)
                    {
                        statMod.value *= -1f;
                        /*Log.Message($"statMod.value {statMod.value}");
                        Log.Message("base item.value is below 1.0f: " + item.value);*/
                        hediff.AddOrEditToStat(statMod, false, true,true);
                    }
                    else
                    {
                        hediff.AddOrEditToStat(statMod, false, true,false);
                    }
                }
            }

            if (original.CurStage.capMods.NotNullOrEmpty())
            {
                foreach (var item in original.CurStage.capMods)
                {
                    if (item.postFactor > 1f || item.offset > 0)
                    {
                        PawnCapacityModifier capMod = new PawnCapacityModifier();
                        capMod.capacity = item.capacity;
                        if (item.postFactor > 1f)
                        {
                            float num = item.postFactor;
                            num -= 1f;
                            num *= 0.1f;
                            capMod.postFactor = num;
                        }
                        else if(item.postFactor < 1f)
                        {
                            float num = item.postFactor;
                            num *= 0.1f;
                            num = 1f - num;
                            capMod.postFactor = num;
                        }
                        if (item.offset > 0f)
                        {
                            float num = item.offset * 0.1f;
                            capMod.offset = num;
                        }
                        hediff.AddOrEditToCapMod(capMod);
                    }
                    
                }
            }
        }
    }
}
