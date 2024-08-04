using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike))]
    [HarmonyPatch("DoStrike")]
    public class WeatherEvent_LightningStrikeGiveSpiritItemPatch
    {
        private static void Postfix(ref IntVec3 strikeLoc, ref Map map, ref Mesh boltMesh)
        {
            if (Rand.Value <= CTR_DefOf.CTR_AzureFragment.generateCommonality && !strikeLoc.Fogged(map) && !strikeLoc.Impassable(map))
            {
                Thing thing = ThingMaker.MakeThing(CTR_DefOf.CTR_AzureFragment);
                thing.HitPoints += 10;
                GenSpawn.Spawn(thing, strikeLoc, map);
            }
            else if (Rand.Value <= CultivatorOfTheRimMod.settings.tribRemnantChance && !strikeLoc.Fogged(map) && !strikeLoc.Impassable(map))
            {
                Thing thing = ThingMaker.MakeThing(CTR_DefOf.CTR_TribulationRemnantPill);
                thing.HitPoints += 10;
                GenSpawn.Spawn(thing, strikeLoc, map);
            }            
            else
            {
                return;
            }
        }
    }
}
