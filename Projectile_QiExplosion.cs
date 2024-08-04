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
    public class Projectile_QiExplosion : Projectile_Explosive
    {
        public override int DamageAmount
        {
            get
            {
                if (Launcher is Pawn pawnLauncher)
                {
                    if (Cultivation_Utility.HaveCultivation(pawnLauncher))
                    {
                        return Mathf.RoundToInt(def.projectile.GetDamageAmount(weaponDamageMultiplier) * launcher.GetStatValue(CTR_DefOf.CultivationSpeed));
                    }
                    else
                    {
                        return 0;
                    }
                }
                return base.DamageAmount;
            }
        }
        protected override void Explode()
        {
            Map map = base.Map;
            IntVec3 pos = base.Position;
            Destroy();
            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(base.Position, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, DamageAmount, ArmorPenetration, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater, postExplosionSpawnChance: def.projectile.postExplosionSpawnChance, postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount, postExplosionGasType: def.projectile.postExplosionGasType, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, affectedAngle: null, doVisualEffects: true, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, excludeRadius: 0f, doSoundEffects: true, screenShakeFactor: def.projectile.screenShakeFactor);
            foreach(Thing item in GenRadial.RadialDistinctThingsAround(pos,map,def.projectile.explosionRadius * 2,true))
            {
                if(!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Contains("Qi_Source"))
                {
                    if(Rand.Value < 0.5f)
                    {
                        float num = GetTypeAndColor(item.def, out var type);
                        num += 1f;
                        num *= 2f;
                        //MoteMaker.ThrowText(item.DrawPos,map,num.ToString("0.000"));
                        Effecter effecter = GetEffecter(type).Spawn(item.Position, map);
                        effecter.Cleanup();
                        GenExplosion.DoExplosion(item.Position, map, num, CTR_DefOf.CTR_Qi_Injury_Explosion, launcher, Mathf.RoundToInt(2 * num), ArmorPenetration);
                    }                                    
                }
            }
            
        }

        public float GetTypeAndColor(ThingDef input,out string type)
        {
            float num = 0f;
            string tag = "N/A";
            foreach(var item in input.tradeTags)
            {
                tag = Cultivation_Utility.getQiTypeString(item);                
                if(tag != "N/A" && tag != null)
                {
                    break;
                }
            }
            foreach(var item in input.tradeTags)
            {
                if(Cultivation_Utility.getItemQiValueForPawn(item) > 0)
                {
                    num = Cultivation_Utility.getItemQiValueForPawn(item);
                    break;
                }
            }
            type = tag;
            return num;
        }

        public EffecterDef GetEffecter(string input)
        {
            EffecterDef effecterDef = null;
            switch (input)
            {
                case null:
                    break;
                case "Neutral_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Pure;
                    break;
                case "Yang_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Pure;
                    break;
                case "Yin_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Pure;
                    break;
                case "Cold_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Water;
                    break;
                case "Metal_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Metal;
                    break;
                case "Water_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Water;
                    break;
                case "Wood_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Wood;
                    break;
                case "Fire_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Fire;
                    break;
                case "Earth_Qi":
                    effecterDef = CTR_DefOf.QiOrbExplosion_Earth;
                    break;
            }

            return effecterDef;
        }
    }
}
