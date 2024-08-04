using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using Verse.Noise;
using UnityEngine;
using VFECore;

namespace CultivatorOfTheRim
{
    public class RecipeWorker_Alchemy : RecipeWorker
    {
        private RecipeExtension_Alchemy modExtension => recipe.GetModExtension<RecipeExtension_Alchemy>();

        private bool passRequirement = false;
        /*public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            Pawn billdoer = null;            
            foreach(var item in Cultivation_Utility.GetNearbyPawnFriendAndFoe(ingredient.Position,map,1))
            {
                if(item.Faction.IsPlayer)
                {
                    billdoer = item;
                    break;
                }
                else
                {
                    continue;
                }
            }
            Hediff_CultivationLevel level = Cultivation_Utility.FindCultivationLevel(billdoer);
            if(level != null)
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
                Log.Message("A");
                Messages.Message("pill making success, " + billdoer.LabelShort + " has successfully making " + modExtension.successfulProduct.label + " with cultivation of " + level.Label + " " + level.CurStage.label, MessageTypeDefOf.NeutralEvent);
                Thing newThing = ThingMaker.MakeThing(modExtension.successfulProduct ?? recipe.ProducedThingDef);
                //newThing.TryGetComp<CompPillGrade>().SetGrade(Cultivation_Utility.GeneratePillGradeCreatedByPawn(billdoer));
                newThing.stackCount = modExtension.count;                
                GenPlace.TryPlaceThing(newThing, billdoer.Position,map,ThingPlaceMode.Near);
                
            }
            else
            {
                Messages.Message("pill making failed, alchemist is either lack in experience or have insufficient cultivation",MessageTypeDefOf.NeutralEvent);
                Thing newThing = ThingMaker.MakeThing(modExtension.failedProduct);
                newThing.stackCount = modExtension.count;
                GenPlace.TryPlaceThing(newThing, billdoer.Position, map, ThingPlaceMode.Near);
                
            }
            base.ConsumeIngredient(ingredient, recipe, map);
        }*/

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
                if(Rand.Value < billDoer.GetStatValue(CTR_DefOf.AlchemySuccessChance) * recipe.recipeUsers.FirstOrDefault().GetStatValueAbstract(CTR_DefOf.AlchemyFurnaceQuality))
                {
                    Thing newThing = ThingMaker.MakeThing(modExtension.successfulProduct ?? recipe.ProducedThingDef);
                    bool flag = billDoer.InspirationDef == InspirationDefOf.Inspired_Creativity;
                    float count = DoAlchemyAmountResult(Cultivation_Utility.realmListAll[level.def], modExtension.count.min, modExtension.count.max, flag);
                    newThing.stackCount = Mathf.FloorToInt(count);
                    GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                    Messages.Message("pill making success, " + billDoer.LabelShort + " has successfully making " + modExtension.successfulProduct.label + " with cultivation of " + level.def.label + " " + level.CurStage.label + " " + billDoer.LabelShort + " has made " + count + " of " + modExtension.successfulProduct.label, MessageTypeDefOf.NeutralEvent);
                    if (newThing.def.tradeTags.Contains("CTR_Pill"))
                    {
                        newThing?.TryGetComp<CompPillGrade>()?.SetGrade(Cultivation_Utility.GeneratePillGradeCreatedByPawn(billDoer, level));
                    }
                }
                else
                {
                    if(Rand.Value <= 0.01f)
                    {
                        Messages.Message(billDoer.LabelCap + " has fucked up in a serious way, and cause a small explosion", MessageTypeDefOf.NegativeEvent);
                        GenExplosion.DoExplosion(billDoer.Position,billDoer.Map,2f,CTR_DefOf.CTR_Qi_Injury_Explosion,null,5);
                    }
                    else
                    {
                        Messages.Message(billDoer.LabelCap + " has sufficient cultivation but failed in doing alchemy due to insufficient skill", MessageTypeDefOf.NeutralEvent);
                    }
                    
                }
                

            }
            else
            {
                Messages.Message("pill making failed, alchemist is either lack in experience or have insufficient cultivation", MessageTypeDefOf.NeutralEvent);
                Thing newThing = ThingMaker.MakeThing(modExtension.failedProduct);
                newThing.stackCount = modExtension.count.RandomInRange;
                GenPlace.TryPlaceThing(newThing, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);

            }
        }

        public static float DoAlchemyAmountResult(int cultivationLevel,int min,int max,bool inspired)
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
