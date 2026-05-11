using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace CultivatorOfTheRim
{
    public class CultivatorOfTheRimSetting : ModSettings
    {        
        public bool isShowingCultivateExpText = false;

        public bool isPlayingTickingSound = true;

        public bool isWildAnimalAutoCultivate = false;

        public bool isOnlyColonyAnimalAutoCultivate = true;

        public bool isWildAnimalAutoBreakthrought = false;

        public bool isColonyAnimalAutoBreakthrough = false;

        public bool isWildAnimalIgnoreSafetyThresholdForBreakthrough = false;

        public bool isColonyAnimalIgnoreSafetyThresholdForBreakthrough = false;

        public bool isAnimalDropBeastCore = true;

        public bool isCulSpeedAffectedByEnviaronment = true;

        public bool isNeedCapped = true;

        public bool isTribulationChangeWeather = true;

        public float severityMultiplier = 1.00f;
        public string severityMultiplierString = "1.00";

        public float tribulationSafety;
        public string tribulationSafetyString;

        public float tribRemnantChance = 0.05f;
        public string tribRemnantChanceString = "0.05";

        public bool isCultivationAffectBodyHP = false;

        public bool isCultivatorNeedQiSourceToImprove = false;

        public bool isAddingCultivationTraderToFactionCaravan = true;
        public bool isAddingCultivationTraderToFactionBase = true;

        public bool isCultivatorNeedHighTierQi = false;

        public bool isBreakthroughCanFailForHumanlike = false;

        public bool isArmorGradeStackMultiplicatively = false; 

        public bool isSpiritPlantRestrictedByCultivationLevel = false;

        public float breakthroughSuccessOverallModifier = 1.00f;
        public string breakthroughSuccessOverallModifierString = "1.00";

        public bool isFertilityFormationAffectSpiritPlant = false;

        public bool isNerfingWorkSpeed = false;

        public bool isFertilityFormationAgePawn = true;

        public bool isNerfingCultivatorIDM = false;

        public bool isAllowWildSpiritPlantSpawn = true;

        public bool isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal = true;

        public int realmDifferentLimit = 3;

        public bool isWildItemSpawnWithGrade;

        public float animalCultivationSpeedMultiplier;
        
        public string animalCultivationSpeedMultiplierBuffer;

        public bool isDeAgingPawn;

        public bool isUsingQuadrum;

        public bool isSpawnQiFleck = true;

        public float globalQiTypeMultiplier = 1f;

        public string globalQiTypeMultiplierString = "1.00";

        public bool isSpiritPlantDamageSource = true;

        public bool isShowDamageModifierGrade = true;

        public bool isNerfBodyCultivatorMeleeDamage = false;

        public float MeleeDamageMultiplier = 1.0f;

        public float globalStatMultiplier = 1.0f;

        public bool isSpiritGrassCanSupportItSelf = true;

        public bool isBodyCulGainXpThroughJob = true;

        public bool isYinYangBackgroundRotate = true;

        public bool isSpiritPlantGrowAnywhere = false;

        public bool isBodyCulCanUseWeapon = false;

        public bool isBodyCulCanUseBionic = false;

        private Vector2 infoScroll;

        private float lastHeight;

        public List<string> settingKey = new List<string>();

        private enum Tab
        {
            GeneralSetting,
            DifficultySetting,
            MiscSetting
        }

        private Tab tab;
        public void DoSettingsWindowContents(Rect inRect)
        {
            tabList.Clear();
            tabList.Add(new TabRecord("General", delegate ()
            {
                tab = Tab.GeneralSetting;
            },tab == Tab.GeneralSetting));
            tabList.Add(new TabRecord("Difficulty", delegate ()
            {
                tab = Tab.DifficultySetting;
            },tab == Tab.DifficultySetting));
            tabList.Add(new TabRecord("Misc", delegate ()
            {
                tab = Tab.MiscSetting;
            },tab == Tab.MiscSetting));
            Rect tabRect = new Rect(inRect);
            tabRect.yMin = 80;
            TabDrawer.DrawTabs<TabRecord>(tabRect, tabList, 200);

            Rect leftThird = new Rect(tabRect);
            //leftThird.width = inRect.width / 2.1f;
            /*Rect otherTwoThird = new Rect(leftThird);
            otherTwoThird.x += 500;*/

            //otherTwoThird.xMin += tabRect.width / 3;
            Rect viewRect = new Rect(0f, 0f, leftThird.width - 25f, leftThird.height + lastHeight);
            //Widgets.BeginScrollView(leftThird, ref infoScroll, viewRect);
            var listing = new Listing_Standard();
            listing.Begin(leftThird);
            listing.Label("some setting may need restart".Colorize(Color.red) + " ver. " + "04.05.26.1");
            listing.Gap(8f);
            listing.ColumnWidth = (leftThird.width / 2f) - 25;
            

            switch (tab)
            {
                case Tab.GeneralSetting
                    :
                    GeneralSetting(ref listing);
                    break;

                case Tab.DifficultySetting:
                    DifficultySetting(ref listing);
                    break;

                case Tab.MiscSetting:
                    MiscSetting(ref listing);
                    break;
                default: break;
            }

            //Widgets.EndScrollView();

            /*if (listing.ButtonText("Reset to global default"))
            {
                GeneralDefault();
                DifficultyDefault();
                MiscDefault();
            }*/


            listing.End();

            /*var listing2 = new Listing_Standard();
            listing2.Begin(otherTwoThird);
            listing2.Gap(8f);

            switch (tab)
            {
                case Tab.GeneralSetting
                :
                    *//*GeneralSetting(ref listing2);*//*
                    break;

                case Tab.DifficultySetting:
                    DifficultySetting_PageTwo(ref listing2);
                    break;

                case Tab.MiscSetting:
                    *//*MiscSetting(ref listing2);*//*
                    break;
                default: break;
            }

            listing2.End();*/
        }
        public void GeneralSetting(ref Listing_Standard listing_Standard)
        {
            float num = 5f;
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("showing exp text", ref isShowingCultivateExpText, "if ON. when pawn meditate, a green text will show severity they gain per trigger");
            listing_Standard.CheckboxLabeled("Chrono Stele and Beacon ticking sound", ref isPlayingTickingSound);
            listing_Standard.CheckboxLabeled("animal auto cultivate", ref isWildAnimalAutoCultivate, "only affect those that spawn with cultivation");
            listing_Standard.CheckboxLabeled("only colony animal auto cultivate", ref isOnlyColonyAnimalAutoCultivate, "only colony animal can auto cultivate");
            listing_Standard.CheckboxLabeled("wild animal auto breakthrough", ref isWildAnimalAutoBreakthrought);
            if (isWildAnimalAutoBreakthrought)
            {
                listing_Standard.CheckboxLabeled("wild animal obey safety threshold: " + tribulationSafety * 100 + "%", ref isWildAnimalIgnoreSafetyThresholdForBreakthrough,
                    "if On. animal will wait until Tribulation chance lowered to the minimum before attempting another breakthrough " +
                    "\nif Off. animal will attempting breakthrough ignoring their safety.");
                if (isWildAnimalIgnoreSafetyThresholdForBreakthrough)
                {
                    tribulationSafetyString = tribulationSafety.ToString("0.00");
                    listing_Standard.TextFieldNumeric(ref tribulationSafety, ref tribulationSafetyString, 0.00f, 1.00f);
                }
            }
            listing_Standard.CheckboxLabeled("colony animal auto breakthrough", ref isColonyAnimalAutoBreakthrough);
            if (isColonyAnimalAutoBreakthrough)
            {
                listing_Standard.CheckboxLabeled("colony animal obey safety rule: " + tribulationSafety * 100 + "%",ref isColonyAnimalIgnoreSafetyThresholdForBreakthrough, "if On. animal will wait until Tribulation chance lowered to the minimum before attempting another breakthrough " +
                    "\nif Off. animal will attempting breakthrough ignoring their safety.");
                if (isColonyAnimalIgnoreSafetyThresholdForBreakthrough)
                {
                    tribulationSafetyString = tribulationSafety.ToString("0.00");
                    listing_Standard.TextFieldNumeric(ref tribulationSafety, ref tribulationSafetyString, 0.00f, 1.00f);
                }
            }
            listing_Standard.CheckboxLabeled("is allowing spirit plant wild spawn",ref isAllowWildSpiritPlantSpawn,"allow spirit plant to randomly spawn in the wild");
            listing_Standard.CheckboxLabeled("spirit grass can support itself",ref isSpiritGrassCanSupportItSelf,"if ON Spirit grass doesn't require external qi source");
            listing_Standard.CheckboxLabeled("is spirit plant damage qi source", ref isSpiritPlantDamageSource,"turn this off if it affect performance");
            listing_Standard.Label("animal cultivation speed multiplier");
            listing_Standard.TextFieldNumeric(ref animalCultivationSpeedMultiplier, ref animalCultivationSpeedMultiplierBuffer, 0.01f);
            listing_Standard.CheckboxLabeled("is de-aging pawn to 21",ref isDeAgingPawn,"from Golden Core and onward, deaging pawn to 21 biologically");
            listing_Standard.CheckboxLabeled("seasonal spirit plant using Quadrum",ref isUsingQuadrum,"if On: seasonal spirit plant that require specific season will use current quadrum for season instead of actual season, this mean a map with permanent summer will still able to growth seasonal plant." +
                "\nif Off: default behavior, seasonal spirit plant will use current season for growth check. this mean map with permanent summer will never be able to grow certain plant.");
            listing_Standard.Gap(8f);
            lastHeight = num;
            if (listing_Standard.ButtonText("Reset General to default"))
            {
                GeneralDefault();
            }

        }
        public void DifficultySetting(ref Listing_Standard listing_Standard)
        {
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("wild animal drop beast core", ref isAnimalDropBeastCore);
            listing_Standard.CheckboxLabeled("pawn cultivation speed are affected by surrounding item", ref isCulSpeedAffectedByEnviaronment);
            listing_Standard.CheckboxLabeled("food and rest cap", ref isNeedCapped, "if On. pawn of golden core or above will restore their food and rest need to 75% every 1 hour");
            listing_Standard.CheckboxLabeled("Heavenly Tribulation change weather", ref isTribulationChangeWeather, "Rainy Thunderstorm count as a threat by storyteller. a lot of pawn/animal take turn summoning heavenly tribulation can prevent raid from spawning");
            listing_Standard.Label("threshold multiplier for cultivation xp required to reach peak(default 1.00)");
            listing_Standard.Label("*require restart");
            severityMultiplierString = severityMultiplier.ToString(".00");
            listing_Standard.TextFieldNumeric(ref severityMultiplier, ref severityMultiplierString, 0.01f, 200f);
            listing_Standard.CheckboxLabeled("Cultivation require Qi source", ref isCultivatorNeedQiSourceToImprove,
                "if On: Pawn can't gain cultivation EXP without a Qi source in range." +
                "\nif Off: Pawn can still gain a certain amount of exp without Qi source in range." +
                "\nonly affect Cultivation in Qi Gathering stage and above");
            listing_Standard.CheckboxLabeled("Cultivator of higher realm need corresponding Qi source tier to cultivate", ref isCultivatorNeedHighTierQi);
            listing_Standard.Label("Chance for tribulation remnant to spawn after lightning strike");
            tribRemnantChanceString = tribRemnantChance.ToString("0.00");
            listing_Standard.TextFieldNumeric(ref tribRemnantChance,ref tribRemnantChanceString,0.00f,1.00f);
            listing_Standard.CheckboxLabeled("can humanlike breakthrough fail",ref isBreakthroughCanFailForHumanlike,"breakthrough will factor in various aspect, such as mood,health,time since last breakthrough");
            if(isBreakthroughCanFailForHumanlike)
            {
                listing_Standard.Label("breathrough chance global modifier: " + breakthroughSuccessOverallModifier.ToStringPercent("0"));
                listing_Standard.TextFieldNumeric(ref breakthroughSuccessOverallModifier, ref breakthroughSuccessOverallModifierString, 0.01f, 2.00f);
                breakthroughSuccessOverallModifier = listing_Standard.Slider(breakthroughSuccessOverallModifier,0.01f,2.00f);
                breakthroughSuccessOverallModifierString = breakthroughSuccessOverallModifier.ToString("0.00");
                /*listing_Standard.TextFieldNumeric(ref breakthroughSuccessOverallModifier, ref breakthroughSuccessOverallModifierString, 0.01f, 2.00f);*/
            }
            listing_Standard.CheckboxLabeled("armor grade stack multiplicatively", ref isArmorGradeStackMultiplicatively, "if ON: each piece of apparel with grade reduce damage multiplicatively, \n if Off: only apparel with the highest grade take effect.");
            listing_Standard.CheckboxLabeled("cultivation restricted plant", ref isSpiritPlantRestrictedByCultivationLevel, "if ON: spirit plant require cultivator to plant, different plant require different cultivation level");
            listing_Standard.CheckboxLabeled("cultivator immunity",ref isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal,"if ON: cultivator of core shaping and above are only take 1% damage from mortal pawn, after Saint realm, they are completely immune.");
            listing_Standard.NewColumn();
            listing_Standard.Label("");
            listing_Standard.Gap(8);
            listing_Standard.GapLine();
            listing_Standard.Label("realm different limit");
            listing_Standard.Label("cultivator can't do damage to pawn " + (realmDifferentLimit + 1).ToString().Colorize(Color.green) + " realm higer than them");
            realmDifferentLimit = Mathf.RoundToInt(listing_Standard.Slider(realmDifferentLimit,1,18));
            listing_Standard.Label("global multiplier for qi type",tooltip:"when pawn meditate near item with matching qi type., it an added multiplier on top, adjust this if you feel the progress is too fast.");
            globalQiTypeMultiplierString = globalQiTypeMultiplier.ToString("0.00");
            listing_Standard.TextFieldNumeric(ref globalQiTypeMultiplier,ref globalQiTypeMultiplierString,0f,1.00f);
            listing_Standard.CheckboxLabeled("nerf Body Cultivator Melee Damage", ref isNerfBodyCultivatorMeleeDamage);
            if (isNerfBodyCultivatorMeleeDamage)
            {
                listing_Standard.Label("multiplier applied to melee damage factor in body cultivation hediff");
                listing_Standard.Label($"Modifier: {MeleeDamageMultiplier}");
                MeleeDamageMultiplier = Mathf.Round(listing_Standard.Slider(MeleeDamageMultiplier,0.1f,2.0f) * 20f) / 20f;
            }
            listing_Standard.CheckboxLabeled("Body Cultivator mining/etc. xp", ref isBodyCulGainXpThroughJob,"if ON, Body Cultivator can gain cultivation through mining and similar job-based task");
            listing_Standard.Label("Global Stat Multiplier",tooltip:"apply modifier to all stat agained from cultivation. require restart.");
            listing_Standard.Label($"Modifier: x{globalStatMultiplier}");
            globalStatMultiplier = Mathf.Round(listing_Standard.Slider(globalStatMultiplier, 0.1f, 2.0f) * 20) / 20f;
            listing_Standard.CheckboxLabeled("Body Cultivator can use weapon",ref isBodyCulCanUseWeapon);
            listing_Standard.CheckboxLabeled("Body Cultivator can use bionic",ref isBodyCulCanUseBionic);
            listing_Standard.Gap(8f);
            if (listing_Standard.ButtonText("Reset Difficulty to default"))
            {
                DifficultyDefault();
            }
        }
        public void MiscSetting(ref Listing_Standard listing_Standard)
        {
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("Cultivation increase bodies HP", ref isCultivationAffectBodyHP, "can affect performance. use with care");
            XMLLabel(listing_Standard,"Add Cultivation caravan trader", ref isAddingCultivationTraderToFactionCaravan,
                "since this mod itself doesn't add it own faction, this option add in a cultivation resource trader to all faction that isn't a permanent hostile","CTR.CaravanTraderKind");
            if (isAddingCultivationTraderToFactionCaravan)
            {
                XMLLabel(listing_Standard,"Add Cultivation base trader", ref isAddingCultivationTraderToFactionBase,
                    "add Cultivation resource trader kind to all faction base" +
                    "\nif On : all faction base can have cultivation resource/pill/manual as one of the possible trade type","CTR.BaseTraderKind");
            }
            listing_Standard.CheckboxLabeled("plant fertility formation can affect spirit plant",ref isFertilityFormationAffectSpiritPlant, "plant fertility formation can affect spirit plant");
            listing_Standard.CheckboxLabeled("plant fertility formation have a chance to age pawn",ref isFertilityFormationAgePawn, "plant fertility formation have a chance to age pawn");
            listing_Standard.CheckboxLabeled("workspeed nerf",ref isNerfingWorkSpeed,"nerf the global workspeed bonus and manipulation");
            listing_Standard.CheckboxLabeled("nerf Incoming Damage Modifier",ref isNerfingCultivatorIDM, "remove damage reduction from cultivation(Incoming Damage Modifier) entirely");
            listing_Standard.CheckboxLabeled("non-player crafted item can spawn with grade", ref isWildItemSpawnWithGrade);
            listing_Standard.CheckboxLabeled("spawn qi fleck when meditating", ref isSpawnQiFleck);
            listing_Standard.CheckboxLabeled("show damage modifier from weapon grade",ref isShowDamageModifierGrade);
            listing_Standard.CheckboxLabeled("yin-yang background rotate",ref isYinYangBackgroundRotate,"yin-yang in Itab background rotating when qi cultivator meditate");
            XMLLabel(listing_Standard,"Spirit Plant anywhere",ref isSpiritPlantGrowAnywhere,"it on the road, the floor, the river","CTR.SpiritPlantGrowAnywhere");
            listing_Standard.Gap(8f);
            if (listing_Standard.ButtonText("Reset Misc to default"))
            {
                MiscDefault();
            }
        }
        public void GeneralDefault()
        {
            isShowingCultivateExpText = false;
            isPlayingTickingSound = true;
            isWildAnimalAutoCultivate = false;
            isOnlyColonyAnimalAutoCultivate = true;
            isWildAnimalAutoBreakthrought = false;
            isWildAnimalIgnoreSafetyThresholdForBreakthrough = false;
            isColonyAnimalAutoBreakthrough = false;
            isColonyAnimalIgnoreSafetyThresholdForBreakthrough = false;
            tribulationSafety = 0.5f;
            isAllowWildSpiritPlantSpawn = true;
            isSpiritPlantDamageSource = true;
            animalCultivationSpeedMultiplier = 1.00f;
            isUsingQuadrum = false;
            isSpiritGrassCanSupportItSelf = true;
        }
        public void DifficultyDefault()
        {
            isAnimalDropBeastCore = true;
            isCulSpeedAffectedByEnviaronment = true;
            isNeedCapped = true;
            isTribulationChangeWeather = true;
            severityMultiplier = 1.0f;
            isCultivatorNeedQiSourceToImprove = false;
            isCultivatorNeedHighTierQi = false;
            tribRemnantChance = 0.05f;
            isBreakthroughCanFailForHumanlike = false;
            breakthroughSuccessOverallModifier = 1.00f;
            isArmorGradeStackMultiplicatively = false;
            isSpiritPlantRestrictedByCultivationLevel = false;
            isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal = true;
            realmDifferentLimit = 3;
            globalQiTypeMultiplier = 1f;
            isNerfBodyCultivatorMeleeDamage = false;
            MeleeDamageMultiplier = 1f;
            isBodyCulGainXpThroughJob = true;
            globalStatMultiplier = 1f;
        }
        public void MiscDefault()
        {
            isCultivationAffectBodyHP = false;
            isAddingCultivationTraderToFactionCaravan = true;
            settingKey.AddDistinct("CTR.CaravanTraderKind");
            isAddingCultivationTraderToFactionBase = true;
            settingKey.AddDistinct("CTR.BaseTraderKind");
            isFertilityFormationAffectSpiritPlant = false;
            isFertilityFormationAgePawn = true;
            isNerfingWorkSpeed = false;
            isNerfingCultivatorIDM = false;
            isWildItemSpawnWithGrade = true;
            isShowDamageModifierGrade = true;
            isYinYangBackgroundRotate = false;
            isSpiritPlantGrowAnywhere = false;
            settingKey.Remove("CTR.SpiritPlantGrowAnywhere");
        }
        
        private static List<TabRecord> tabList = new List<TabRecord>();
        
        public void XMLLabel(Listing_Standard listingStandard, string label, ref bool checkOn, string tooltip, string key)
        {
            try
            {
                listingStandard.CheckboxLabeled(label, ref checkOn, tooltip);
                if (settingKey.NullOrEmpty())
                {
                    settingKey = [];
                    Log.Warning("[CTR]Warning: settingKey is null or empty");
                }
                if (checkOn)
                {
                    settingKey.AddDistinct(key);
                }
                else
                {
                    settingKey.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[CTR]Error adding setting {ex}");
            }
            
        }
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref isShowingCultivateExpText, "isShowingCultivateExpText",false);
            Scribe_Values.Look(ref severityMultiplier, "severityMultiplier", 1f);
            Scribe_Values.Look(ref isPlayingTickingSound, "isPlayingTickingSound", false);
            Scribe_Values.Look(ref isWildAnimalAutoCultivate, "isWildAnimalAutoCultivate", false);
            Scribe_Values.Look(ref isOnlyColonyAnimalAutoCultivate, "isOnlyColonyAnimalAutoCultivate", true);
            Scribe_Values.Look(ref isWildAnimalAutoBreakthrought, "isWildAnimalAutoBreakthrought", false);
            Scribe_Values.Look(ref isColonyAnimalAutoBreakthrough, "isColonyAnimalAutoBreakthrough", false);
            Scribe_Values.Look(ref isWildAnimalIgnoreSafetyThresholdForBreakthrough, "isWildAnimalIgnoreSafetyThresholdForBreakthrough", false);
            Scribe_Values.Look(ref isColonyAnimalIgnoreSafetyThresholdForBreakthrough, "isColonyAnimalIgnoreSafetyThresholdForBreakthrough", false);
            Scribe_Values.Look(ref tribulationSafety, "tribulationSafety", 0.01f);
            Scribe_Values.Look(ref isAnimalDropBeastCore, "isAnimalDropBeastCore", true);
            Scribe_Values.Look(ref isCulSpeedAffectedByEnviaronment, "isCulSpeedAffectedByEnviaronment", true);
            Scribe_Values.Look(ref isNeedCapped, "isNeedCapped", true);
            Scribe_Values.Look(ref isTribulationChangeWeather, "isTribulationChangeWeather", true);
            Scribe_Values.Look(ref isCultivationAffectBodyHP, "isCultivationAffectBodyHP", false);
            Scribe_Values.Look(ref isCultivatorNeedQiSourceToImprove, "isCultivatorNeedQiSourceToImprove", false);
            Scribe_Values.Look(ref isAddingCultivationTraderToFactionCaravan, "isAddingCultivationTraderToFactionCaravan", false);
            Scribe_Values.Look(ref isAddingCultivationTraderToFactionBase, "isAddingCultivationTraderToFactionBase", false);
            Scribe_Values.Look(ref isCultivatorNeedHighTierQi, "isCultivatorNeedHighTierQi", false);
            Scribe_Values.Look(ref tribRemnantChance, "tribRemnantChance", 0.05f);
            Scribe_Values.Look(ref isBreakthroughCanFailForHumanlike, "isBreakthroughCanFailForHumanlike", false);
            Scribe_Values.Look(ref breakthroughSuccessOverallModifier, "breakthroughSuccessOverallModifier", 1.00f);
            Scribe_Values.Look(ref isArmorGradeStackMultiplicatively, "isArmorGradeStackMultiplicatively", false);
            Scribe_Values.Look(ref isSpiritPlantRestrictedByCultivationLevel, "isSpiritPlantRestrictedByCultivationLevel", false);
            Scribe_Values.Look(ref isFertilityFormationAffectSpiritPlant, "isFertilityFormationAffectSpiritPlant", false);
            Scribe_Values.Look(ref isFertilityFormationAgePawn, "isFertilityFormationAgePawn", false);
            Scribe_Values.Look(ref isNerfingWorkSpeed, "isNerfingWorkSpeed", false);
            Scribe_Values.Look(ref isNerfingCultivatorIDM, "isNerfingCultivatorIDM", false);
            Scribe_Values.Look(ref isAllowWildSpiritPlantSpawn, "isAllowWildSpiritPlantSpawn", true);
            Scribe_Values.Look(ref isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal, "isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal", true);
            Scribe_Values.Look(ref realmDifferentLimit, "realmDifferentLimit", 3);
            Scribe_Values.Look(ref isWildItemSpawnWithGrade, "isWildItemSpawnWithGrade", true);
            Scribe_Values.Look(ref animalCultivationSpeedMultiplier, "animalCultivationSpeedMultiplier", 1.00f);
            Scribe_Values.Look(ref isDeAgingPawn, "isDeAgingPawn", true);
            Scribe_Values.Look(ref isUsingQuadrum, "isUsingQuadrum", true);
            Scribe_Values.Look(ref isSpawnQiFleck, "isSpawnQiFleck", true);
            Scribe_Values.Look(ref globalQiTypeMultiplier, "globalQiTypeMultiplier", 1.00f);
            Scribe_Values.Look(ref isSpiritPlantDamageSource, "isSpiritPlantDamageSource", true);
            Scribe_Values.Look(ref isShowDamageModifierGrade, "isShowDamageModifierGrade", true);
            Scribe_Values.Look(ref isNerfBodyCultivatorMeleeDamage, "isNerfBodyCultivatorMeleeDamage", false);
            Scribe_Values.Look(ref MeleeDamageMultiplier, "MeleeDamageMultiplier", 1.0f);
            Scribe_Values.Look(ref globalStatMultiplier, "globalStatMultiplier", 1.0f);
            Scribe_Values.Look(ref isSpiritGrassCanSupportItSelf, "isSpiritGrassCanSupportItSelf", true);
            Scribe_Values.Look(ref isBodyCulGainXpThroughJob, "isBodyCulGainXpThroughJob", true);
            Scribe_Values.Look(ref isYinYangBackgroundRotate, "isYinYangBackgroundRotate", true);
            Scribe_Values.Look(ref isSpiritPlantGrowAnywhere, "isSpiritPlantGrowAnywhere", true);
            Scribe_Values.Look(ref isBodyCulCanUseWeapon, "isBodyCulCanUseWeapon", false);
            Scribe_Values.Look(ref isBodyCulCanUseBionic, "isBodyCulCanUseBionic", false);
            Scribe_Collections.Look(ref settingKey,"settingKey", LookMode.Value);
            base.ExposeData();
        }        
    }
    public class CultivatorOfTheRimMod : Mod
    {
        public static CultivatorOfTheRimSetting settings;

        public static CultivatorOfTheRimMod instance;

        public CultivatorOfTheRimMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<CultivatorOfTheRimSetting>();
            instance = this;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return base.Content.Name;
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }
}