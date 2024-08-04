using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class CompProperties_EscapeTheDomain : CompProperties_UseEffect
    {

        public CompProperties_EscapeTheDomain()
        {
            compClass = typeof(CompUseEffect_EscapeTheDomain);
        }
    }
}
