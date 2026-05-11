using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Verse.Noise;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Assertions;

namespace CultivatorOfTheRim
{
    public class RecipeWorker_MixingIngredient : RecipeWorker
    {
        private RecipeExtension_MixingIngredient modExtension => recipe.GetModExtension<RecipeExtension_MixingIngredient>();

        private bool passRequirement = false;    
        
        public List<ThingDef> tempList = new List<ThingDef>();

        public List<string> tempTagList = new List<string>();
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            Hediff_CultivationBase level = billDoer.FindAnyCultivationLevel();
            if (level != null)
            {
                if (modExtension.allowedCultivationLevel.Any(x => level.def.tags.Contains(x)))
                {
                    passRequirement = true;
                }
                else
                {
                    passRequirement = false;
                }
            }
            if (passRequirement)
            {
                if (modExtension.fixedIngredient)
                {                    
                    RecipeCombination combination = null;
                    tempList.Clear();
                    foreach (var item in ingredients)
                    {
                        tempList.Add(item.def);
                    }
                    if (!tempList.NullOrEmpty())
                    {
                        combination = modExtension.combinations.FirstOrDefault(x => tempList.Contains(x.firstThing) && tempList.Contains(x.secondThing));
                    }
                    if (combination != null)
                    {
                        if (recipe == CTR_DefOf.CTR_MakeAlchemy)
                        {
                            DoAlchemyRecipe(billDoer, level, true, combination);
                        }
                        if (recipe == CTR_DefOf.CTR_MakeTalisman)
                        {
                            DoTalismanRecipe(billDoer, level, true, combination);
                        }
                        if (recipe == CTR_DefOf.CTR_MakeAlchemy_Generic)
                        {
                            DoAlchemyGenericRecipe(billDoer, level, true, combination);
                        }
                    }
                    else
                    {
                        if (recipe == CTR_DefOf.CTR_MakeAlchemy)
                        {
                            DoAlchemyRecipe(billDoer, level, false);
                        }
                        if (recipe == CTR_DefOf.CTR_MakeTalisman)
                        {
                            DoTalismanRecipe(billDoer, level, false);
                        }
                        if (recipe == CTR_DefOf.CTR_MakeAlchemy_Generic)
                        {
                            DoAlchemyGenericRecipe(billDoer, level, false);
                        }
                    }
                }
                else
                {
                    RecipeCombination combination = null;
                    foreach (var comb in modExtension.combinations)
                    {
                        if (ingredients[0].HasTradeTag(comb.firstTag) && ingredients[1].HasTradeTag(comb.secondTag))
                        {
                            combination = comb;
                        }
                    }
                    if (combination != null)
                    {
                        if (recipe == CTR_DefOf.CTR_MakeAlchemy)
                        {
                            DoAlchemyRecipe(billDoer, level, true, combination);
                        }
                        if (recipe == CTR_DefOf.CTR_MakeTalisman)
                        {
                            DoTalismanRecipe(billDoer, level, true, combination);
                        }
                    }
                    else
                    {
                        if (recipe == CTR_DefOf.CTR_MakeAlchemy)
                        {
                            DoAlchemyRecipe(billDoer, level, false);
                        }
                        if (recipe == CTR_DefOf.CTR_MakeTalisman)
                        {
                            DoTalismanRecipe(billDoer, level,false);
                        }
                    }
                }

                
            }
            else
            {
                Messages.Message("mixing failed, user have insufficient cultivation", MessageTypeDefOf.NeutralEvent);

            }
        }

