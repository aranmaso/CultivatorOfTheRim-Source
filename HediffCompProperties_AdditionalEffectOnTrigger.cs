using ItsSorceryFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_AdditionalEffectOnTrigger : HediffCompProperties
    {
        public ThingDef projectileDef;

        public IntRange projectileCount;

        public DamageDef bonusDamageDef;

        public IntRange damageAmount;

        public float armorPenetration;

        public float chance = 1f;

        public bool onAttack;

        public bool onAttacked;

        public bool onlyMelee;

        public bool onlyUnarmed;

        public bool spawnOnTarget;

        public bool spawnOnUser;

        public IntVec3 spawnOffset;

        public float randomRadius;

        public int cooldown;

        public SoundDef soundOnSpawn;

        public bool isSelfResurrect;

        /*[MayRequire("zomuro.itssorcery")]
        public SorcerySchemaDef sorcerySchemaDef;

        public StatDef energyStat;

        public float energyCost;*/

        public HediffCompProperties_AdditionalEffectOnTrigger()
        {
            compClass = typeof(HediffComp_AdditionalEffectOnTrigger);
        }
    }
}
