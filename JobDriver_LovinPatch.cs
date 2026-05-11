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
    [HarmonyPatch(typeof(JobDriver_Lovin))]
    [HarmonyPatch("GenerateRandomMinTicksToNextLovin")]
    public class JobDriver_LovinPatch
    {
        public static void Postfix(ref Pawn pawn, JobDriver_Lovin __instance)
        {
            if (!Cultivation_Utility.HaveCultivation(__instance.pawn))
            {
                return;
            }
            if (!Cultivation_Utility.HaveCultivation(__instance.Partner))
            {
                return;
            }
            Hediff_CultivationLevel mainCul = __instance.pawn.FindCultivationLevel();
            Hediff_CultivationLevel partCul = __instance.Partner.FindCultivationLevel();
            int mainLevel = mainCul.cultivationDef.realmPower;
            int partLevel = partCul.cultivationDef.realmPower;
            if (mainLevel >= partLevel)
            {
                if (Rand.Chance(0.5f))
                {
                    if (mainCul.Severity > 0)
                    {
                        float num = mainCul.Severity * (Rand.Range(0.1f, 0.2f));
                        float num2 = num * __instance.Partner.GetStatValueForPawn(CTR_DefOf.CultivationSpeed,__instance.Partner);
                        partCul.Severity += num2;
                    }
                }
                else
                {
                    if (partCul.Severity > 0)
                    {
                        float num = partCul.Severity * (Rand.Range(0.1f, 0.2f));
                        float num2 = num * __instance.pawn.GetStatValueForPawn(CTR_DefOf.CultivationSpeed, __instance.pawn);
                        mainCul.Severity += num2;
                    }
                }
            }
        }
        
    }
}