        public void DoTalismanRecipe(Pawn billDoer, Hediff_CultivationBase level, bool foundMatch, RecipeCombination resultThing = null)
        {
            if (foundMatch)
            {
                Thing newThing = ThingMaker.MakeThing(resultThing.result);
                bool flag = billDoer.InspirationDef == InspirationDefOf.Inspired_Creativity;
                int count = 0;
                if (modExtension.count != null)
                {
                    count = modExtension.count.RandomInRange;
                }
                else
                {
                    count = resultThing.count.RandomInRange;
                }
                if (count <= 0) count = 1;
                newThing.stackCount = count;
                CompItemGrade compItemGrade = newThing.TryGetComp<CompItemGrade>();
                if (compItemGrade != null)
                {
                    compItemGrade?.SetGrade(Cultivation_Utility.GenerateGradeCreatedByPawn(billDoer));
                }
                GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                Messages.Message("mixing success! " + billDoer.LabelShort + " made a " + newThing.LabelCap, MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Thing newThing = ThingMaker.MakeThing(modExtension.failedProduct);
                newThing.stackCount = modExtension.count.RandomInRange;
                GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                Messages.Message("mixing failed, no valid combination", MessageTypeDefOf.NeutralEvent);
            }
        }
        public void DoAlchemyGenericRecipe(Pawn billDoer, Hediff_CultivationBase level, bool foundMatch, RecipeCombination resultThing = null)
        {
            Thing workbench = GenRadial.RadialDistinctThingsAround(billDoer.Position, billDoer.MapHeld, 1.9f, true).
                    Where(x => x.def == CTR_DefOf.CTR_AlchemyFurnace_Basic || x.def == CTR_DefOf.CTR_AlchemyFurnace_Intermediate).FirstOrDefault();
            float furnaceQuality = workbench != null ? workbench.GetStatValue(CTR_DefOf.AlchemyFurnaceQuality) : 0f;
            float chance = billDoer.GetStatValue(CTR_DefOf.AlchemySuccessChance) * furnaceQuality;
            if (Rand.Chance(chance))
            {
                if (foundMatch)
                {
                    Thing newThing = ThingMaker.MakeThing(resultThing.result);
                    bool flag = billDoer.InspirationDef == InspirationDefOf.Inspired_Creativity;
                    int count = 0;
                    if (modExtension.count != null)
                    {
                        count = modExtension.count.RandomInRange;
                    }
                    else
                    {
                        count = resultThing.count.RandomInRange;
                    }
                    if (count <= 0) count = 1;
                    newThing.stackCount = count;
                    GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    Messages.Message("mixing success! " + billDoer.LabelShort + " made a " + newThing.LabelCap, MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Thing newThing = ThingMaker.MakeThing(modExtension.failedProduct);
                    newThing.stackCount = Rand.RangeInclusive(0,1);
                    GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    Messages.Message("mixing failed, no valid combination", MessageTypeDefOf.NeutralEvent);
                }
            }
            else
            {
                Messages.Message($"mixing failed,{billDoer.LabelShort} pass the requirement but didn't pass success check. {chance.ToStringPercent("0.00")}", MessageTypeDefOf.NeutralEvent);
            }
        }
        public void DoAlchemyRecipe(Pawn billDoer, Hediff_CultivationBase level, bool foundMatch, RecipeCombination resultThing = null)
        {
            Thing workbench = GenRadial.RadialDistinctThingsAround(billDoer.Position, billDoer.MapHeld, 1.9f, true).
                    Where(x => x.def == CTR_DefOf.CTR_AlchemyFurnace_Basic || x.def == CTR_DefOf.CTR_AlchemyFurnace_Intermediate).FirstOrDefault();
            float furnaceQuality = workbench != null ? workbench.GetStatValue(CTR_DefOf.AlchemyFurnaceQuality) : 0f;
            float chance = billDoer.GetStatValue(CTR_DefOf.AlchemySuccessChance) * furnaceQuality;
            if (Rand.Chance(chance))
            {
                if (foundMatch)
                {
                    Thing newThing = ThingMaker.MakeThing(resultThing.result);
                    bool flag = billDoer.InspirationDef == InspirationDefOf.Inspired_Creativity;
                    int count = 0;
                    if (modExtension.count != null)
                    {
                        count = modExtension.count.RandomInRange;
                    }
                    else
                    {
                        count = resultThing.count.RandomInRange;
                    }
                    newThing.stackCount = count;
                    if (newThing.def.tradeTags.Contains("CTR_Pill"))
                    {
                        newThing?.TryGetComp<CompPillGrade>()?.SetGrade(Cultivation_Utility.GeneratePillGradeCreatedByPawn(billDoer, level));
                    }
                    GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    Messages.Message("mixing success! " + billDoer.LabelShort + " made a " + newThing.LabelCap, MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Thing newThing = ThingMaker.MakeThing(modExtension.failedProduct);
                    newThing.stackCount = modExtension.count.RandomInRange;
                    GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    Messages.Message("mixing failed, no valid combination", MessageTypeDefOf.NeutralEvent);
                }
            }
            else
            {
                Messages.Message($"mixing failed,{billDoer.LabelShort} pass the requirement but didn't pass success check. {chance.ToStringPercent("0.00")}", MessageTypeDefOf.NeutralEvent);
            }
        }
        public static float GetAmountResult(int cultivationLevel, int min, int max, bool inspired)
        {
            float num = 0f;
            switch (cultivationLevel)
            {
                case 0:
                    num += 0.7f;
                    break;
                case 1:
                    num += 1.1f;
                    break;
                case 2:
                    num += 1.5f;
                    break;
                case 3:
                    num += 1.8f;
                    break;
                case 4:
                    num += 2f;
                    break;
                case 5:
                    num += 2.2f;
                    break;
                case 6:
                    num += 2.4f;
                    break;
                case 7:
                    num += 2.6f;
                    break;
                case 8:
                    num += 3.5f;
                    break;
                case 9:
                    num += 3.6f;
                    break;
                case 10:
                    num += 3.7f;
                    break;
                case 11:
                    num += 3.8f;
                    break;
                case 12:
                    num += 5.5f;
                    break;
                case 13:
                    num += 5.6f;
                    break;
                case 14:
                    num += 5.7f;
                    break;
                case 15:
                    num += 6.5f;
                    break;
                case 16:
                    num += 6.6f;
                    break;
                case 17:
                    num += 6.7f;
                    break;
                case 18:
                    num += 9.0f;
                    break;
                case 19:
                    num += 20f;
                    break;
            }
            int value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.8f);
            value = Mathf.Clamp(value, 0, max);
            if (value == max && Rand.Value < 0.5f)
            {
                value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.95f);
                value = Mathf.Clamp(value, 0, max);
            }
            if (inspired)
            {
                value += 2;
            }
            return value;
        }

        /*public void LegacyAlchemyMethod()
        {
            bool firstMatch = false;
            bool secondMatch = false;
            ThingDef result = null;
            foreach (var item in modExtension.combinations)
            {
                if (ingredients[0].def.tradeTags.Contains(item.firstTag))
                {
                    firstMatch = true;
                }
                if (ingredients[1].def.tradeTags.Contains(item.secondTag))
                {
                    secondMatch = true;
                }
                if (firstMatch && secondMatch)
                {
                    result = item.result;
                    break;
                }
                else
                {
                    firstMatch = false;
                    secondMatch = false;
                }

            }
            if (recipe == CTR_DefOf.CTR_MakeAlchemy)
            {
                float chance = billDoer.GetStatValue(CTR_DefOf.AlchemySuccessChance) * recipe.recipeUsers.FirstOrDefault().GetStatValueAbstract(CTR_DefOf.AlchemyFurnaceQuality);
                if (Rand.Chance(chance))
                {
                    if (firstMatch && secondMatch)
                    {
                        Thing newThing = ThingMaker.MakeThing(result);
                        bool flag = billDoer.InspirationDef == InspirationDefOf.Inspired_Creativity;
                        float count = GetAmountResult(Cultivation_Utility.realmListAll[level.def], modExtension.count.min, modExtension.count.max, flag);
                        newThing.stackCount = modExtension.count.RandomInRange;
                        GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        Messages.Message("mixing success! " + billDoer.LabelShort + " made a " + newThing.LabelCap, MessageTypeDefOf.PositiveEvent);
                    }
                    else
                    {
                        Thing newThing = ThingMaker.MakeThing(modExtension.failedProduct);
                        newThing.stackCount = modExtension.count.RandomInRange;
                        GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                        Messages.Message("mixing failed, no valid combination", MessageTypeDefOf.NeutralEvent);
                    }
                }
                else
                {
                    Messages.Message($"mixing failed,{billDoer.LabelShort} pass the requirement but didn't pass success check. {chance.ToStringPercent("0.00")}", MessageTypeDefOf.NeutralEvent);
                }
            }
            if (recipe == CTR_DefOf.CTR_MakeTalisman)
            {
                if (firstMatch && secondMatch)
                {
                    Thing newThing = ThingMaker.MakeThing(result);
                    bool flag = billDoer.InspirationDef == InspirationDefOf.Inspired_Creativity;
                    float count = GetAmountResult(Cultivation_Utility.realmListAll[level.def], modExtension.count.min, modExtension.count.max, flag);
                    newThing.stackCount = modExtension.count.RandomInRange;
                    CompItemGrade compItemGrade = newThing.TryGetComp<CompItemGrade>();
                    if (compItemGrade != null)
                    {
                        compItemGrade?.SetGrade(Cultivation_Utility.GenerateGradeCreatedByPawn(billDoer));
                    }
                    GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    Messages.Message("mixing success! " + billDoer.LabelShort + " made a " + newThing.LabelCap, MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Thing newThing = ThingMaker.MakeThing(modExtension.failedProduct);
                    newThing.stackCount = modExtension.count.RandomInRange;
                    GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    Messages.Message("mixing failed, no valid combination", MessageTypeDefOf.NeutralEvent);
                }
            }
        }*/
    }
}
