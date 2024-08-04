using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffComp_EmittingAura : HediffComp
    {
        public HediffCompProperties_EmittingAura Props => (HediffCompProperties_EmittingAura)props;

        public int cooldownTick = 0;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref cooldownTick,"cooldownTick",0);
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if(cooldownTick > 0)
            {
                cooldownTick--;
            }
            else
            {
                DoPulse();
            }
        }
        public void DoPulse()
        {
            cooldownTick = Props.pulseCooldown;
            if (Props.effecterDef != null)
            {
                Effecter effecter = Props.effecterDef.Spawn(Pawn.Position,Pawn.Map);
                effecter.Cleanup();
            }
            DamageInfo dinfo = new DamageInfo(Props.damageDef,Props.damageAmount.RandomInRange);
            int num = 0;
            foreach(Pawn item in Cultivation_Utility.GetNearbyPawnFriendAndFoe(Pawn.Position,Pawn.Map,Props.radius))
            {
                if(item == Pawn)
                {
                    continue;
                }
                if(Props.onlyAffectHostile && (!item.Faction.HostileTo(Pawn.Faction) || !item.HostileTo(Pawn)))
                {
                    continue;
                }
                item.TakeDamage(dinfo);
                num++;
                if(num > Props.maxTargetAmount)
                {
                    break;
                }
            }
        }
    }
}
