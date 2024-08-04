using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class RecipeExtension_MixingIngredient : DefModExtension
    {
        public List<RecipeCombination> combinations;

        public List<string> allowedCultivationLevel;

        public ThingDef failedProduct = CTR_DefOf.CTR_JunkPill;

        public IntRange count;
    }

    public class RecipeCombination
    {
        public string firstTag;

        public string secondTag;

        public ThingDef result;
    }
}
