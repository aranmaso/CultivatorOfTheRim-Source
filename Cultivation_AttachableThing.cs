using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class Cultivation_AttachableThing : AttachableThing
    {
        public string inspectString = "";
        public override string InspectStringAddon => inspectString;
    }
}
