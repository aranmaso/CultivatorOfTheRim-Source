using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class BookOutcomeProperties_TechniqueManual : BookOutcomeProperties
    {
        public HediffDef hediffDef;

        public float learnChance;

        public string benefitString;
        public override Type DoerClass => typeof(BookOutcomeDoerTechniqueManual);
    }
}
