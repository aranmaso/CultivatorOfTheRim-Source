using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_EmittingAura : HediffCompProperties
    {
        public DamageDef damageDef;

        public IntRange damageAmount;

        public float radius;

        public EffecterDef effecterDef;

        public int pulseCooldown;

        public bool onlyAffectHostile;

        public int maxTargetAmount = 5;
        public HediffCompProperties_EmittingAura()
        {
            compClass = typeof(HediffComp_EmittingAura);
        }
    }
}
