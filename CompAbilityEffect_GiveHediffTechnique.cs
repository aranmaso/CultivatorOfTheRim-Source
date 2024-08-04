using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class CompAbilityEffect_GiveHediffTechnique : CompAbilityEffect
    {
        public new CompProperties_GiveHediffTechnique Props => (CompProperties_GiveHediffTechnique)props;

        public Pawn caster => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if(Props.havePenaltyStat)
            {
                if(target.Pawn.GetStatValue(Props.penaltyStat) >  Props.penaltyThreshold)
                {
                    if (target.Pawn.health.hediffSet.HasHediff(Props.hediffDef))
                    {
                        Hediff hediff = target.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                        hediff.Severity = Props.penaltySeverity;
                        HediffComp_Disappears hdd = hediff.TryGetComp<HediffComp_Disappears>();
                        if (hdd != null)
                        {
                            hdd.ticksToDisappear = Props.duration;
                        }                        
                    }
                    else
                    {
                        Hediff hediff = Cultivation_Utility.CreateHediff(target.Pawn, Props.hediffDef, Props.duration);
                        hediff.Severity = Props.penaltySeverity;
                        target.Pawn.health.AddHediff(hediff);
                    }
                }
                else
                {
                    if (target.Pawn.health.hediffSet.HasHediff(Props.hediffDef))
                    {
                        Hediff hediff = target.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
                        hediff.Severity = Props.severity;
                        HediffComp_Disappears hdd = hediff.TryGetComp<HediffComp_Disappears>();
                        if (hdd != null)
                        {
                            hdd.ticksToDisappear = Props.duration;
                        }
                    }
                    else
                    {
                        Hediff hediff = Cultivation_Utility.CreateHediff(target.Pawn, Props.hediffDef, Props.duration);
                        hediff.Severity = Props.severity;
                        target.Pawn.health.AddHediff(hediff);
                    }
                }
            }
            else
            {
                if (target.Pawn.health.hediffSet.HasHediff(Props.hediffDef))
                {
                    HediffComp_Disappears hdd = target.Pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef).TryGetComp<HediffComp_Disappears>();
                    if (hdd != null)
                    {
                        hdd.ticksToDisappear = Props.duration;
                    }
                }
                else
                {
                    Hediff hediff = Cultivation_Utility.CreateHediff(target.Pawn, Props.hediffDef, Props.duration);
                    hediff.Severity = Props.severity;
                    target.Pawn.health.AddHediff(hediff);
                }
            }
                   
        }
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if(caster.Faction != null)
            {
                if (target.HasThing && target.Thing is Pawn pawn)
                {
                    if(pawn.Faction == caster.Faction || !pawn.Faction.HostileTo(pawn.Faction))
                    {
                        return true;
                    }
                }
            }    
            return false;
        }
    }
}
