using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateInitialHediffs")]
    public class PawnGenerator_GenerateInitialHediffsPatch
    {
        private static void Postfix(Pawn pawn)
        {            
            if (pawn == null) return;            
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }            
            if (pawn.story == null)
            {
                return;
            }
            Cultivation_Utility.TryGiveCultivationBasedOnBackstory(pawn);  
        }
    }
}
