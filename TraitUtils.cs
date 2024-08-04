using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{   
    public static class TraitUtils
    {
        private static List<TraitDef> talentTrait = new List<TraitDef>()
        {
            CTR_DefOf.CTR_MediocreTalent,
            CTR_DefOf.CTR_GoodTalent,
            CTR_DefOf.CTR_CultivationProdigy
        };
        public static bool HasTalentTrait(this Pawn pawn)
        {
            if (pawn.story.traits.allTraits.Any(x => talentTrait.Contains(x.def)))
            {
                return true;
            }
            return false;
        }
    }
}
