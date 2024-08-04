using RimWorld;
using Verse;

namespace CultivatorOfTheRim
{
    public class StatWorker_Cultivation : StatWorker
    {

        public override bool ShouldShowFor(StatRequest req)
        {
            if(req.Thing == null)
            {
                return false;
            }
            if(req.Thing is Plant)
            {
                return false;
            }
            if (req.Thing?.def?.category != ThingCategory.Pawn)
            {
                return false;
            }
            if (req.Thing is Pawn pawn)
            {
                if(pawn.def.race.Humanlike)
                {
                    if(Cultivation_Utility.HaveCultivation(pawn))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
