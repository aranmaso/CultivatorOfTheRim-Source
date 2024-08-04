using RimWorld;
using Verse;
using HarmonyLib;
using System.Collections.Generic;
using System;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(ThingMaker), nameof(ThingMaker.MakeThing))]
    public class ThingMaker_MakeThingGrade
    {
        private static void Postfix(ref Thing __result, ref ThingDef def, ThingDef stuff = null)
        {
            if (__result == null) return;
            //if (!__result.def.HasComp(typeof(CompQuality))) return;            
            if (!__result.def.HasComp(typeof(CompItemGrade))) return;            
            /*if (__result.def.Verbs.Any((VerbProperties v) => typeof(Verb_ShootOneUse).IsAssignableFrom(v.GetType())))
            {
                return;
            }*/
            if (__result.MarketValue >= 250)
            {
                if (__result.def.thingClass == typeof(Apparel) || __result.TryGetComp<CompEquippable>() != null)
                {
                    CompItemGrade compItemGrade = __result.TryGetComp<CompItemGrade>();
                    if(compItemGrade != null) 
                    {
                        ItemGrade result = ItemGrade.Mortal;
                        float num = Rand.Value;
                        if (num <= 0.03f)
                        {
                            result = ItemGrade.Dao;
                            compItemGrade.SetGrade(result);
                        }
                        else if (num <= 0.05f)
                        {
                            result = ItemGrade.Emperor;
                            compItemGrade.SetGrade(result);
                        }
                        else if (num <= 0.10f)
                        {
                            result = ItemGrade.Divine;
                            compItemGrade.SetGrade(result);
                        }
                        else if (num <= 0.15f)
                        {
                            result = ItemGrade.Mysterious;
                            compItemGrade.SetGrade(result);
                        }
                        else if (num <= 0.25f)
                        {
                            result = ItemGrade.Heaven;
                            compItemGrade.SetGrade(result);
                        }
                        else if (num <= 0.35f)
                        {
                            result = ItemGrade.Earth;
                            compItemGrade.SetGrade(result);
                        }
                        else if (num <= 0.55f)
                        {
                            result = ItemGrade.Ordinary;
                            compItemGrade.SetGrade(result);
                        }
                        else
                        {
                            result = ItemGrade.Mortal;
                            compItemGrade.SetGrade(result);
                        }


                        /*ItemGrade newGrade = Cultivation_Utility.GenerateGradeTraderItem();
                        if(newGrade < compItemGrade.Grade) 
                        {
                            result = compItemGrade.Grade;
                        }
                        else
                        {
                            result = newGrade;
                        }
                        compItemGrade.SetGrade(result);*/
                    }
                    //__result.TryGetComp<CompItemGrade>().SetGrade(Cultivation_Utility.GenerateQualityTraderItem());

                }
            }
        }
    }
    [HarmonyPatch(typeof(ThingMaker), nameof(ThingMaker.MakeThing))]
    public class ThingMaker_MakeThingPillGrade
    {
        private static void Postfix(ref Thing __result, ref ThingDef def, ThingDef stuff = null)
        {
            if (__result == null) return;         
            if (!__result.def.HasComp(typeof(CompPillGrade))) return;
            if (__result.def.tradeTags.Contains("CTR_Pill"))
            {
                CompPillGrade compPillGrade = __result.TryGetComp<CompPillGrade>();
                if(compPillGrade != null)
                {
                    PillGrade result = PillGrade.Spirit;
                    PillGrade newGrade = Cultivation_Utility.GeneratePillGradeTraderItem();
                    if (newGrade < compPillGrade.Grade)
                    {
                        result = compPillGrade.Grade;
                    }
                    else
                    {
                        result = newGrade;
                    }
                    compPillGrade.SetGrade(result);
                }
            }
        }
    }

    [HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("PostProcessProduct")]
    public class GenRecipe_MakeThingGrade
    {
        private static void Postfix(ref Thing __result,ref Thing product,ref RecipeDef recipeDef,ref Pawn worker,Precept_ThingStyle precept = null, ThingStyleDef style = null, int? overrideGraphicIndex = null)
        {
            CompItemGrade compItemGrade = __result.TryGetComp<CompItemGrade>();
            if (compItemGrade != null) 
            {
                if (recipeDef.workSkill == null)
                {
                    Log.Error(string.Concat(recipeDef, " needs workSkill because it creates a product with a item grade."));
                }
                float refCha = worker.GetStatValue(CTR_DefOf.RefiningSuccessChance);
                ItemGrade g = Cultivation_Utility.GenerateGradeCreatedByPawn(worker);
                if(Rand.Value <= refCha)
                {
                    compItemGrade.SetGrade(g);
                }
                else
                {
                    compItemGrade.SetGrade(ItemGrade.Ordinary);
                }                
                //QualityUtility.SendCraftNotification(product, worker);
            }
        }
    }

    /*[HarmonyPatch(typeof(GenRecipe))]
    [HarmonyPatch("PostProcessProduct")]
    public class GenRecipe_MakeThingPillGrade
    {
        private static void Postfix(ref Thing __result, ref Thing product, ref RecipeDef recipeDef, ref Pawn worker, Precept_ThingStyle precept = null, ThingStyleDef style = null, int? overrideGraphicIndex = null)
        {
            CompPillGrade compPillGrade = __result.TryGetComp<CompPillGrade>();
            if (compPillGrade == null)
            {
                return;
            }
            PillGrade g = Cultivation_Utility.GeneratePillGradeCreatedByPawn(worker);
            compPillGrade.SetGrade(g);
        }
    }*/

    /*[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.PostProcessGeneratedGear))]
    public class PawnGenerator_MakeThingGrade
    {
        private static void Postfix(ref Thing gear,ref Pawn pawn)
        {
            if (gear == null) return;
            if (!gear.def.HasComp(typeof(CompItemGrade))) return;
            if (gear.MarketValue >= 500)
            {
                Log.Message("value exceed 500");
                Log.Message(gear.Label);
                if (gear.def.thingClass == typeof(Apparel) || gear.TryGetComp<CompEquippable>() != null)
                {
                    CompItemGrade compItemGrade = gear.TryGetComp<CompItemGrade>();
                    if (compItemGrade != null)
                    {
                        Log.Message("original grade: " + compItemGrade.Grade.ToString());
                        ItemGrade result = ItemGrade.Mortal;
                        ItemGrade newGrade = Cultivation_Utility.GenerateGradeTraderItem();
                        Log.Message("new grade: " + newGrade.ToString());
                        if (newGrade > compItemGrade.Grade)
                        {
                            result = compItemGrade.Grade;
                        }
                        else
                        {
                            result = newGrade;
                        }
                        Log.Message("final grade: " + result.ToString());
                        compItemGrade.SetGrade(result);
                        
                    }

                }
            }
        }
    }*/
}
