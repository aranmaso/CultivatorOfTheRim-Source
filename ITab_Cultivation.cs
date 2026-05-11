using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows;
using Verse;
using Verse.AI;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;

namespace CultivatorOfTheRim
{
    [StaticConstructorOnStartup]
    public class ITab_Cultivation : ITab
    {
        private Texture2D texture_Back_Gray;

        private Texture2D texture_Back_Gold;

        private Texture2D texture_Back_DarkGold;

        private Texture2D texture_Red;

        private Texture2D texture_Back_DarkerGold;

        private Texture2D texture_Back_DarkCyan;

        private Texture2D texture_Back_Cyan;


        private Texture2D texture_Back_Green;

        private Texture2D texture_Back_TipSectionTitleColor;

        private Texture2D BreakthroughIcon;

        private Texture2D UI_Background;

        private Texture2D UI_BackgroundYinYang;

        private Texture2D BreakthroughArrow;

        private Vector2 infoScroll;

        private Vector2 infoScrollUpper;

        private float lastHeightLeft;

        private float lastHeightLeftUpper;

        private Vector2 infoScrollRight;

        private float lastHeightRight;

        private float ticking = 0;

        private float rotationAngle = 0;

        private float culSpeed = 1f;

        private float tribChance = 0f;

        private float healthMod = 1f;
        
        private float breakthroughChance = 1f;

        private int pulseTick = 0;

        public float apparelDamageReduction = 1f;

        public override bool IsVisible => Find.Selector.SingleSelectedThing is Pawn pawn && pawn.HaveAnyCultivation();
        
        public Pawn GetPawn
        {
            get
            {
                if (SelPawn != null)
                {
                    return SelPawn;
                }
                if (SelThing is Corpse corpse)
                {
                    return corpse.InnerPawn;
                }
                return null;
            }
        }

        public Pawn tempPawn;
        
        public Hediff_CultivationBase CultivationBase;
        
        public Hediff_CultivationLevel QiCultivation;
        
        public Hediff_BodyCultivaton BodyCultivation;

        public CultivationHediffDef CulDef;

        public HediffStage CurStage;

