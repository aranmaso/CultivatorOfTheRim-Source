using RimWorld;
using Verse;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class CompProperties_ApparelHediffGiver : CompProperties
    {
        public List<HediffDef> hediffDefs;

        public float severity = 0.5f;

        public bool removeOnUnequip = true;

        public CompProperties_ApparelHediffGiver()
        {
            compClass = typeof(CompApparelHediffGiver);
        }
    }
}
