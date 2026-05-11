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
    [HarmonyPatch("PostApplyDamage")]
    public class Thing_PostApplyDamage_BodyCultivator
    {
        public static void Postfix(DamageInfo dinfo, float totalDamageDealt, Thing __instance)
        {
            if (dinfo.Instigator == null) return;
            if (dinfo.Instigator is not Pawn pawn) return;
            if (pawn.HaveBodyCultivationOutHediff(out var hediff))
            {
                if (dinfo.Def.isRanged) return;
                if(dinfo.Def == DamageDefOf.Flame) return;
                if (hediff.comp_BodyCultivation.Props.severityPerDamageDealt.min > 0)
                {
                    float num = hediff.comp_BodyCultivation.Props.severityPerDamageDealt.RandomInRange;
                    num *= totalDamageDealt;
                    num *= hediff.comp_BodyCultivation.CultivationSpeed;
                    if (IsJobBasedDamage(dinfo.Def) && CultivatorOfTheRimMod.settings.isBodyCulGainXpThroughJob)
                    {
                        if (CultivatorOfTheRimMod.settings.isBodyCulGainXpThroughJob) 
                        {
                            num *= Rand.Range(0.1f,0.25f);
                        }
                        else
                        {
                            return;
                        }
                    }
                    hediff.Severity += num;
                }
            }
        }

        public static bool IsJobBasedDamage(DamageDef damageDef)
        {
            if(damageDef == DamageDefOf.Mining) return true;
            if(damageDef == DamageDefOf.SurgicalCut) return true; 
            if(damageDef == DamageDefOf.ExecutionCut) return true;
            return false;
        }
    }
}