        static ITab_Cultivation()
        {
            foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
            {
                RaceProperties race = allDef.race;
                if (race != null)
                {
                    allDef.inspectorTabs?.Add(typeof(ITab_Cultivation));
                    allDef.inspectorTabsResolved?.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Cultivation)));
                }
            }
        }
        public ITab_Cultivation()
        {
            size = new Vector2(Mathf.Min(610f, UI.screenWidth), Mathf.Min(500f, UI.screenHeight ));
            labelKey = "ITab_Cultivation";
            BreakthroughIcon = ContentFinder<Texture2D>.Get("UI/UIicon/BreakthroughIcon");
            UI_Background = ContentFinder<Texture2D>.Get("UI/CTR_Backgroundtab");
            UI_BackgroundYinYang = ContentFinder<Texture2D>.Get("UI/CTR_Backgroundtab_yinyang");
            BreakthroughArrow = ContentFinder<Texture2D>.Get("UI/UIicon/BreakthroughIconArrow");
            texture_Back_Gold = SolidColorMaterials.NewSolidColorTexture(new Color(0.76f, 0.60f, 0.0f));
            texture_Back_DarkGold = SolidColorMaterials.NewSolidColorTexture(new Color(0.56f, 0.44f, 0f, 0.65f));
            texture_Back_DarkerGold = SolidColorMaterials.NewSolidColorTexture(GenColor.FromHex("221B00"));
            texture_Back_DarkCyan = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0.25f, 0.25f));
            texture_Back_Cyan = SolidColorMaterials.NewSolidColorTexture(new Color(0f, 0.5f, 0.5f));
            texture_Back_Gray = SolidColorMaterials.NewSolidColorTexture(new Color(0.25f, 0.25f, 0.25f, 0.65f));
            texture_Red = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0f, 0.06f,1f));
        }
        public override void OnOpen()
        {
            base.OnOpen();
            if (tempPawn != GetPawn)
            {
                tempPawn = GetPawn;
            }
            GetCachedHediff();
            ticking = 0;
            culSpeed = GetPawn.GetStatValue(CTR_DefOf.CultivationSpeed);
            tribChance = GetPawn.GetStatValue(CTR_DefOf.TribulationChance);
            healthMod = GetPawn.GetStatValue(CTR_DefOf.CTR_HealthMultiplier);
            breakthroughChance = Cultivation_Utility.GetBreakthroughChance(GetPawn, false);
            pulseTick = 0;
            if (GetPawn.RaceProps.Humanlike)
            {
                GetApparelGradeTotal();
            }
        }

        protected override void CloseTab()
        {
            base.CloseTab();
        }

        public void GetCachedHediff()
        {
            CultivationBase = GetPawn.FindAnyCultivationLevel();
            QiCultivation = GetPawn.FindCultivationLevel(); 
            BodyCultivation = GetPawn.FindBodyCultivationLevel();
            CulDef = CultivationBase.cultivationDef;
            CurStage = CultivationBase.CurStage;
        }

        public void GetApparelGradeTotal()
        {
            apparelDamageReduction = 1f;
            if (GetPawn.apparel.WornApparel.NotNullOrEmpty())
            {
                foreach (var item in GetPawn.apparel.WornApparel)
                {
                    CompItemGrade grade = item?.TryGetComp<CompItemGrade>();
                    if (grade != null)
                    {
                        if (CultivatorOfTheRimMod.settings.isArmorGradeStackMultiplicatively)
                        {
                            switch (grade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    break;
                                case ItemGrade.Ordinary:
                                    apparelDamageReduction *= 0.95f;
                                    break;
                                case ItemGrade.Earth:
                                    apparelDamageReduction *= 0.85f;
                                    break;
                                case ItemGrade.Heaven:
                                    apparelDamageReduction *= 0.825f;
                                    break;
                                case ItemGrade.Mysterious:
                                    apparelDamageReduction *= 0.80f;
                                    break;
                                case ItemGrade.Divine:
                                    apparelDamageReduction *= 0.75f;
                                    break;
                                case ItemGrade.Emperor:
                                    apparelDamageReduction *= 0.725f;
                                    break;
                                case ItemGrade.Dao:
                                    apparelDamageReduction *= 0.5f;
                                    break;
                            }
                        }
                        else
                        {
                            switch (grade.Grade)
                            {
                                case ItemGrade.Mortal:
                                    break;
                                case ItemGrade.Ordinary:
                                    if (0.8f < apparelDamageReduction) apparelDamageReduction = 0.8f;
                                    break;
                                case ItemGrade.Earth:
                                case ItemGrade.Heaven:
                                case ItemGrade.Mysterious:
                                    if (0.5f < apparelDamageReduction) apparelDamageReduction = 0.5f;
                                    break;
                                case ItemGrade.Divine:
                                case ItemGrade.Emperor:
                                    if (0.25f < apparelDamageReduction) apparelDamageReduction = 0.25f;
                                    break;
                                case ItemGrade.Dao:
                                    if (0.1f < apparelDamageReduction) apparelDamageReduction = 0.1f;
                                    break;
                            }
                        }
                    }
                }
                
            }
        }
        
        protected override void FillTab()
        {
            if (GetPawn != null)
            {
                if (tempPawn != GetPawn)
                {
                    OnOpen();
                }
                Rect background = new Rect(0f, 0f, size.x, size.y);
                GUI.DrawTexture(background, texture_Back_DarkerGold);
                GUI.DrawTexture(background, UI_Background);

                Rect background_yinyang = background;
                if (QiCultivation != null)
                {
                    if (GetPawn.RaceProps.Humanlike && GetPawn.psychicEntropy.IsCurrentlyMeditating && Find.TickManager.CurTimeSpeed != TimeSpeed.Paused && CultivatorOfTheRimMod.settings.isYinYangBackgroundRotate)
                    {
                        Vector2 center = new Vector2(background_yinyang.x + background_yinyang.width / 2f, background_yinyang.y + background_yinyang.height / 2f);
                        float angle = Find.TickManager.TicksGame * 6f;
                        Widgets.DrawTextureRotated(background_yinyang, UI_BackgroundYinYang, angle);
                        rotationAngle = angle;
                    }
                    else
                    {
                        Widgets.DrawTextureRotated(background_yinyang, UI_BackgroundYinYang, rotationAngle);
                    }
                }
                else
                {
                    GUI.DrawTexture(background_yinyang, UI_BackgroundYinYang);
                }
                Rect rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(10f);

                Rect majorInfo = rect.ContractedBy(10f);
                majorInfo.width /= 2f;
                majorInfo.width -= 25f;
                majorInfo.height /= 2f;
                majorInfo.height -= 20f;
                majorInfo.y += 30f;
                Rect majorInfoBack = majorInfo.ContractedBy(-2f);
                GUI.DrawTexture(majorInfoBack, texture_Back_DarkGold);
                GUI.DrawTexture(majorInfo, texture_Back_Gray);

                Rect viewRectLeftUpper = new Rect(0f, 0f, majorInfo.width, lastHeightLeftUpper);
                viewRectLeftUpper.width -= 20f;
                Widgets.BeginScrollView(majorInfo, ref infoScrollUpper, viewRectLeftUpper);
                try
                {
                    float num = 5f;
                    Rect innerRect = viewRectLeftUpper.ContractedBy(2f);
                    Text.Font = GameFont.Medium;
                    Widgets.Label(innerRect, $"<b>Realm Info:</b>");
                    Text.Font = GameFont.Small;
                    innerRect.y += 30f;
                    num += 30f;

                    Widgets.LabelFit(innerRect, $"<b>Realm Name</b>: {CulDef.LabelCap}");
                    innerRect.y += 20f;
                    num += 20f;
                    Widgets.Label(innerRect, $"<b>Stage</b>: {CurStage.label.CapitalizeFirst()}");
                    innerRect.y += 20f;
                    num += 20f;
                    Widgets.Label(innerRect, $"<b>Realm Power</b>: {CulDef.realmPower}");
                    innerRect.y += 20f;
                    num += 20f;
                    Widgets.Label(innerRect, $"<b>Power Info</b>: capable of damage pawn up to {CultivatorOfTheRimMod.settings.realmDifferentLimit + CulDef.realmPower} Realm Power.");
                    innerRect.y += 35f;
                    num += 35f;
                    Widgets.LabelFit(innerRect,$"<b>Immune to Mortal</b>: {(CultivatorOfTheRimMod.settings.isCultivatorOfGoldenCoreOrSaintAndUpImmuneToMortal && CultivationBase.IsCoreShapingOrAbove() ? "✔" : "✘")}");
                    innerRect.y += 20f;
                    num += 20f;
                    bool canGetTrib = QiCultivation?.canGetTrib ?? (BodyCultivation?.canGetTrib ?? CulDef.canGetTribulation);
                    Color tribWarning = new Color(1f,1f * (tribChance * -1f), 0f);
                    if (canGetTrib)
                    {
                        if (tribChance >= 0f) tribWarning = new Color(1f, 1f - tribChance, 1 - (tribChance * 2f));
                    }
                    Widgets.LabelFit(innerRect, $"<b>Tribulation Chance</b>: {(canGetTrib ? tribChance >= 0f ? tribChance.ToStringPercent("0").Colorize(tribWarning) : tribChance.ToStringPercent("0") : 0f.ToStringPercent("0"))}");
                    innerRect.y += 20f;
                    num += 20f;
                    Widgets.LabelFit(innerRect, $"<b>Cultivation Speed</b>: {culSpeed.ToStringPercent("0.00")}");
                    innerRect.y += 20f;
                    num += 20f;
                    Widgets.LabelFit(innerRect, $"<b>Health Multiplier</b>: {healthMod.ToStringPercent("0.00").Colorize(Color.green)}");
                    innerRect.y += 20f;
                    num += 20f;
                    Widgets.LabelFit(innerRect, $"<b>Damage Reduction</b>: {apparelDamageReduction.ToStringPercent("0.00")}");
                    innerRect.y += 20f;
                    num += 20f;
                    lastHeightLeftUpper = num;
                }
                finally
                {
                    Widgets.EndScrollView();
                }
                Rect stageInfoRect = majorInfo;
                stageInfoRect.y = majorInfo.yMax + 15f;
                Rect rect3Back = stageInfoRect.ContractedBy(-2f);
                GUI.DrawTexture(rect3Back, texture_Back_DarkGold);
                GUI.DrawTexture(stageInfoRect, texture_Back_Gray);

                Rect viewRectLeft = new Rect(0f, 0f, stageInfoRect.width, lastHeightLeft);
                Widgets.BeginScrollView(stageInfoRect, ref infoScroll, viewRectLeft);
                try
                {
                    float num = 5f;
                    Rect innerRect = viewRectLeft.ContractedBy(2f);
                    if (CulDef.bonusHediff.NotNullOrEmpty())
                    {
                        Text.Font = GameFont.Medium;
                        Widgets.Label(innerRect, $"<b>Initial Bonus</b>");
                        Text.Font = GameFont.Small;
                        innerRect.y += 20f;
                        num += 20f;
                        foreach (var bonus in CulDef.bonusHediff)
                        {
                            innerRect.y += 10f;
                            num += 10f;
                            Widgets.Label(innerRect, $"{bonus.hediffDef.LabelCap}:  {bonus.chance.ToStringPercent("0")}");
                            innerRect.y += 10f;
                            num += 10f;
                        }
                        innerRect.y += 15f;
                        num += 15f;
                    }
                    if (CulDef.cultivationStages.NotNullOrEmpty())
                    {
                        Text.Font = GameFont.Medium;
                        Widgets.Label(innerRect, $"<b>Available Cultivation Stage</b>".Colorize(Color.white));
                        Text.Font = GameFont.Small;
                        innerRect.y += 50f;
                        num += 50f;
                        foreach (var culStage in CulDef.cultivationStages)
                        {
                            Widgets.Label(innerRect, $"<b>Stage: {culStage.label}. ({culStage.minSeverity} / {CulDef.maxSeverity})</b>".Colorize(Color.cyan));
                            innerRect.y += 25f;
                            num += 25f;
                            if (culStage.empty)
                            {
                                Widgets.Label(innerRect, $"-".Colorize(Color.green));                                
                            }
                            if (culStage.hediffChance.NotNullOrEmpty())
                            {
                                Widgets.Label(innerRect, $"<b>Hediff Chance</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var heChance in culStage.hediffChance)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{heChance.hediffDef.LabelCap}:  {heChance.chance.ToStringPercent("0")}".Colorize(Color.green));
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.hediffsGained.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Hediff Gained</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var hediff in culStage.hediffsGained)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{hediff.LabelCap}");
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.hediffsRemoved.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Hediff Removed</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var hediff in culStage.hediffsRemoved)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{hediff.LabelCap}");
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.abilities.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Ability Gained</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var ability in culStage.abilities)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{ability.LabelCap}");
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.removeAbilities.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Ability Removed</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var ability in culStage.removeAbilities)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{ability.LabelCap}");
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.geneDefs.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Gene Gained</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var geneDef in culStage.geneDefs)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{geneDef.LabelCap}:   Xenogene: {culStage.isXenogene}");
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.geneDefsRemove.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Gene Removed</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var geneDef in culStage.geneDefsRemove)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{geneDef.LabelCap}");
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.traitDefs.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Trait Gained</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var trait in culStage.traitDefs)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    if (trait.degree.HasValue)
                                    {
                                        Widgets.Label(innerRect, $"{trait.def.DataAtDegree(trait.degree.Value).LabelCap}");
                                    }
                                    else
                                    {
                                        Widgets.Label(innerRect, $"{trait.def.DataAtDegree(0).LabelCap}");
                                    }
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (culStage.removeTraitDefs.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Trait Removed</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var trait in culStage.removeTraitDefs)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    if (trait.degree.HasValue)
                                    {
                                        Widgets.Label(innerRect, $"{trait.def.DataAtDegree(trait.degree.Value).LabelCap}");
                                    }
                                    else
                                    {
                                        Widgets.Label(innerRect, $"{trait.def.DataAtDegree(0).LabelCap}");
                                    }
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            innerRect.y += 25f;
                            num += 25f;
                        }
                    }

                    if (CulDef.stages.NotNullOrEmpty())
                    {
                        Text.Font = GameFont.Medium;
                        Widgets.Label(innerRect, $"<b>Available Stage</b>".Colorize(Color.white));
                        Text.Font = GameFont.Small;
                        innerRect.y += 30f;
                        num += 30f;
                        foreach (var item in CulDef.stages)
                        {
                            //Rect innerRect = new Rect(0f, num, viewRectLeft.width, 28f);
                            
                            Widgets.Label(innerRect, $"<b>Stage: {item.label}. ({item.minSeverity} / {CulDef.maxSeverity})</b>".Colorize(Color.cyan));
                            innerRect.y += 10f;
                            num += 10f;
                            if (item.regeneration > 0)
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Regeneration:   {item.regeneration}hp/day</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            if (item.totalBleedFactor != 1f)
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Total Bleed Factor:   x{item.totalBleedFactor.ToStringPercent("0.00")}</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            if (item.naturalHealingFactor != -1f)
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>Natural Healing Factor:   x{item.naturalHealingFactor.ToStringPercent("0.00")}</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            if (item.statOffsets.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>StatOffset</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var stat in item.statOffsets)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                    {
                                        Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(item.multiplyStatChangesBySeverity ? (stat.value * item.minSeverity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                    }
                                    else if(stat.stat == StatDefOf.CarryingCapacity)
                                    {
                                        Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(item.multiplyStatChangesBySeverity ? stat.value * item.minSeverity : stat.value)}kg");
                                    }
                                    else if(stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                    {
                                        Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(item.multiplyStatChangesBySeverity ? stat.value * item.minSeverity : stat.value)}kg");
                                    }
                                    else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                    {
                                        Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(item.multiplyStatChangesBySeverity ? stat.value * item.minSeverity : stat.value)}c");
                                    }
                                    else
                                    {
                                        Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(item.multiplyStatChangesBySeverity ? stat.value * item.minSeverity : stat.value)}");
                                    }                                    
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            if (item.statFactors.NotNullOrEmpty())
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"<b>StatFactor</b>".Colorize(Color.green));
                                innerRect.y += 10f;
                                num += 10f;
                                foreach (var factor in item.statFactors)
                                {
                                    innerRect.y += 10f;
                                    num += 10f;
                                    Widgets.Label(innerRect, $"{factor.stat.LabelCap}:  x{(item.multiplyStatChangesBySeverity ? (factor.value * item.minSeverity).ToStringPercent("0.00") : factor.value.ToStringPercent("0.00"))}");
                                    innerRect.y += 10f;
                                    num += 10f;
                                }
                            }
                            innerRect.y += 25f;
                            num += 25f;
                        }
                    }
                    lastHeightLeft = num;
                }
                finally
                {
                    Widgets.EndScrollView();
                }


                Rect currentStageInfoRect = rect.ContractedBy(10f);
                currentStageInfoRect.x = stageInfoRect.xMax + 10;
                currentStageInfoRect.width = stageInfoRect.width;
                currentStageInfoRect.width += 25f;
                currentStageInfoRect.x += 10;
                currentStageInfoRect.height -= 25f;
                currentStageInfoRect.y += 30f;
                Rect rect4Back = currentStageInfoRect.ContractedBy(-2f);
                GUI.DrawTexture(rect4Back, texture_Back_DarkGold);
                GUI.DrawTexture(currentStageInfoRect, texture_Back_Gray);

                Rect viewRectRight = new Rect(0,0,currentStageInfoRect.width,lastHeightRight);
                Widgets.BeginScrollView(currentStageInfoRect, ref infoScrollRight, viewRectRight);
                try
                {
                    float num = 5f;
                    Rect innerRect = viewRectRight.ContractedBy(2f);
                    if (CurStage != null)
                    {
                        Text.Font = GameFont.Medium;
                        Widgets.Label(innerRect, $"<b>Current Stage</b>".Colorize(Color.white));
                        Text.Font = GameFont.Small;
                        innerRect.y += 30f;
                        num += 30f;

                        Widgets.Label(innerRect, $"<b>Stage: {CurStage.label}. ({CurStage.minSeverity} / {CulDef.maxSeverity})</b>".Colorize(Color.cyan));
                        innerRect.y += 10f;
                        num += 10f;
                        if (CurStage.regeneration > 0)
                        {
                            innerRect.y += 10f;
                            num += 10f;
                            Widgets.Label(innerRect, $"<b>Regeneration:   {CurStage.regeneration}day/s</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                        }
                        if (CurStage.totalBleedFactor != 1f)
                        {
                            innerRect.y += 10f;
                            num += 10f;
                            Widgets.Label(innerRect, $"<b>Total Bleed Factor:   x{CurStage.totalBleedFactor.ToStringPercent("0.00")}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                        }
                        if (CurStage.naturalHealingFactor != -1f)
                        {
                            innerRect.y += 10f;
                            num += 10f;
                            Widgets.Label(innerRect, $"<b>Natural Healing Factor:   x{CurStage.naturalHealingFactor.ToStringPercent("0.00")}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                        }
                        if (CurStage.statOffsets.NotNullOrEmpty())
                        {
                            innerRect.y += 10f;
                            num += 10f;
                            Widgets.Label(innerRect, $"<b>StatOffset</b>".Colorize(Color.green));
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,$"<b>{CTR_DefOf.CTR_PawnCultivation.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == CTR_DefOf.CTR_PawnCultivation))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,$"<b>{StatCategoryDefOf.BasicsPawn.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.BasicsPawn))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if(stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CurStage.minSeverity : stat.value)}kg");
                                }
                                else if(stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CurStage.minSeverity : stat.value)}kg");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnResistances.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.PawnResistances))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnPsyfocus.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.PawnPsyfocus))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnHealth.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.PawnHealth))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.Apparel.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.Apparel))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnCombat.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.PawnCombat))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnSocial.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.PawnSocial))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnWork.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statOffsets.Where(x => x.stat.category == StatCategoryDefOf.PawnWork))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 10f;
                            num += 10f;
                            
                            
                            
                            
                            /*foreach (var stat in CurStage.statOffsets)
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }*/
                        }
                        innerRect.y += 10f;
                        num += 10f;
                        Vector2 start = new Vector2(innerRect.xMin, innerRect.y);
                        Vector2 end = new Vector2(innerRect.xMax -30f, innerRect.y);
                        Widgets.DrawLine(start,end,Color.white,1f);
                        if (CurStage.statFactors.NotNullOrEmpty())
                        {
                            innerRect.y += 10f;
                            num += 10f;
                            Widgets.Label(innerRect, $"<b>StatFactor</b>".Colorize(Color.green));
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,$"<b>{CTR_DefOf.CTR_PawnCultivation.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == CTR_DefOf.CTR_PawnCultivation))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,$"<b>{StatCategoryDefOf.Basics.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.Basics))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnResistances.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.PawnResistances))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnPsyfocus.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.PawnPsyfocus))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnHealth.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.PawnHealth))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.Apparel.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.Apparel))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnCombat.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.PawnCombat))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnSocial.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.PawnSocial))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 20f;
                            num += 20f;
                            Widgets.LabelFit(innerRect,
                                $"<b>{StatCategoryDefOf.PawnWork.LabelCap}</b>".Colorize(Color.green));
                            innerRect.y += 10f;
                            num += 10f;
                            foreach (var stat in CurStage.statFactors.Where(x => x.stat.category == StatCategoryDefOf.PawnWork))
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                if (stat.stat.toStringStyle == ToStringStyle.PercentZero || stat.stat.toStringStyle == ToStringStyle.PercentOne)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:  {(CurStage.multiplyStatChangesBySeverity ? (stat.value * CultivationBase.Severity).ToStringPercent("0.00") : stat.value.ToStringPercent("0.00"))}");
                                }
                                else if (stat.stat == StatDefOf.CarryingCapacity)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.FloatTwoOrThree)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}kg");
                                }
                                else if (stat.stat.toStringStyle == ToStringStyle.Temperature)
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}c");
                                }
                                else
                                {
                                    Widgets.Label(innerRect, $"{stat.stat.LabelCap}:   {(CurStage.multiplyStatChangesBySeverity ? stat.value * CultivationBase.Severity : stat.value)}");
                                }
                                innerRect.y += 10f;
                                num += 10f;
                            }
                            innerRect.y += 10f;
                            num += 10f;
                            
                            /*foreach (var factor in CurStage.statFactors)
                            {
                                innerRect.y += 10f;
                                num += 10f;
                                Widgets.Label(innerRect, $"{factor.stat.LabelCap}:  x{(CurStage.multiplyStatChangesBySeverity ? (factor.value * CultivationBase.Severity).ToStringPercent("0.00") : factor.value.ToStringPercent("0.00"))}");
                                innerRect.y += 10f;
                                num += 10f;
                            }*/
                        }
                        innerRect.y += 25f;
                        num += 25f;
                    }
                    lastHeightRight = num;
                }
                finally
                {
                    Widgets.EndScrollView();
                }
                                             

                Rect rect2 = rect;
                rect2.y = 12f;
                rect2.height = 30f;

                DrawTitle(rect2);

                //GUI.DrawTexture(rect, texture_Back_Gray);

                /*GUI.DrawTexture(rect4, texture_Back_Green);
                GUI.DrawTexture(position, texture_Back_TipSectionTitleColor);*/



            }
        }
        
        public void DrawTitle(Rect rect)
        {
            Rect header_empty = rect;
            header_empty.y += 20f;
            Rect headerBack = header_empty.ContractedBy(-3f);
            GUI.DrawTexture(headerBack, texture_Back_DarkGold);
            Rect header_filling = rect;
            header_filling.y += 20f;

            
            if (CultivatorOfTheRimMod.settings.isBreakthroughCanFailForHumanlike && BodyCultivation == null)
            {
                Rect header_healthFilling = headerBack;
                //header_healthFilling.x = headerBack.xMax;
                float width = headerBack.width;
                pulseTick++;
                if (pulseTick >= 60)
                {
                    breakthroughChance = Cultivation_Utility.GetBreakthroughChance(GetPawn, false);
                    pulseTick = 1;
                }
                header_healthFilling.width = (width * (1f - breakthroughChance));
                
                float pulseSpeed = 5f;
                float pulseIntensity = 5f; // Adjust this for how much it grows

                float pulse = Mathf.Sin(Time.realtimeSinceStartup * pulseSpeed) * 0.5f + 0.5f;
                //float currentOffset = Mathf.Lerp(20f,40f, pulse * pulseIntensity);
                float currentOffset = pulse * pulseIntensity;
                
                // texture_Red = SolidColorMaterials.NewSolidColorTexture(new Color(1.5f - breakthroughChance, breakthroughChance - 0.2f, 0f,1.5f * currentOffset));
                Color redTex = Color.Lerp(Color.red, Color.green, breakthroughChance);
                redTex.a = 1.5f * currentOffset;
                texture_Red = SolidColorMaterials.NewSolidColorTexture(redTex);
                GUI.DrawTexture(header_healthFilling, texture_Red);
            }
            
            float num = CultivationBase.Severity;
            float num2 = CultivationBase.cultivationDef.maxSeverity;
            float num3 = header_empty.width;
            float num4 = num3 * (num / num2);
            header_filling.width = num4;
            GUI.DrawTexture(header_empty, texture_Back_DarkCyan);
            GUI.DrawTexture(header_filling, texture_Back_Cyan);
            if (header_filling.width == header_empty.width)
            {
                GUI.DrawTexture(header_empty, texture_Back_Gold);                
            }

            
            
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Medium;
            Rect rect2 = rect;
            rect2.y += 20f;
            rect2.x += rect.height + 10f;
            rect2.width -= rect.height + 10f;
            //Widgets.LabelFit(rect2, $"<b>Cultivation: {GetPawn.FindAnyCultivationLevel().LabelBaseCap} - {cultivationBase.CurStage.label.CapitalizeFirst()}</b>");            
            Widgets.LabelFit(rect2, $"<b>{GetPawn.FindAnyCultivationLevel().LabelCap.Colorize(CultivationBase.LabelColor)} - Realm Power: {CultivationBase.cultivationDef.realmPower}</b>");            
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Rect position = rect;
            position.height += 5f;
            position.width = position.height;
            position.y += 20f;
            
            if (header_empty.width == header_filling.width && CultivationBase.nextRealm != CultivationBase.cultivationDef)
            {
                position.height *= 1.5f;
                position.width = position.height;
                position.x -= 10f;
                position.y -= 20f;

                if (Mouse.IsOver(position))
                {
                    position.height -= 10f;
                    position.width = position.height;
                    position.y += 5f;
                    position.x += 5f;
                }

                float pulseSpeed = 0.05f;
                float pulseIntensity = 40f; // Adjust this for how much it grows
                ticking += 1;

                float pulse = Mathf.Sin(ticking * pulseSpeed) * 0.5f + 0.5f;
                //float currentOffset = Mathf.Lerp(20f,40f, pulse * pulseIntensity);
                float currentOffset = pulse * pulseIntensity;
                Rect drawingRect = new Rect(position.x - (currentOffset / 2f), position.y - (currentOffset / 2f),position.width + currentOffset,position.height + currentOffset);
                

                GUI.DrawTexture(drawingRect, BreakthroughIcon);
                Rect drawingRect2 = drawingRect;
                drawingRect2.y = drawingRect.yMax;
                GUI.DrawTexture(drawingRect2, BreakthroughArrow);
                if (Widgets.ButtonInvisible(drawingRect, true))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    Hediff_CultivationBase culHediff = GetPawn.FindAnyCultivationLevel();
                    if (culHediff != null)
                    {
                        if (culHediff.def.IsQiCultivation())
                        {
                            var qiComp = culHediff.TryGetComp<HediffComp_Cultivation>();
                            if (qiComp != null)
                            {
                                CultivationHediffDef nextLevel = (CultivationHediffDef)(qiComp.Props.nextLevel != null ? qiComp.Props.nextLevel : qiComp.Props.nextLevels.RandomElementByWeight(x => x.weight).nextRealm);
                                if (nextLevel == null || nextLevel == qiComp.Props.currentLevel)
                                {
                                    Messages.Message("pawn has reach the peak, there no more higher realm than " + culHediff.Label, MessageTypeDefOf.NeutralEvent);
                                }
                                else if (culHediff.Severity < culHediff.def.maxSeverity)
                                {
                                    Messages.Message("realm level hasn't reach it peak state yet", MessageTypeDefOf.NeutralEvent);
                                }
                                else
                                {
                                    if (culHediff.Severity >= culHediff.def.maxSeverity && !GetPawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (nextLevel != null && culHediff.def != nextLevel))
                                    {
                                        culHediff.Cultivation_Advance(qiComp.Props.shouldGetTribulation, culHediff.cultivationDef, nextLevel, qiComp.Props.breakingThroughDuration.RandomInRange, qiComp.Props.TribulationStrikeInterval);
                                        Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, GetPawn);
                                        job.count = 1;
                                        GetPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                        Messages.Message($"{GetPawn} has started the process of breaking through to {nextLevel.LabelCap}", MessageTypeDefOf.NeutralEvent);
                                    }
                                    else if (GetPawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                                    {
                                        Messages.Message("pawn in the process of breakingthrough", MessageTypeDefOf.NeutralEvent);
                                    }
                                }
                            }
                        }
                        else if(culHediff.def.IsBodyCultivation())
                        {
                            var bodyComp = culHediff.TryGetComp<HediffComp_BodyCultivation>();
                            if (bodyComp != null)
                            {
                                CultivationHediffDef nextLevel = (CultivationHediffDef)(bodyComp.Props.nextLevel != null ? bodyComp.Props.nextLevel : bodyComp.Props.nextLevels.RandomElementByWeight(x => x.weight).nextRealm);
                                if (nextLevel == null || nextLevel == culHediff.def)
                                {
                                    Messages.Message("pawn has reach the peak, there no more higher realm than " + culHediff.Label, MessageTypeDefOf.NeutralEvent);
                                }
                                else if (culHediff.Severity < culHediff.def.maxSeverity)
                                {
                                    Messages.Message("realm level hasn't reach it peak state yet", MessageTypeDefOf.NeutralEvent);
                                }
                                else
                                {
                                    if (culHediff.Severity >= culHediff.def.maxSeverity && !GetPawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (nextLevel != null && culHediff.def != nextLevel))
                                    {
                                        culHediff.Cultivation_Advance(bodyComp.Props.canGetTrib, BodyCultivation.cultivationDef, nextLevel, bodyComp.Props.breakingThroughDuration.RandomInRange, bodyComp.Props.TribulationStrikeInterval);
                                        Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, GetPawn);
                                        job.count = 1;
                                        GetPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                        Messages.Message($"{GetPawn} has started the process of breaking through to {nextLevel.LabelCap}", MessageTypeDefOf.NeutralEvent);
                                    }
                                    else if (GetPawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                                    {
                                        Messages.Message("pawn in the process of breakingthrough", MessageTypeDefOf.NeutralEvent);
                                    }
                                }
                            }
                        }
                        else
                        {
                            CultivationHediffDef nextLevel = culHediff.cultivationDef.nextLevel != null ? culHediff.cultivationDef.nextLevel : culHediff.cultivationDef.nextLevels.RandomElementByWeight(x => x.weight).nextRealm;
                            if (nextLevel == null || nextLevel == culHediff.cultivationDef)
                            {
                                Messages.Message("pawn has reach the peak, there no more higher realm than " + culHediff.Label, MessageTypeDefOf.NeutralEvent);
                            }
                            else if (culHediff.Severity < culHediff.def.maxSeverity)
                            {
                                Messages.Message("realm level hasn't reach it peak state yet", MessageTypeDefOf.NeutralEvent);
                            }
                            else
                            {
                                if (culHediff.Severity >= culHediff.def.maxSeverity &&
                                    !GetPawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) &&
                                    (nextLevel != null && culHediff.def != nextLevel))
                                {
                                    culHediff.Cultivation_Advance(culHediff.cultivationDef.canGetTribulation, culHediff.cultivationDef, nextLevel,culHediff.cultivationDef.breakthroughDuration.RandomInRange);
                                    Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, GetPawn);
                                    job.count = 1;
                                    GetPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                                    Messages.Message($"{GetPawn} has started the process of breaking through to {nextLevel.LabelCap}", MessageTypeDefOf.NeutralEvent);
                                }
                                else if (GetPawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                                {
                                    Messages.Message("pawn in the process of breakingthrough", MessageTypeDefOf.NeutralEvent);
                                }
                            }
                        }
                    }
                }
            }            
            else
            {
                GUI.DrawTexture(position, BreakthroughIcon);
            }
        }
    }
}