using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_Technique_XuanwuFist
    {
        private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {
            if(__instance == null)
            {
                return;
            }
            if(dinfo.Instigator is Pawn attacker)
            {
                Hediff firstOfHediffDef = attacker.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_Xuanwu_Fist);
                if (firstOfHediffDef == null)
                {
                    return;
                }
                HediffExtension_TechniqueEffect modExtension = firstOfHediffDef.def.GetModExtension<HediffExtension_TechniqueEffect>();
                if (firstOfHediffDef != null)
                {
                    if(modExtension.extraDamageOnAttack)
                    {
                        if (dinfo.Def == modExtension.damageDef || dinfo.Def == CTR_DefOf.CTR_Qi_Injury)
                        {
                            return;
                        }
                        if (dinfo.Def == DamageDefOf.Extinguish || dinfo.Def == DamageDefOf.EMP)
                        {
                            return;
                        }
                        if (dinfo.Amount <= 0)
                        {
                            return;
                        }
                        if (dinfo.Weapon != null)
                        {
                            return;
                        }
                        float num = dinfo.Amount;
                        float num2 = num * modExtension.percentOfDamage;
                        if (dinfo.Weapon == null)
                        {
                            __instance.TakeDamage(new DamageInfo(modExtension.damageDef, num2, dinfo.ArmorPenetrationInt, instigator: dinfo.Instigator, hitPart: dinfo.HitPart));
                        }
                        else
                        {
                            return;
                        }
                    }                    
                }
                else
                {
                    return;
                }
            }                        
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_Technique_XuanwuShell
    {
        private static void Prefix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {
            if (__instance == null)
            {
                return;
            }
            Hediff firstOfHediffDef = __instance.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_Xuanwu_Shell);
            if (firstOfHediffDef == null)
            {
                return;
            }
            HediffExtension_TechniqueEffect modExtension = firstOfHediffDef.def.GetModExtension<HediffExtension_TechniqueEffect>();
            if (firstOfHediffDef != null)
            {
                if(modExtension.chanceToTakeNoDamage)
                {
                    if (Rand.Value <= modExtension.chance)
                    {
                        absorbed = true;
                        MoteMaker.ThrowText(__instance.DrawPos, __instance.Map, "Invincible Shell: active");
                    }
                }                
                else
                {
                    return;
                }
            }
        }        
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_Technique_XuanwuReflect
    {        
        private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {
            if (__instance == null)
            {
                return;
            }
            if(dinfo.Instigator == null)
            {
                return;
            }
            if(dinfo.Instigator == __instance)
            {
                return;
            }
            Hediff firstOfHediffDef = __instance.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_Xuanwu_ReflectShell);
            if(firstOfHediffDef == null)
            {
                return;
            }
            HediffExtension_TechniqueEffect modExtension = firstOfHediffDef.def.GetModExtension<HediffExtension_TechniqueEffect>();
            if (firstOfHediffDef != null && !__instance.health.hediffSet.HasHediff(CTR_DefOf.CTR_Xuanwu_ReflectShellCD))
            {
                if(modExtension.dealDamageWhenAttacked)
                {
                    if (dinfo.Def == CTR_DefOf.CTR_XuanwuReflect)
                    {
                        return;
                    }
                    float num = dinfo.Amount;
                    num *= modExtension.percentOfIncomingDamageAsAmount;
                    dinfo.SetAmount(dinfo.Amount * modExtension.reducePercent);
                    dinfo.Instigator.TakeDamage(new DamageInfo(CTR_DefOf.CTR_XuanwuReflect, num, 2f));
                    Hediff hediff = Cultivation_Utility.CreateHediff(__instance, CTR_DefOf.CTR_Xuanwu_ReflectShellCD, 600);
                    __instance.health.AddHediff(hediff);
                }                
            }
        }        
    }
}
