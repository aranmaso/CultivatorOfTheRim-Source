using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{

    [StaticConstructorOnStartup]
    public static class CTR_StartPatch
    {
        static CTR_StartPatch()
        {
            //new Harmony("FarmerJoe.CultivatorOfTheRim").PatchAll();
            //Log.Message("Finishing introducing cultivation to the rim");
            try
            {
                StartUpCollectionCached();
                Inject();
                ConfirmXmlPatchKey();
                if (CultivatorOfTheRimMod.settings.severityMultiplier != 1.00f)
                {
                    AlterCultivatorRequirement(CultivatorOfTheRimMod.settings.severityMultiplier);
                }
                if (CultivatorOfTheRimMod.settings.isNerfingWorkSpeed)
                {
                    AlterWorkSpeedBoost();
                }
                if (CultivatorOfTheRimMod.settings.isNerfingCultivatorIDM)
                {
                    AlterIDM();
                }
                if (CultivatorOfTheRimMod.settings.isNerfBodyCultivatorMeleeDamage)
                {
                    AlterMeleeDamageBody();
                }
                if (CultivatorOfTheRimMod.settings.globalStatMultiplier != 1.00f)
                {
                    AlterStatMultiplier();
                }
                if (!CultivatorOfTheRimMod.settings.isAllowWildSpiritPlantSpawn)
                {
                    SuppressWildSpiritPlantSpawn();
                }
                if (CultivatorOfTheRimMod.settings.isSpiritGrassCanSupportItSelf)
                {
                    SpiritGrassPatching();
                }
                if (!ModsConfig.IsActive("zomuro.itssorcery"))
                {
                    StatDefMinCheck();
                }
                AutoAddingStats();
            }
            catch(Exception ex)
            {
                Log.Error($"[CTR] Error in {ex}");
            }
            
        }
        private static void StatDefMinCheck()
        {
            IEnumerable<StatDef> list = DefDatabase<StatDef>.AllDefs.Where(CTRStatDefPredicate);
            foreach(StatDef item in list)
            {
                item.defaultBaseValue = 0.75f;
            }
        }
        private static void SuppressWildSpiritPlantSpawn()
        {
            //IEnumerable<ThingDef> list = DefDatabase<ThingDef>.AllDefs.Where(IsSpiritPlant);
            IEnumerable<BiomeDef> biomes = DefDatabase<BiomeDef>.AllDefs.ToList();
            foreach (var item in biomes)
            {
                IReadOnlyList<BiomePlantRecord> bpr = item.wildPlants.ToList();
                foreach (var item2 in bpr)
                {
                    if (item2.plant.modContentPack.PackageId == "aranmaho.xianxia")
                    {
                        item.wildPlants.Remove(item2);
                    }
                }
            }
            foreach (var item in DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.thingClass == typeof(Plant_SpiritPlant)))
            {
                if (item.plant.wildBiomes.NotNullOrEmpty())
                {
                    item.plant.wildBiomes.Clear();
                }
            }
           /* foreach(var item in list)
            {
                if(item.plant == null) continue;
                if(item.plant.wildBiomes.NullOrEmpty()) continue;
                item.plant.wildBiomes.Clear();
            }*/
        }

        private static void SpiritGrassPatching()
        {
            CTR_DefOf.CTR_SpiritGrassPlant.GetModExtension<PlantExtension_SpiritPlant>().excludedThing.Clear();
        }
        private static void AlterCultivatorRequirement(float num)
        {
            IEnumerable<CultivationHediffDef> list = StaticCollectionCached.CultivationHediffDefs;
            foreach(var def in list)
            {
                try
                {
                    def.maxSeverity *= num;
                    foreach(var item in def.stages)
                    {
                        item.minSeverity *= num;
                    }
                    if (def.cultivationStages != null && def.cultivationStages.NotNullOrEmpty())
                    {
                        foreach (var culStage in def.cultivationStages)
                        {
                            culStage.minSeverity *= num;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[CTR] Error Alter cultivation requirement for {def}. {ex}");
                }
                
            }
        }
        public static void AlterIDM()
        {
            IEnumerable<HediffDef> list = DefDatabase<HediffDef>.AllDefs.Where(AlterPredicate);
            foreach(var def in list)
            {
                foreach(var stage in def.stages)
                {
                    if(!stage.statOffsets.NullOrEmpty())
                    {
                        foreach (var offset in stage.statOffsets)
                        {
                            if (offset.stat == StatDefOf.IncomingDamageFactor)
                            {
                                offset.value = 0f;
                                //stage.statOffsets.Remove(offset);
                            }
                            else continue;
                        }
                    }
                    if(!stage.statFactors.NullOrEmpty())
                    {
                        foreach (var factor in stage.statFactors)
                        {
                            if (factor.stat == StatDefOf.IncomingDamageFactor)
                            {
                                factor.value = 1f;
                                //stage.statFactors.Remove(factor);
                            }
                            else continue;
                        }
                    }                    
                }
            }
        }
        public static void AutoAddingStats()
        {
            foreach (var item in StaticCollectionCached.CultivationHediffDefs)
            {
                if (item.hediffClass == typeof(Hediff_BodyCultivaton))
                {
                    if (item.realmPower >= 7)
                    {
                        foreach (var stage in item.stages)
                        {
                            StatModifier statMod = new StatModifier();
                            statMod.stat = StatDefOf.InjuryHealingFactor;
                            statMod.value = stage.naturalHealingFactor;
                            stage.statOffsets.Add(statMod);
                        }
                    }
                }
            }
        }
        public static void AlterWorkSpeedBoost()
        {
            //IEnumerable<HediffDef> list = DefDatabase<HediffDef>.AllDefs.Where(AlterPredicate);
            foreach (var def in StaticCollectionCached.CultivationHediffDefs)
            {
                try
                {
                    int level = def.realmPower;
                    foreach (var stage in def.stages)
                    {
                        if (level >= 6 && level < 13)
                        {
                            foreach (var statOffset in stage.statOffsets)
                            {
                                if (statOffset.stat == StatDefOf.WorkSpeedGlobal)
                                {
                                    statOffset.value -= 2.25f;
                                }
                            }
                        }
                        else if (level >= 13 && level < 18)
                        {
                            foreach (var statOffset in stage.statOffsets)
                            {
                                if (statOffset.stat == StatDefOf.WorkSpeedGlobal)
                                {
                                    statOffset.value -= 2.50f;
                                }
                            }
                        }
                        else
                        {
                            foreach (var statOffset in stage.statOffsets)
                            {
                                if (!stage.multiplyStatChangesBySeverity)
                                {
                                    continue;
                                }
                                if (statOffset.stat == StatDefOf.WorkSpeedGlobal)
                                {
                                    statOffset.value -= 0.03f;
                                }
                            }
                        }
                        /*foreach (var cap in stage.capMods)
                        {
                            if(level < 13)
                            {
                                if (cap.capacity == PawnCapacityDefOf.Manipulation)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.5f;
                                }
                                if (cap.capacity == PawnCapacityDefOf.Sight)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.5f;
                                }
                            }
                            else if(level >= 13 && level < 14)
                            {
                                if (cap.capacity == PawnCapacityDefOf.Manipulation)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.45f;
                                }
                                if (cap.capacity == PawnCapacityDefOf.Sight)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.45f;
                                }
                            }
                            else if(level >= 14 && level < 15)
                            {
                                if (cap.capacity == PawnCapacityDefOf.Manipulation)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.425f;
                                }
                                if (cap.capacity == PawnCapacityDefOf.Sight)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.425f;
                                }
                            }
                            else if(level >= 15 && level < 18)
                            {
                                if (cap.capacity == PawnCapacityDefOf.Manipulation)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.425f;
                                }
                                if (cap.capacity == PawnCapacityDefOf.Sight)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.425f;
                                }
                            }
                            else if (level >= 18)
                            {
                                if (cap.capacity == PawnCapacityDefOf.Manipulation)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.25f;
                                }
                                if (cap.capacity == PawnCapacityDefOf.Sight)
                                {
                                    float numTemp = cap.postFactor;
                                    cap.postFactor = 1f;
                                    cap.offset = (numTemp - 1f) * 0.25f;
                                }
                            }
                        }*/
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[CTR] Error patching workspeed for {def}. {ex}");
                }

            }
        }

        public static void AlterMeleeDamageBody()
        {
            foreach (var item in StaticCollectionCached.CultivationHediffDefs.Where(x => x.IsBodyCultivation()))
            {
                try
                {
                    foreach (var stage in item.stages)
                    {
                        foreach (var statOffset in stage.statOffsets)
                        {
                            if (statOffset.stat == StatDefOf.MeleeDamageFactor)
                            {
                                statOffset.value *= CultivatorOfTheRimMod.settings.MeleeDamageMultiplier;
                            }
                        }
                    }
                }
                catch(Exception ex) 
                {
                    Log.Error($"[CTR] Error patching melee damage for {item}. {ex}");
                }
            }
        }

        public static void AlterStatMultiplier()
        {
            float mul = CultivatorOfTheRimMod.settings.globalStatMultiplier;
            foreach (var item in StaticCollectionCached.CultivationHediffDefs)
            {
                try
                {
                    foreach (var stage in item.stages)
                    {
                        if (stage.statOffsets.NotNullOrEmpty())
                        {
                            foreach (var statOffset in stage.statOffsets)
                            {
                                statOffset.value *= mul;
                            }
                        }
                        if (stage.statFactors.NotNullOrEmpty())
                        {
                            foreach (var factor in stage.statFactors)
                            {
                                float num = factor.value;
                                num -= 1f;
                                num *= mul;
                                factor.value = 1f + num;
                            }
                        }
                        if (stage.capMods.NotNullOrEmpty())
                        {
                            foreach (var capMod in stage.capMods)
                            {
                                if (capMod.offset != 0)
                                {
                                    capMod.offset *= mul;
                                }
                                if (capMod.postFactor != 1)
                                {
                                    float num = capMod.postFactor;
                                    num -= 1f;
                                    num *= mul;
                                    capMod.postFactor = 1f + num;

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[CTR] Error patching stat multiplier for {item}. {ex}");
                }
            }
        }
        private static void ConfirmXmlPatchKey()
        {
            if (CultivatorOfTheRimMod.settings.settingKey.NullOrEmpty())
            {
                CultivatorOfTheRimMod.settings.settingKey = [];
            }

            if (CultivatorOfTheRimMod.settings.isAddingCultivationTraderToFactionCaravan)
            {
                if (!CultivatorOfTheRimMod.settings.settingKey.Contains("CTR.CaravanTraderKind"))
                {
                    CultivatorOfTheRimMod.settings.settingKey.AddDistinct("CTR.CaravanTraderKind");
                }
            }
            
            if (CultivatorOfTheRimMod.settings.isAddingCultivationTraderToFactionBase)
            {
                if (!CultivatorOfTheRimMod.settings.settingKey.Contains("CTR.BaseTraderKind"))
                {
                    CultivatorOfTheRimMod.settings.settingKey.AddDistinct("CTR.BaseTraderKind");
                }
            }

            if (CultivatorOfTheRimMod.settings.isSpiritPlantGrowAnywhere)
            {
                if (!CultivatorOfTheRimMod.settings.settingKey.Contains("CTR.SpiritPlantGrowAnywhere"))
                {
                    CultivatorOfTheRimMod.settings.settingKey.AddDistinct("CTR.SpiritPlantGrowAnywhere");
                }
            }
        }
        private static void Inject()
        {
            List<ThingDef> list = DefDatabase<ThingDef>.AllDefs.Where(InjectPredicate).ToList();
            CompProperties comp = new CompProperties
            {
                compClass = typeof(CompItemGrade)
            };
            foreach (var item in list)
            {
                item.comps.Add(comp);
            }
            //Log.Message("Add Grade to item complete");
            List<ThingDef> pillList = DefDatabase<ThingDef>.AllDefs.Where(InjectPredicatePill).ToList();
            CompProperties compPill = new CompProperties
            {
                compClass = typeof(CompPillGrade)
            };
            foreach (var item in pillList)
            {
                item.comps.AddDistinct(compPill);
            }
            //Log.Message("Add Grade to Pill complete");

            //inject Qi Type to item
            /*foreach(var item in DefDatabase<ThingDef>.AllDefsListForReading.Where(InjectQiType))
            {
                if(item.category == ThingCategory.Item)
                {
                    if (item.modContentPack != null && item.modContentPack?.PackageId == "aranmaho.xianxia") continue;

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(item.description);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    string tags = null;
                    if (item.IsStuff)
                    {
                        if (!item.stuffProps.categories.NullOrEmpty())
                        {
                            bool hasQiTag = false;
                            if (item.stuffProps.categories.Any(ShouldHaveQiTags))
                            {
                                hasQiTag = true;
                            }
                            if (hasQiTag)
                            {                                                                
                                foreach (var category in item.stuffProps.categories)
                                {
                                    if (category == CTR_DefOf.Metallic)
                                    {
                                        if (item.tradeTags == null)
                                        {
                                            item.tradeTags = new List<string>();
                                        }
                                        item.tradeTags.Add("Metal_Qi");
                                        tags += "Metal_Qi".Colorize(Color.yellow);
                                        tags += ",";
                                    }
                                    if (category == CTR_DefOf.Stony)
                                    {
                                        if (item.tradeTags == null)
                                        {
                                            item.tradeTags = new List<string>();
                                        }
                                        item.tradeTags.Add("Earth_Qi");
                                        tags += "Earth_Qi".Colorize(new Color(0.54f, 0.27f, 0.07f));
                                        tags += ",";
                                    }
                                    if (category == CTR_DefOf.Woody)
                                    {
                                        if (item.tradeTags == null)
                                        {
                                            item.tradeTags = new List<string>();
                                        }
                                        item.tradeTags.Add("Wood_Qi");
                                        tags += "Wood_Qi".Colorize(Color.green);
                                        tags += ",";
                                    }
                                }                                                               
                            }                            
                        }                        
                    }
                    if(!item.thingCategories.NullOrEmpty() && item.thingCategories.Contains(ThingCategoryDefOf.StoneChunks))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Earth_Qi");
                        tags += "Earth_Qi".Colorize(new Color(0.54f, 0.27f, 0.07f));
                        tags += ",";
                    }
                    if(item.thingClass == typeof(Medicine) || item.thingClass.IsSubclassOf(typeof(Medicine)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Healing_Item");
                        tags += "Healing_Item".Colorize(Color.green);
                        tags += ",";
                    }
                    IEnumerable<string> fire = new List<string>()
                    {
                        "fire","hot","heat","star","sun","flame"
                    };
                    if (fire.Any(x => item.label.ToLower().Contains(x)))
                                {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Fire_Qi");
                        tags += "Fire_Qi".Colorize(Color.red);
                        tags += ",";
                    }

                    IEnumerable<string> icy = new List<string>()
                    {
                        "ice","frozen","freeze","cold","frigid","water","icy","frost"
                    };
                    if (icy.Any(x => item.label.ToLower().Contains(x)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Water_Qi");
                        tags += "Water_Qi".Colorize(Color.blue);
                        tags += ",";
                    }
                    IEnumerable<string> yinCold = new List<string>()
                    {
                        "death","soul"
                    };
                    if(yinCold.Any(x => item.label.ToLower().Contains(x)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Cold_Qi");
                        item.tradeTags.Add("Yin_Qi");
                        tags += "Cold_Qi".Colorize(Color.cyan);
                        tags += ",";
                        tags += "Yin_Qi".Colorize(Color.cyan);
                        tags += ",";
                    }
                    IEnumerable<string> celestial = new List<string>()
                    {
                        "celestial","star","divine","holy"
                    };
                    if(celestial.Any(x => item.label.ToLower().Contains(x)))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Immortal_Qi");
                        tags += "Immortal_Qi".Colorize(Color.cyan);
                        tags += ",";
                    }
                    if(item.label.ToLower().Contains("existence"))
                    {
                        if (item.tradeTags == null)
                        {
                            item.tradeTags = new List<string>();
                        }
                        item.tradeTags.Add("Qi_Source");
                        item.tradeTags.Add("Qi_Source_Tier6");
                        item.tradeTags.Add("Unlimited_Source");
                        item.tradeTags.Add("Neutral_Qi");
                        item.tradeTags.Add("Cold_Qi");
                        item.tradeTags.Add("Yin_Qi");
                        item.tradeTags.Add("Yang_Qi");
                        item.tradeTags.Add("Fire_Qi");
                        item.tradeTags.Add("Water_Qi");
                        item.tradeTags.Add("Wood_Qi");
                        item.tradeTags.Add("Earth_Qi");
                        item.tradeTags.Add("Metal_Qi");
                        item.tradeTags.Add("Pure_Qi");
                        item.tradeTags.Add("Saint_Qi");
                        item.tradeTags.Add("Heavenly_Qi");
                        item.tradeTags.Add("Immortal_Qi");
                        foreach (var tradeTags in item.tradeTags)
                        {
                            tags += tradeTags;
                            tags += ",";
                            tags += "\n";
                        }
                        
                    }
                    if(tags != null)
                    {
                        stringBuilder.AppendLine("Tags: " + tags);
                        item.description = stringBuilder.ToString().TrimEndNewlines();
                    }                    
                }
                else
                {
                    continue;
                }
            }
            static bool ShouldHaveQiTags(StuffCategoryDef stuffDef)
            {
                if(stuffDef == CTR_DefOf.Metallic) return true;
                if(stuffDef == CTR_DefOf.Woody) return true;
                if(stuffDef == CTR_DefOf.Stony) return true;
                return false;
            }*/

            //add recipe info to mixing
            foreach (var item in DefDatabase<RecipeDef>.AllDefsListForReading.Where(x => x == CTR_DefOf.CTR_MakeAlchemy || x == CTR_DefOf.CTR_MakeTalisman || x == CTR_DefOf.CTR_MakeAlchemy_Generic))
            {
                if(item == CTR_DefOf.CTR_MakeAlchemy)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(item.description);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("recipe list");
                    stringBuilder.AppendLine();
                    foreach (var recipe in item.GetModExtension<RecipeExtension_MixingIngredient>().combinations)
                    {
                        stringBuilder.AppendLine("first tag: ".Colorize(Color.green) + recipe.firstTag);
                        stringBuilder.AppendLine("second tag: ".Colorize(Color.green) + recipe.secondTag);
                        stringBuilder.AppendLine("result: ".Colorize(Color.green) + recipe.result.label);
                        stringBuilder.AppendLine();
                    }
                    item.description = stringBuilder.ToString().TrimEndNewlines();
                }
                else if(item == CTR_DefOf.CTR_MakeTalisman)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(item.description);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("recipe list");
                    stringBuilder.AppendLine();
                    foreach (var recipe in item.GetModExtension<RecipeExtension_MixingIngredient>().combinations)
                    {
                        stringBuilder.AppendLine("first tag: ".Colorize(Color.green) + recipe.firstTag);
                        stringBuilder.AppendLine("second tag: ".Colorize(Color.green) + recipe.secondTag);
                        stringBuilder.AppendLine("result: ".Colorize(Color.green) + recipe.result.label);
                        stringBuilder.AppendLine();
                    }
                    item.description = stringBuilder.ToString().TrimEndNewlines();
                }
                else if (item == CTR_DefOf.CTR_MakeAlchemy_Generic)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(item.description);
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine("recipe list");
                    stringBuilder.AppendLine();
                    foreach (var recipe in item.GetModExtension<RecipeExtension_MixingIngredient>().combinations)
                    {
                        stringBuilder.AppendLine("first thing: ".Colorize(Color.green) + recipe.firstThing.LabelCap);
                        stringBuilder.AppendLine("second thing: ".Colorize(Color.green) + recipe.secondThing.LabelCap);
                        stringBuilder.AppendLine("result: ".Colorize(Color.green) + recipe.result.label);
                        stringBuilder.AppendLine();
                    }
                    item.description = stringBuilder.ToString().TrimEndNewlines();
                }
            }

            
        }
        private static bool CTRStatDefPredicate(StatDef def)
        {
            if(def.defName == "AlchemySuccessChance")
            {
                return true;
            }
            if(def.defName == "RefiningSuccessChance")
            {
                return true;
            }
            return false;
        }

        private static bool AlterPredicate(HediffDef def)
        {
            /*if (def.hediffClass == typeof(Hediff_CultivationLevel))
            {
                return true;
            }*/
            if (def.tags.NullOrEmpty())
            {
                return false;
            }
            if (!def.tags.Contains("CTR_Realm"))
            {
                return false;
            }
            return true;
        }
        private static bool IsSpiritPlant(ThingDef plantDef)
        {
            if(plantDef.tradeTags.NullOrEmpty())
            {
                return false;
            }
            if(!plantDef.tradeTags.Contains("Spirit_Plant"))
            {
                return false;
            }
            return true;
        }
        private static bool FactionPredicate(FactionDef def)
        {
            if(!def.CanEverBeNonHostile)
            {
                return false;
            }
            return true;
        }

        private static bool InjectQiType(ThingDef def)
        {
            if(def.category == ThingCategory.Item)
            {
                return true;
            }
            return false;
        }
        private static bool InjectPredicate(ThingDef def)
        {
            if (def.BaseMarketValue <= 0)
            {
                return false;
            }
            if (def.HasComp(typeof(CompEquippable)) || def.HasComp(typeof(CompEquippableAbilityReloadable)) || def.HasComp(typeof(CompEquippableAbility)))
            {
                return true;
            }
            if (def.thingClass != null)
            {
                if (def.thingClass == typeof(Apparel) || def.thingClass.IsSubclassOf(typeof(Apparel)))
                {
                    return true;
                }
            }            
            /*if (!def.HasComp(typeof(CompQuality)))
            {
                return false;
            }*/
            /*if (def.Verbs.Any((VerbProperties v) => typeof(Verb_ShootOneUse).IsAssignableFrom(v.GetType())))
            {
                return false;
            }*/            
            return false;
        }

        private static bool InjectPredicatePill(ThingDef def)
        {
            if (!def.HasComp(typeof(CompDrug)))
            {
                return false;
            }
            if(def.tradeTags.NullOrEmpty() || !def.tradeTags.Contains("CTR_Pill"))
            {
                return false;
            }
            /*if(!def.tradeTags.Contains("Cultivation_Pill"))
            {
                return false;
            }*/
            if (def.BaseMarketValue <= 0)
            {
                return false;
            }
            return true;
        }

        public static void StartUpCollectionCached()
        {
            StaticCollectionCached.FactionCultivationDef.AddRange(DefDatabase<FactionCultivationDef>.AllDefsListForReading);
            StaticCollectionCached.CultivationHediffDefs.AddRange(DefDatabase<CultivationHediffDef>.AllDefsListForReading);
            foreach (var item in StaticCollectionCached.CultivationHediffDefs)
            {
                StaticCollectionCached.CultivationRealmPower.Add(item, item.realmPower);
                StaticCollectionCached.CultivationRealmWeight.Add(item, item.realmWeight);
            }
        }
    }

}
