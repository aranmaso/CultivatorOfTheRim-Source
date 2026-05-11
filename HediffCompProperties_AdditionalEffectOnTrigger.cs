using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

        public string uiIcon;

        public bool hasUiIcon;

        /*[MayRequire("zomuro.itssorcery")]
        public 
        sorcerySchemaDef;

        public StatDef energyStat;

        public float energyCost;*/

        public Texture2D icon;

        public HediffCompProperties_AdditionalEffectOnTrigger()
        {
            compClass = typeof(HediffComp_AdditionalEffectOnTrigger);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            if (hasUiIcon && uiIcon == null)
            {
                yield return "uiIcon property is null";
            }
        }

        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            if (hasUiIcon)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                { 
                    icon = ContentFinder<Texture2D>.Get(uiIcon);
                });
            }
        }
    }
}
