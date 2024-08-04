using Verse;
using RimWorld;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class RecipeExtension_Alchemy : DefModExtension
    {
        public List<string> allowedCultivationLevel;

        public ThingDef successfulProduct;

        public ThingDef failedProduct;

        public IntRange count;
    }
}
