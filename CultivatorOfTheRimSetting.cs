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

        public bool isWildAnimalAutoBreakthrought = false;

        public bool isWildAnimalIgnoreSafetyThresholdForBreakthrough = false;

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

            var listing = new Listing_Standard();
            listing.Begin(leftThird);
            listing.Label("some setting may need restart".Colorize(Color.red));
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
            listing_Standard.GapLine();
            listing_Standard.CheckboxLabeled("showing exp text", ref isShowingCultivateExpText, "if ON. when pawn meditate, a green text will show severity they gain per trigger");
            listing_Standard.CheckboxLabeled("Chrono Stele and Beacon ticking sound", ref isPlayingTickingSound);
            listing_Standard.CheckboxLabeled("wild animal auto cultivate", ref isWildAnimalAutoCultivate, "only affect those that spawn with cultivation");
            listing_Standard.CheckboxLabeled("wild animal auto breakthrough", ref isWildAnimalAutoBreakthrought);
            listing_Standard.CheckboxLabeled("is allowing spirit plant wild spawn",ref isAllowWildSpiritPlantSpawn,"allow spirit plant to randomly spawn in the wild");
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
            listing_Standard.Gap(8f);
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
            listing_Standard.CheckboxLabeled("food and rest cap", ref isNeedCapped, "if On. pawn of golden core or above will restore their food and rest need to 50% every 1 hour");
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
            listing_Standard.CheckboxLabeled("cultivator immunity",ref isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal,"cultivator of golden core and above are immune to damage from mortal pawn");
            listing_Standard.NewColumn();
            listing_Standard.Label("");
            listing_Standard.Gap(8);
            listing_Standard.GapLine();
            listing_Standard.Label("realm different limit");
            listing_Standard.Label("cultivator can't do damage to pawn " + realmDifferentLimit.ToString().Colorize(Color.green) + " realm higer than them");
            realmDifferentLimit = Mathf.RoundToInt(listing_Standard.Slider(realmDifferentLimit,1,18));
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
            listing_Standard.CheckboxLabeled("Add Cultivation caravan trader", ref isAddingCultivationTraderToFactionCaravan,
                "since this mod itself doesn't add it own faction, this option add in a cultivation resource trader to all faction that isn't a permanent hostile");
            if (isAddingCultivationTraderToFactionCaravan)
            {
                listing_Standard.CheckboxLabeled("Add Cultivation base trader", ref isAddingCultivationTraderToFactionBase,
                    "add Cultivation resource trader kind to all faction base" +
                    "\nif On : all faction base can have cultivation resource/pill/manual as one of the possible trade type");
            }
            listing_Standard.CheckboxLabeled("plant fertility formation can affect spirit plant",ref isFertilityFormationAffectSpiritPlant, "plant fertility formation can affect spirit plant");
            listing_Standard.CheckboxLabeled("plant fertility formation have a chance to age pawn",ref isFertilityFormationAgePawn, "plant fertility formation have a chance to age pawn");
            listing_Standard.CheckboxLabeled("workspeed nerf",ref isNerfingWorkSpeed,"nerf the global workspeed bonus and manipulation");
            listing_Standard.CheckboxLabeled("nerf Incoming Damage Modifier",ref isNerfingCultivatorIDM, "remove damage reduction from cultivation(Incoming Damage Modifier) entirely");
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
            isWildAnimalAutoBreakthrought = false;
            isWildAnimalIgnoreSafetyThresholdForBreakthrough = false;
            tribulationSafety = 0.5f;
            isAllowWildSpiritPlantSpawn = true;
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
        }
        public void MiscDefault()
        {
            isCultivationAffectBodyHP = false;
            isAddingCultivationTraderToFactionCaravan = true;
            isAddingCultivationTraderToFactionBase = true;
            isFertilityFormationAffectSpiritPlant = false;
            isFertilityFormationAgePawn = true;
            isNerfingWorkSpeed = false;
            isNerfingCultivatorIDM = false;            
        }

        private static List<TabRecord> tabList = new List<TabRecord>();
        public override void ExposeData()
        {
            Scribe_Values.Look(ref isShowingCultivateExpText, "isShowingCultivateExpText",false);
            Scribe_Values.Look(ref severityMultiplier, "severityMultiplier", 1f);
            Scribe_Values.Look(ref isPlayingTickingSound, "isPlayingTickingSound", false);
            Scribe_Values.Look(ref isWildAnimalAutoCultivate, "isWildAnimalAutoCultivate", false);
            Scribe_Values.Look(ref isWildAnimalAutoBreakthrought, "isWildAnimalAutoBreakthrought", false);
            Scribe_Values.Look(ref isWildAnimalIgnoreSafetyThresholdForBreakthrough, "isWildAnimalIgnoreSafetyThresholdForBreakthrough", false);
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
            base.ExposeData();
        }        
    }
    public class CultivatorOfTheRimMod : Mod
    {
        public static CultivatorOfTheRimSetting settings;

        public CultivatorOfTheRimMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<CultivatorOfTheRimSetting>();
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
