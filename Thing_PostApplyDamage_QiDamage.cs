using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("PreApplyDamage")]
    public class Thing_PostApplyDamage_QiDamage
    {
        public static SimpleCurve cultivationToDamageCurve = new SimpleCurve()
        {
            new CurvePoint(1,0.10f),
            new CurvePoint(19, 0.20f),
        };
        public static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Thing __instance)
        {
            if (dinfo.Instigator is Pawn attacker)
            {
                if (dinfo.Def == CTR_DefOf.CTR_Qi_Injury || dinfo.Def == CTR_DefOf.CTR_Qi_Injury_Explosion)
                {
                    return;
                }
                if (dinfo.Def == DamageDefOf.Extinguish 
                    || dinfo.Def == DamageDefOf.EMP 
                    || dinfo.Def == DamageDefOf.Mining 
                    || dinfo.Def == DamageDefOf.SurgicalCut 
                    || dinfo.Def == DamageDefOf.Flame
                    || dinfo.Def == DamageDefOf.ExecutionCut)
                {
                    return;
                }
                if (dinfo.Amount <= 0)
                {
                    return;
                }
                if(attacker.Dead) return;
                if (attacker.RaceProps.IsMechanoid) return;
                if (!attacker.HaveCultivation())
                {
                    return;
                }
                Hediff_CultivationLevel attackCulLevel = attacker.FindCultivationLevel();
                if (attackCulLevel == null)
                {
                    return;
                }
                int num = attackCulLevel.cultivationDef.realmPower;
                if (num > 3)
                {
                    //float rand = Rand.Range(0.01f, 1.25f);
                    float num2 = cultivationToDamageCurve.Evaluate(num);
                    float num3 = dinfo.Amount * num2;
                    DamageInfo newdinfo = new DamageInfo(CTR_DefOf.CTR_Qi_Injury, num3, dinfo.ArmorPenetrationInt, dinfo.Angle, attacker);
                    __instance.TakeDamage(newdinfo);
                }
            }
            else
            {
                return;
            }

        }
    }
}
