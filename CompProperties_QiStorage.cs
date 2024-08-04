using Verse;
using RimWorld;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class CompProperties_QiStorage : CompProperties
    {
        public int maxStorage;

        public float usageMultiplier;

        public CompProperties_QiStorage()
        {
            compClass = typeof(CompQiStorage);
        }
    }
}
