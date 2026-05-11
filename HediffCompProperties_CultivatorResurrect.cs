using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_CultivatorResurrect : HediffCompProperties
    {
        public int livesLeft = 1;

        public bool replenishLivesOvertime = true;

        public bool replenishToFull = false;

        public int timeToReplenish = 60000;

        public HediffCompProperties_CultivatorResurrect()
        {
            compClass = typeof(HediffComp_CultivatorResurrect);
        }
    }
}
