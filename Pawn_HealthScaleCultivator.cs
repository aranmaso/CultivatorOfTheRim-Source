using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Pawn), "HealthScale", MethodType.Getter)]
    public static class Pawn_HealthScaleCultivator
    {
        public static void Postfix(ref float __result, Pawn __instance)
        {
            if(!CultivatorOfTheRimMod.settings.isCultivationAffectBodyHP)
            {
                return;
            }
            if (__instance != null)
            {
                //__result *= __instance.GetStatValue(CTR_DefOf.CTR_HealthMultiplier,cacheStaleAfterTicks: 60);
                /* the GetStatValue suck ass. even with cacheStaleAfterTicks */
                if (Cultivation_Utility.HaveCultivationOutHediff(__instance,out var hediff))
                {
                    __result *= hediff.TryGetComp<HediffComp_Cultivation>().HealthScaleMul;
                   //__result = __instance.ageTracker.CurLifeStage.healthScaleFactor * __instance.RaceProps.baseHealthScale * hediff.TryGetComp<HediffComp_Cultivation>().HealthScaleMul;
                }                
            }
        }
    }
}
