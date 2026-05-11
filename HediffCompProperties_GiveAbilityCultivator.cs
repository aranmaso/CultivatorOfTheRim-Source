using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_GiveAbilityCultivator : HediffCompProperties
    {
        public AbilityDef abilityDef;

        public HediffCompProperties_GiveAbilityCultivator()
        {
            compClass = typeof(HediffComp_GiveAbilityCultivator);
        }
    }
}
