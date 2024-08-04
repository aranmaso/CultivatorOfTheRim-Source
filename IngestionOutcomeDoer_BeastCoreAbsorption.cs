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
    public class IngestionOutcomeDoer_BeastCoreAbsorption : IngestionOutcomeDoer
    {
        public HediffDef HediffDef;

        public FloatRange severity = new FloatRange(1,10);
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            Hediff_CultivationLevel level = Cultivation_Utility.FindCultivationLevel(pawn);
            
            if (level != null)
            {                
                HediffDef pawnCult = level.def;
                CompBeastCore beastCore = ingested.TryGetComp<CompBeastCore>();
                if (beastCore != null)
                {
                    HediffDef beastCult = beastCore.ownerCultivation;
                    int num = Cultivation_Utility.realmListAll[level.def];
                    int num2 = Cultivation_Utility.realmListAll[beastCult];
                    if (level.def.tags.Contains("Mortal_Transforming"))
                    {
                        Messages.Message("too low cultivation, the core is wasted", MessageTypeDefOf.NegativeEvent);
                        return;
                    }
                    if(num2 < 4)
                    {
                        level.Severity += Rand.Range(0.1f, 0.5f);                        
                        if (num2 > num && num2 - num > 3)
                        {
                            if (Rand.Value < 0.5f)
                            {
                                Messages.Message("the beast cultivation is more than 3 level higher than " + pawn.LabelShort, MessageTypeDefOf.NegativeEvent);
                                Messages.Message("the excess Qi cause " + pawn.LabelShort + " to collapse", MessageTypeDefOf.NegativeEvent);
                                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000 * (num2 - num);
                                pawn.health.AddHediff(hediff);
                                if (Rand.Value < 0.25f)
                                {
                                    Messages.Message("the excess Qi cause " + pawn.LabelShort + " to explode", MessageTypeDefOf.NegativeEvent);
                                    GenExplosion.DoExplosion(pawn.Position, pawn.Map, num2 / 2, CTR_DefOf.CTR_Qi_Injury_Explosion, null, num2, 2);
                                }
                            }
                        }
                    }
                    if (num2 >= 4 && num2 < 8)
                    {
                        level.Severity += Rand.Range(5f, 10f);
                        if (num2 > num && num2 - num > 3)
                        {
                            if(Rand.Value < 0.5f)
                            {
                                Messages.Message("the beast cultivation is more than 3 level higher than " + pawn.LabelShort,MessageTypeDefOf.NegativeEvent);
                                Messages.Message("the excess Qi cause " + pawn.LabelShort + " to collapse",MessageTypeDefOf.NegativeEvent);
                                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000 * (num2 - num);
                                pawn.health.AddHediff(hediff);
                                if(Rand.Value < 0.25f)
                                {
                                    Messages.Message("the excess Qi cause " + pawn.LabelShort + " to explode", MessageTypeDefOf.NegativeEvent);
                                    GenExplosion.DoExplosion(pawn.Position,pawn.Map,num2 / 2,CTR_DefOf.CTR_Qi_Injury_Explosion,null,num2,2);
                                }
                            }
                        }
                    }
                    if (num2 >= 8 && num2 < 13)
                    {
                        level.Severity += Rand.Range(10f, 20f);
                        if (num2 > num && num2 - num > 3)
                        {
                            if (Rand.Value < 0.5f)
                            {
                                Messages.Message("the beast cultivation is more than 3 level higher than " + pawn.LabelShort, MessageTypeDefOf.NegativeEvent);
                                Messages.Message("the excess Qi cause " + pawn.LabelShort + " to collapse", MessageTypeDefOf.NegativeEvent);
                                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000 * (num2 - num);
                                pawn.health.AddHediff(hediff);
                                if (Rand.Value < 0.25f)
                                {
                                    Messages.Message("the excess Qi cause " + pawn.LabelShort + " to explode", MessageTypeDefOf.NegativeEvent);
                                    GenExplosion.DoExplosion(pawn.Position, pawn.Map, num2 / 2, CTR_DefOf.CTR_Qi_Injury_Explosion, null, num2, 2);
                                }
                            }
                        }
                    }
                    if (num2 >= 13 && num2 < 16)
                    {
                        level.Severity += Rand.Range(20f, 40f);
                        if (num2 > num && num2 - num > 3)
                        {
                            if (Rand.Value < 0.5f)
                            {
                                Messages.Message("the beast cultivation is more than 3 level higher than " + pawn.LabelShort, MessageTypeDefOf.NegativeEvent);
                                Messages.Message("the excess Qi cause " + pawn.LabelShort + " to collapse", MessageTypeDefOf.NegativeEvent);
                                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000 * (num2 - num);
                                pawn.health.AddHediff(hediff);
                                if (Rand.Value < 0.25f)
                                {
                                    Messages.Message("the excess Qi cause " + pawn.LabelShort + " to explode", MessageTypeDefOf.NegativeEvent);
                                    GenExplosion.DoExplosion(pawn.Position, pawn.Map, num2 / 2, CTR_DefOf.CTR_Qi_Injury_Explosion, null, num2, 2);
                                }
                            }
                        }
                    }
                    if (num2 >= 16 && num2 < 18)
                    {
                        level.Severity += Rand.Range(50f, 100f);
                        if (num2 > num && num2 - num > 3)
                        {
                            if (Rand.Value < 0.5f)
                            {
                                Messages.Message("the beast cultivation is more than 3 level higher than " + pawn.LabelShort, MessageTypeDefOf.NegativeEvent);
                                Messages.Message("the excess Qi cause " + pawn.LabelShort + " to collapse", MessageTypeDefOf.NegativeEvent);
                                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000 * (num2 - num);
                                pawn.health.AddHediff(hediff);
                                if (Rand.Value < 0.25f)
                                {
                                    Messages.Message("the excess Qi cause " + pawn.LabelShort + " to explode", MessageTypeDefOf.NegativeEvent);
                                    GenExplosion.DoExplosion(pawn.Position, pawn.Map, num2 / 2, CTR_DefOf.CTR_Qi_Injury_Explosion, null, num2, 2);
                                }
                            }
                        }
                    }
                    if (num2 >= 18)
                    {
                        level.Severity += Rand.Range(100f, 200f);
                        if (num2 > num && num2 - num > 3)
                        {
                            if (Rand.Value < 0.5f)
                            {
                                Messages.Message("the beast cultivation is more than 3 level higher than " + pawn.LabelShort, MessageTypeDefOf.NegativeEvent);
                                Messages.Message("the excess Qi cause " + pawn.LabelShort + " to collapse", MessageTypeDefOf.NegativeEvent);
                                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 60000 * (num2 - num);
                                pawn.health.AddHediff(hediff);
                                if (Rand.Value < 0.25f)
                                {
                                    Messages.Message("the excess Qi cause " + pawn.LabelShort + " to explode", MessageTypeDefOf.NegativeEvent);
                                    GenExplosion.DoExplosion(pawn.Position, pawn.Map, num2 / 2, CTR_DefOf.CTR_Qi_Injury_Explosion, null, num2, 2);
                                }
                            }
                        }
                    }
                }
                
            }
            else
            {
                if (Rand.Value <= 0.10f)
                {
                    Messages.Message("the excess Qi cause " + pawn.LabelShort + " to perish", MessageTypeDefOf.NegativeEvent);
                    pawn.Kill(new DamageInfo(CTR_DefOf.CTR_Qi_Injury, 99999, 999, hitPart: pawn.health.hediffSet.GetBrain()));
                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                    hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 240000;
                    pawn.health.AddHediff(hediff);
                }
            }
        }
    }
}
