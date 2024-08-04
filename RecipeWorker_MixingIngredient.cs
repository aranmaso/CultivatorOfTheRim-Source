using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Verse.Noise;
using UnityEngine;
using VFECore;

namespace CultivatorOfTheRim
{
    public class RecipeWorker_MixingIngredient : RecipeWorker
    {
        private RecipeExtension_MixingIngredient modExtension => recipe.GetModExtension<RecipeExtension_MixingIngredient>();

        private bool passRequirement = false;        

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            Hediff_CultivationLevel level = Cultivation_Utility.FindCultivationLevel(billDoer);
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
                bool firstMatch = false;
                bool secondMatch = false;
                ThingDef result = null;
                foreach (var item in modExtension.combinations)
                {
                    if(ingredients[0].def.tradeTags.Contains(item.firstTag))
                    {
                        firstMatch = true;
                    }
                    if (ingredients[1].def.tradeTags.Contains(item.secondTag))
                    {
                        secondMatch = true;
                    }
                    if(firstMatch && secondMatch)
                    {
                        result = item.result; 
                        break;
                    }
                    else
                    {
                        firstMatch = false;
                        secondMatch = false;
                    }
                    /*foreach(var tag in ingredients[1].def.tradeTags)
                    {
                        if(item.requireTag.Contains(tag))
                        {
                            firstMatch = true;
                            break;
                        }
                    }*/
                    
                }
                if(recipe == CTR_DefOf.CTR_MakeAlchemy)
                {
                    if (Rand.Value < billDoer.GetStatValue(CTR_DefOf.AlchemySuccessChance) * recipe.recipeUsers.FirstOrDefault().GetStatValueAbstract(CTR_DefOf.AlchemyFurnaceQuality))
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
                        Messages.Message("mixing failed, alchemy success chance too low", MessageTypeDefOf.NeutralEvent);
                    }
                }                
                if(recipe == CTR_DefOf.CTR_MakeTalisman)
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

            }
            else
            {
                Messages.Message("mixing failed, user have insufficient cultivation", MessageTypeDefOf.NeutralEvent);

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

    }
}
