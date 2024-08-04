using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("PreApplyDamage")]
    public class Pawn_PreApplyDamage_DantianDamage
    {
        private static SimpleCurve extraChanceCurve = new SimpleCurve()
        {
            new CurvePoint (0f,0f),
            new CurvePoint (5f,0.25f),
            new CurvePoint (10,0.5f)
        };
        private static void Postfix(ref DamageInfo dinfo, ref bool absorbed, Pawn __instance)
        {
            if (dinfo.Def == DamageDefOf.Extinguish || dinfo.Def == DamageDefOf.EMP) return;
            if (__instance == null) return;
            if(__instance.RaceProps.IsMechanoid || __instance.RaceProps.Animal) return;
            if(dinfo.HitPart == null) return;
            if(!Cultivation_Utility.HaveCultivation(__instance)) return;

            bool hitTorso = false;
            bool hitStomach = false;
            if(dinfo.HitPart.def == BodyPartDefOf.Torso)
            {
                hitTorso = true;
            }
            if(dinfo.HitPart.def == CTR_DefOf.Stomach)
            {
                hitStomach = true;
            }
            float dantianDamageChance = __instance.GetStatValue(CTR_DefOf.CTR_DantianDamageChance);
            float extraChance = extraChanceCurve.Evaluate(dinfo.Amount);
            float finalChance = dantianDamageChance + extraChance;
            if (hitTorso && Rand.Value < finalChance)
            {
                MoteMaker.ThrowText(__instance.Position.ToVector3(), __instance.Map, "Dantian Hit!", Color.red);
                if (!__instance.health.hediffSet.HasHediff(CTR_DefOf.CTR_DantianDamage))
                {
                    BodyPartRecord br = __instance.RaceProps.body.AllParts.FirstOrDefault(x => x.def == CTR_DefOf.Stomach);
                    Hediff dantinaDamage = HediffMaker.MakeHediff(CTR_DefOf.CTR_DantianDamage, __instance, br);
                    dantinaDamage.Severity = 0.1f;
                    __instance.health.AddHediff(dantinaDamage);
                }
                else
                {
                    __instance.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_DantianDamage).Severity += 0.1f;
                }
            }
            if (hitStomach && Rand.Value < finalChance + 0.1f)
            {
                MoteMaker.ThrowText(__instance.Position.ToVector3(), __instance.Map, "Dantian Hit!", Color.red);
                if (!__instance.health.hediffSet.HasHediff(CTR_DefOf.CTR_DantianDamage))
                {
                    Hediff dantinaDamage = HediffMaker.MakeHediff(CTR_DefOf.CTR_DantianDamage, __instance, dinfo.HitPart);
                    dantinaDamage.Severity = 0.1f;
                    __instance.health.AddHediff(dantinaDamage);
                }
                else
                {
                    __instance.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_DantianDamage).Severity += 0.1f;
                }
            }
        }
    }
}
