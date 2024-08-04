using RimWorld;
using Verse;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class HediffExtension_TechniqueEffect : DefModExtension
    {
        public bool dealDamageWhenAttacked;

        public bool onlyMeleeAttack;

        public float percentOfIncomingDamageAsAmount;

        public IntRange damageAmount;

        public float percentOfDamage;

        public DamageDef damageDef;

        public bool reduceIncomingDamage;

        public float reducePercent;

        public int reduceAmount;

        public bool extraDamageOnAttack;

        public bool onlyUnarmed;

        public bool chanceToTakeNoDamage;

        public float chance;

        public bool extraProjectileOnAttack;

        public ThingDef projectileDef;
    }
}
