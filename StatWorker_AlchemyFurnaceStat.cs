using RimWorld;
using Verse;

namespace CultivatorOfTheRim
{
    public class StatWorker_AlchemyFurnaceStat : StatWorker
    {

        public override bool ShouldShowFor(StatRequest req)
        {
            if(req.Thing == null)
            {
                return false;
            }
            if(req.Thing.def.tradeTags.NullOrEmpty())
            {
                return false;
            }
            if (req.Thing?.def?.category != ThingCategory.Building)
            {
                return false;
            }
            if (req.Thing.def.tradeTags.Contains("Alchemy_Furnace"))
            {
                return true;
            }
            return false;
        }
    }
}
