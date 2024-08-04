using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CultivatorOfTheRim
{

    [StaticConstructorOnStartup]
    public static class CTR_StartPatch
    {
        static CTR_StartPatch()
        {
            new Harmony("FarmerJoe.CultivatorOfTheRim").PatchAll();
            Log.Message("Finishing introducing cultivation to the rim");
            Inject();
            if(CultivatorOfTheRimMod.settings.severityMultiplier != 1.00f)
            {
                AlterCultivatorRequirement(CultivatorOfTheRimMod.settings.severityMultiplier);
            }
            if(CultivatorOfTheRimMod.settings.isNerfingWorkSpeed)
            {
                AlterWorkSpeedBoost();
            }
            if(CultivatorOfTheRimMod.settings.isNerfingCultivatorIDM)
            {
                AlterIDM();
            }
            if(CultivatorOfTheRimMod.settings.isAddingCultivationTraderToFactionCaravan)
            {
                GiveFactionTrader(CultivatorOfTheRimMod.settings.isAddingCultivationTraderToFactionBase);
            }
            if(!CultivatorOfTheRimMod.settings.isAllowWildSpiritPlantSpawn)
            {
                SuppressWildSpiritPlantSpawn();
            }
            if(!ModsConfig.IsActive("zomuro.itssorcery"))
            {
                StatDefMinCheck();
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
            IEnumerable<ThingDef> list = DefDatabase<ThingDef>.AllDefs.Where(IsSpiritPlant);
            foreach(var item in list)
            {
                if(item.plant == null) continue;
                if(item.plant.wildBiomes.NullOrEmpty()) continue;
                item.plant.wildBiomes.Clear();
            }
        }
        private static void AlterCultivatorRequirement(float num)
        {
            IEnumerable<HediffDef> list = DefDatabase<HediffDef>.AllDefs.Where(AlterPredicate);
            foreach(HediffDef def in list)
            {
                def.maxSeverity *= num;
                foreach(var item in def.stages)
                {
                    item.minSeverity *= num;
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
        public static void AlterWorkSpeedBoost()
        {
            IEnumerable<HediffDef> list = DefDatabase<HediffDef>.AllDefs.Where(AlterPredicate);
            foreach (HediffDef def in list)
            {
                int level = Cultivation_Utility.realmListAll[def];
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
                    else if(level >= 13 && level < 18)
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
                            if(!stage.multiplyStatChangesBySeverity)
                            {
                                continue;
                            }
                            if (statOffset.stat == StatDefOf.WorkSpeedGlobal)
                            {
                                statOffset.value -= 0.03f;
                            }
                        }
                    }
                    foreach (var cap in stage.capMods)
                    {
                        if(level < 13)
                        {
                            if (cap.capacity == PawnCapacityDefOf.Manipulation)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.5f;
                            }
                            if (cap.capacity == PawnCapacityDefOf.Sight)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.5f;
                            }
                        }
                        else if(level >= 13 && level < 14)
                        {
                            if (cap.capacity == PawnCapacityDefOf.Manipulation)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.45f;
                            }
                            if (cap.capacity == PawnCapacityDefOf.Sight)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.45f;
                            }
                        }
                        else if(level >= 14 && level < 15)
                        {
                            if (cap.capacity == PawnCapacityDefOf.Manipulation)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.425f;
                            }
                            if (cap.capacity == PawnCapacityDefOf.Sight)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.425f;
                            }
                        }
                        else if(level >= 15 && level < 18)
                        {
                            if (cap.capacity == PawnCapacityDefOf.Manipulation)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.425f;
                            }
                            if (cap.capacity == PawnCapacityDefOf.Sight)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.425f;
                            }
                        }
                        else if (level >= 18)
                        {
                            if (cap.capacity == PawnCapacityDefOf.Manipulation)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.25f;
                            }
                            if (cap.capacity == PawnCapacityDefOf.Sight)
                            {
                                float numTemp = cap.postFactor;
                                cap.postFactor = 1f;
                                cap.offset = numTemp * 0.25f;
                            }
                        }
                    }
                }
                
                
            }
        }
        private static void GiveFactionTrader(bool alsoGiveToFactionBase)
        {
            IEnumerable<FactionDef> list = DefDatabase<FactionDef>.AllDefs.Where(FactionPredicate);
            foreach(var item in list)
            {
                item.caravanTraderKinds.Add(CTR_DefOf.Caravan_CultivationResource);
                item.caravanTraderKinds.Add(CTR_DefOf.Caravan_CultivationPill);
                item.caravanTraderKinds.Add(CTR_DefOf.Caravan_CultivationTechnique);

                if(alsoGiveToFactionBase)
                {
                    item.baseTraderKinds.Add(CTR_DefOf.Caravan_CultivationResource);
                    item.baseTraderKinds.Add(CTR_DefOf.Caravan_CultivationPill);
                    item.baseTraderKinds.Add(CTR_DefOf.Caravan_CultivationTechnique);
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
                if (item.thingClass == typeof(Apparel) || item.thingClass.IsSubclassOf(typeof(Apparel)) || item.HasComp(typeof(CompEquippable)))
                {
                    item.comps.Add(comp);
                }
            }
            Log.Message("Add Grade to item complete");
            List<ThingDef> pillList = DefDatabase<ThingDef>.AllDefs.Where(InjectPredicatePill).ToList();
            CompProperties compPill = new CompProperties
            {
                compClass = typeof(CompPillGrade)
            };
            foreach (var item in pillList)
            {
                item.comps.AddDistinct(compPill);
            }
            Log.Message("Add Grade to Pill complete");
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
            if(def.tags.NullOrEmpty())
            {
                return false;
            }
            if(!def.tags.Contains("CTR_Realm"))
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

        private static bool InjectPredicate(ThingDef def)
        {
            if(def.HasComp(typeof(CompEquippable)))
            {
                return true;
            }
            if (!def.HasComp(typeof(CompQuality)))
            {
                return false;
            }
            /*if (def.Verbs.Any((VerbProperties v) => typeof(Verb_ShootOneUse).IsAssignableFrom(v.GetType())))
            {
                return false;
            }*/
            if (def.BaseMarketValue <= 0)
            {
                return false;
            }
            return true;
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
    }

}
