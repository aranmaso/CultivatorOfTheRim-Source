using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffComp_GiveAbilityCultivator : HediffComp
    {
        public HediffCompProperties_GiveAbilityCultivator Props => (HediffCompProperties_GiveAbilityCultivator)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (Props.abilityDef != null)
            {
                if (Pawn.RaceProps.Humanlike)
                {
                    Pawn.abilities?.GainAbility(Props.abilityDef);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (Props.abilityDef != null)
            {
                if (Pawn.RaceProps.Humanlike)
                {
                    parent.pawn.abilities?.RemoveAbility(Props.abilityDef);
                }
            }
        }
    }
}
