using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_QiDamage
    {
        public static SimpleCurve cultivationToDamageCurve = new SimpleCurve()
        {
            new CurvePoint(1,0.10f),
            new CurvePoint(19, 0.20f),
        };
        private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {            
            if(dinfo.Instigator is Pawn attacker)
            {
                if(dinfo.Def == CTR_DefOf.CTR_Qi_Injury || dinfo.Def == CTR_DefOf.CTR_Qi_Injury_Explosion)
                {
                    return;
                }
                if(dinfo.Def == DamageDefOf.Extinguish || dinfo.Def == DamageDefOf.EMP)
                {
                    return;
                }
                if(dinfo.Amount <= 0)
                {
                    return;
                }
                if (attacker.RaceProps.IsMechanoid) return;
                if (!Cultivation_Utility.HaveCultivation(attacker))
                {
                    return;
                }
                Hediff attackCulLevel = Cultivation_Utility.FindCultivationLevel(attacker);
                if (attackCulLevel == null)
                {
                    return;
                }
                int num = Cultivation_Utility.realmListAll[attackCulLevel.def];
                if (num > 3)
                {
                    //float rand = Rand.Range(0.01f, 1.25f);
                    float num2 = cultivationToDamageCurve.Evaluate(num);
                    float num3 = dinfo.Amount * num2;
                    DamageInfo newdinfo = new DamageInfo(CTR_DefOf.CTR_Qi_Injury,num3,dinfo.ArmorPenetrationInt,dinfo.Angle,attacker);
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
