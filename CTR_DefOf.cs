using RimWorld;
using Verse;

namespace CultivatorOfTheRim
{

    [DefOf]
    internal static class CTR_DefOf
    {
        public static HediffDef CTR_Tribulation;
        public static HediffDef CTR_BreakthroughCounter;
        public static HediffDef CTR_BreakthroughProcess;
        public static HediffDef CTR_DantianDamage;

        public static StatCategoryDef CTR_PawnCultivation;

        [MayRequire("zomuro.itssorcery")]
        public static StatCategoryDef CTR_PawnCultivationTechnique;


        public static JobDef CTR_BreakingThrough;
        public static ThingDef Mote_ResurrectAbility;
        public static ThingDef CTR_JunkPill;
        public static ThingDef CTR_TribulationRemnantPill;
        public static ThingDef CTR_AzureFragment;

        public static ThingDef CTR_SpiritStone;
        public static ThingDef CTR_SpiritStone_Condensed_2X;
        public static ThingDef CTR_SpiritStone_Condensed_3X;
        public static ThingDef CTR_SpiritStone_Condensed_4X;
        public static ThingDef CTR_SpiritStone_Condensed_5X;
        public static ThingDef CTR_SpiritStone_Condensed_6X;

        public static ThingDef CTR_BeastCore;
        public static DamageDef CTR_TribulationLightning;

        public static ThingCategoryDef Artifacts;

        public static BodyPartDef Brain;
        public static BodyPartDef Stomach;

        public static BackstoryDef CTR_ImmortalChild;

        public static TraderKindDef Caravan_CultivationResource;
        public static TraderKindDef Caravan_CultivationPill;
        public static TraderKindDef Caravan_CultivationTechnique;

        //Mote
        public static ThingDef CTR_AbsorbQiOrb;
        public static FleckDef CTR_AbsorbQiOrbPure;
        public static FleckDef CTR_AbsorbQiOrbMetal;
        public static FleckDef CTR_AbsorbQiOrbWater;
        public static FleckDef CTR_AbsorbQiOrbWood;
        public static FleckDef CTR_AbsorbQiOrbFire;
        public static FleckDef CTR_AbsorbQiOrbEarth;

        public static HediffDef CTR_BodyTempering;
        public static HediffDef CTR_MarrowCleansing;
        public static HediffDef CTR_BoneForging;
        public static HediffDef CTR_Qi_Gathering;
        public static HediffDef CTR_FoundationEstablishment;
        public static HediffDef CTR_CoreShaping;
        public static HediffDef CTR_GoldenCore;
        public static HediffDef CTR_NascentSoul;
        public static HediffDef CTR_Transcendent;
        public static HediffDef CTR_HalfStep_Saint;
        public static HediffDef CTR_SaintRealm;
        public static HediffDef CTR_SaintKing;
        public static HediffDef CTR_ImmortalAscension;
        public static HediffDef CTR_TrueImmortal;
        public static HediffDef CTR_ImmortalSaint;
        public static HediffDef CTR_HalfStep_God;
        public static HediffDef CTR_True_God;
        public static HediffDef CTR_Creation_Realm;
        public static HediffDef CTR_OutsidetheDomain;

        public static RecipeDef CTR_MakeAlchemy;
        public static RecipeDef CTR_MakeTalisman;

        public static StatDef CultivationSpeed;
        public static StatDef TribulationChance;

        [MayRequire("zomuro.itssorcery")] public static StatDef TechniqueEfficiency;

        public static StatDef AlchemySuccessChance;
        public static StatDef AlchemyFurnaceQuality;
        public static StatDef RefiningSuccessChance;
        public static StatDef CTR_DantianDamageChance;
        public static StatDef CTR_HealthMultiplier;

        [MayRequire("zomuro.itssorcery")] public static StatDef CTR_QiEnergyCost;

        public static ThinkTreeDef Humanlike;



        public static StatDef CTR_Qi_AbsorptionMultiplier;
        public static StatDef CTR_YangQi_AbsorptionMultiplier;
        public static StatDef CTR_YinQi_AbsorptionMultiplier;
        public static StatDef CTR_ColdQi_AbsorptionMultiplier;
        public static StatDef CTR_MetalQi_AbsorptionMultiplier;
        public static StatDef CTR_WaterQi_AbsorptionMultiplier;
        public static StatDef CTR_WoodQi_AbsorptionMultiplier;
        public static StatDef CTR_FireQi_AbsorptionMultiplier;
        public static StatDef CTR_EarthQi_AbsorptionMultiplier;

        public static DamageDef CTR_Qi_Injury;
        public static DamageDef CTR_Qi_Injury_Explosion;
        public static DamageDef CTR_XuanwuFist;
        public static DamageDef CTR_XuanwuReflect;

        public static ThingCategoryDef Cultivation_Resource;
        public static ThingCategoryDef InertRelics;

        public static EffecterDef QiOrbExplosion_Fire;
        public static EffecterDef QiOrbExplosion_Earth;
        public static EffecterDef QiOrbExplosion_Wood;
        public static EffecterDef QiOrbExplosion_Water;
        public static EffecterDef QiOrbExplosion_Metal;
        public static EffecterDef QiOrbExplosion_Pure;

        public static HediffDef CTR_Xuanwu_Fist;
        public static HediffDef CTR_Xuanwu_Shell;
        public static HediffDef CTR_Xuanwu_ReflectShell;
        public static HediffDef CTR_Xuanwu_ReflectShellCD;
        public static HediffDef CTR_RedFlameFist_CleansingFlame;

        public static TraitDef CTR_MediocreTalent;
        public static TraitDef CTR_GoodTalent;
        public static TraitDef CTR_CultivationProdigy;
        public static TraitDef CTR_PeerlessEmpress;

    }
}
