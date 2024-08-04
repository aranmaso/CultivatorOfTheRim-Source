using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(WorkGiver_GrowerSow), "ExtraRequirements")]
    public static class WorkGiver_GrowerSow_CultivatorPatch
    {
        private static bool Postfix(bool __result, WorkGiver_GrowerSow __instance, ref IPlantToGrowSettable settable, ref Pawn pawn)
        {
            if(CultivatorOfTheRimMod.settings.isSpiritPlantRestrictedByCultivationLevel)
            {
                if (settable is Zone_Growing zoneGrow)
                {
                    ThingDef plantDef = zoneGrow.GetPlantDefToGrow();                    
                    if (plantDef != null)
                    {
                        if (plantDef.thingClass == typeof(Plant_SpiritPlant))
                        {
                            string requireTag = plantDef.GetModExtension<PlantExtension_SpiritPlant>().allowedCultivation;
                            if (!Cultivation_Utility.HaveCultivation(pawn))
                            {
                                return false;
                            }
                            else
                            {
                                Hediff pawnCultivation = Cultivation_Utility.FindCultivationLevel(pawn);
                                if(!pawnCultivation.def.tags.Contains(requireTag))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }            
            return __result;

        }
    }
}

