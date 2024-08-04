using Verse;
using RimWorld;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class CompProperties_GetNearbyPawn : CompProperties
    {
        public int checkInterval;

        public bool onlyTargetCultivator;

        public bool targetOnlyFemale;

        public bool hostileOnly;

        public bool friendlyOnly;

        public float radius;

        public bool isTargetDowned;

        public FactionDef targetSpecificFaction;

        public CompProperties_GetNearbyPawn()
        {
            compClass = typeof(CompGetNearbyPawn);
        }
    }
}
