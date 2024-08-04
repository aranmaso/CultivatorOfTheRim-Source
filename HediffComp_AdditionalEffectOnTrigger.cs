using ItsSorceryFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultivatorOfTheRim
{
    public class HediffComp_AdditionalEffectOnTrigger : HediffComp
    {
        public HediffCompProperties_AdditionalEffectOnTrigger Props => (HediffCompProperties_AdditionalEffectOnTrigger)props;

        public int cooldownTick = 0;

        /*public SorcerySchema sorcerySchema;
        public SorcerySchema sorcerySchemaGet
        {
            get
            {
                if(Props.sorcerySchemaDef != null)
                {
                    if(sorcerySchema == null)
                    {
                        sorcerySchema = SorcerySchemaUtility.GetSorcerySchemaList(Pawn).Where(x => x.def == Props.sorcerySchemaDef).FirstOrDefault();
                    }
                }
                return sorcerySchema;
            }
        }*/
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if(cooldownTick > 0)
            {
                cooldownTick--;
            }
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref cooldownTick,"cooldownTick",0);
        }
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (dinfo.Instigator == null) return;
            if (dinfo.Instigator == Pawn) return;
            if (!Props.onAttacked) return;
            if (Props.bonusDamageDef != null)
            {
                if (Props.onlyUnarmed && Pawn.equipment.Primary != null)
                {
                    return;
                }
                /*if (ModsConfig.IsActive("zomuro.itssorcery") && Props.energyStat != null)
                {
                    float curEnergy = SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).currentEnergy;
                    if (curEnergy <= 0 || curEnergy < Props.energyCost)
                    {
                        return;
                    }
                    SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).currentEnergy -= Props.energyCost;
                    MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).def.LabelCap + ": -" + Props.energyCost);
                }*/
                DamageInfo newdinfo = new DamageInfo(Props.bonusDamageDef, Props.damageAmount.RandomInRange, Props.armorPenetration, instigator: Pawn);
                dinfo.Instigator.TakeDamage(newdinfo);
            }
            if (Props.projectileDef != null)
            {
                IntVec3 spawnPos = new IntVec3();
                if (Props.spawnOnUser)
                {
                    spawnPos = Pawn.Position;
                }
                else if (Props.spawnOnTarget)
                {
                    spawnPos = dinfo.Instigator.Position;
                }
                if (!Props.spawnOnTarget && !Props.spawnOnUser)
                {
                    spawnPos = dinfo.Instigator.Position;
                }
                spawnPos += Props.spawnOffset;
                for (int i = 0; i < Props.projectileCount.RandomInRange; i++)
                {
                    IntVec3 newPos = spawnPos;
                    if (Props.randomRadius > 0)
                    {
                        newPos = GenRadial.RadialCellsAround(spawnPos, Props.randomRadius, true).RandomElement();
                    }
                    Projectile projectile = (Projectile)GenSpawn.Spawn(Props.projectileDef, newPos.ClampInsideMap(Pawn.Map), Pawn.Map);
                    if (dinfo.Instigator != null)
                    {
                        projectile.Launch(Pawn, dinfo.Instigator, dinfo.Instigator, ProjectileHitFlags.IntendedTarget);
                    }
                }
            }
        }
        public override string CompLabelInBracketsExtra
        {
            get
            {
                if(cooldownTick > 0)
                {
                    return cooldownTick.ToStringTicksToPeriod(allowSeconds: true, shortForm: true, canUseDecimals: true, allowYears: true, true);
                }
                return base.CompLabelInBracketsExtra;
            }
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if(Props.isSelfResurrect)
            {
                if(cooldownTick > 0)
                {
                    return;
                }
                Map map = parent.pawn.Corpse.Map;
                IntVec3 pos = parent.pawn.Corpse.Position;
                if (map != null)
                {
                    MoteMaker.ThrowText(parent.pawn.Corpse.Position.ToVector3(), map, "Revived");
                    ResurrectionUtility.TryResurrect(parent.pawn.Corpse.InnerPawn);
                    Pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(map, pos, 0, Rand.Range(1, 4)));
                    cooldownTick = Props.cooldown;
                }
            }            
        }
        public override void Notify_PawnUsedVerb(Verb verb, LocalTargetInfo target)
        {
            /*if (verb.GetType() == typeof(Verb_BeatFire)) return;
            if (verb.GetType() == typeof(Verb_CastAbility)) return;
            if (verb.GetType() == typeof(Verb_CastPsycast)) return;
            if (verb.GetType() == typeof(Verb_CastAbilityTouch)) return;
            if (verb.GetType() == typeof(Verb_CastAbilityJump)) return;
            if (verb.GetType() == typeof(Verb_DeployBroadshield)) return;
            if (verb.GetType() == typeof(Verb_Jump)) return;
            if (verb.GetType() == typeof(Verb_Ignite)) return;
            if (verb.GetType() == typeof(Verb_FirefoamPop)) return;
            if (verb.GetType() == typeof(Verb_Spawn)) return;*/
            base.Notify_PawnUsedVerb(verb, target);
            if (!Props.onAttack) return;
            if (Props.cooldown > 0 && cooldownTick > 0) return;
            if(Props.bonusDamageDef != null)
            {
                if (Props.onlyUnarmed && Pawn.equipment?.Primary != null)
                {
                    return;
                }
                if (Props.onlyMelee && (verb is not Verb_MeleeAttack)) return;
                if (verb is Verb_MeleeAttack
                || verb is Verb_Shoot
                || verb is Verb_ShootBeam
                || verb is Verb_LaunchProjectile)
                {
                    
                    DamageInfo dinfo = new DamageInfo(Props.bonusDamageDef, Props.damageAmount.RandomInRange, Props.armorPenetration, instigator: Pawn);
                    if (target.HasThing && target.Thing != null)
                    {
                        target.Thing.TakeDamage(dinfo);
                    }
                }
                /*if (ModsConfig.IsActive("zomuro.itssorcery") && Props.energyStat != null)
                   {
                       float curEnergy = SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).currentEnergy;
                       if (curEnergy <= 0 || curEnergy < Props.energyCost)
                       {
                           return;
                       }
                       SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).currentEnergy -= Props.energyCost;
                       MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).def.LabelCap + ": -" + Props.energyCost);
                   }*/
            }
            if (Props.projectileDef != null)
            {
                if (verb is Verb_MeleeAttack
                || verb is Verb_Shoot
                || verb is Verb_ShootBeam
                || verb is Verb_LaunchProjectile)
                {
                    if (Rand.Value > Props.chance)
                    {
                        return;
                    }
                    if (Props.onlyUnarmed && Pawn.equipment.Primary != null)
                    {
                        return;
                    }
                    if (Props.soundOnSpawn != null)
                    {
                        Props.soundOnSpawn.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map));
                    }
                    /*if (Props.energyStat != null)
                    {
                        float curEnergy = SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).currentEnergy;
                        if (curEnergy <= 0 || curEnergy < Props.energyCost)
                        {
                            return;
                        }
                        SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).currentEnergy -= Props.energyCost;
                        MoteMaker.ThrowText(Pawn.DrawPos,Pawn.Map, SorcerySchemaUtility.GetEnergyTracker(sorcerySchemaGet, Props.energyStat).def.LabelCap + ": -" + Props.energyCost);
                    }*/
                    IntVec3 spawnPos = new IntVec3();
                    if (Props.spawnOnUser)
                    {
                        spawnPos = Pawn.Position;
                    }
                    else if (Props.spawnOnTarget)
                    {
                        if(target.HasThing)
                        {
                            spawnPos = target.Thing.Position;
                        }
                        else
                        {
                            spawnPos = target.Cell;
                        }
                    }
                    if (!Props.spawnOnTarget && !Props.spawnOnUser)
                    {
                        spawnPos = target.Cell;
                    }
                    spawnPos += Props.spawnOffset;
                    for (int i = 0; i < Props.projectileCount.RandomInRange; i++)
                    {
                        IntVec3 newPos = spawnPos;
                        if (Props.randomRadius > 0)
                        {
                            newPos = GenRadial.RadialCellsAround(spawnPos, Props.randomRadius, true).RandomElement();
                        }
                        Projectile projectile = (Projectile)GenSpawn.Spawn(Props.projectileDef, newPos.ClampInsideMap(Pawn.Map), Pawn.Map);
                        if (target.HasThing)
                        {
                            if (Rand.Value > 0.5f)
                            {
                                IntVec3 newTarget = GenRadial.RadialCellsAround(target.Thing.Position, 3f, true).Where(x => !x.Fogged(Pawn.Map)).RandomElement();
                                projectile.Launch(Pawn, newTarget.ClampInsideMap(Pawn.Map), newTarget.ClampInsideMap(Pawn.Map), ProjectileHitFlags.NonTargetPawns);
                            }
                            else
                            {
                                projectile.Launch(Pawn, target.Thing, target.Thing, ProjectileHitFlags.IntendedTarget);
                            }
                        }
                        else
                        {
                            if (Rand.Value > 0.5f)
                            {
                                IntVec3 newTarget = GenRadial.RadialCellsAround(target.Cell, 3f, true).Where(x => !x.Fogged(Pawn.Map)).RandomElement();
                                projectile.Launch(Pawn, newTarget.ClampInsideMap(Pawn.Map), newTarget.ClampInsideMap(Pawn.Map), ProjectileHitFlags.NonTargetPawns);
                            }
                            else
                            {
                                projectile.Launch(Pawn, target.Cell, target.Cell, ProjectileHitFlags.IntendedTarget);
                            }
                        }
                    }
                    cooldownTick = Props.cooldown;
                }
               
                

            }
        }
    }
}
