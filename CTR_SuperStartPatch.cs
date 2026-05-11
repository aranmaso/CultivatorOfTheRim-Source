using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class CTR_SuperStartPatch : Mod
    {
        public CTR_SuperStartPatch(ModContentPack content) : base(content)
        {
            new Harmony("FarmerJoe.CultivatorOfTheRim").PatchAll();
        }
    }
}
