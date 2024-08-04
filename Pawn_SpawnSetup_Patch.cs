using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("SpawnSetup")]
    public class Pawn_SpawnSetup_Patch
    {

        private static Dictionary<TraitDef, float> talentTrait = new Dictionary<TraitDef, float>()
        {
            {CTR_DefOf.CTR_MediocreTalent, 0.75f },
            {CTR_DefOf.CTR_GoodTalent,0.5f },
            {CTR_DefOf.CTR_CultivationProdigy,0.1f}
        };
        private static void Postfix(Pawn __instance)
        {
            if(!__instance.RaceProps.Humanlike)
            {
                return;
            }
            if(__instance.story.traits == null)
            {
                return;
            }
            if(!__instance.HasTalentTrait())
            {
                __instance.story.traits.GainTrait(new Trait(talentTrait.RandomElementByWeight(x => x.Value).Key));
            }
        }
    }
}
