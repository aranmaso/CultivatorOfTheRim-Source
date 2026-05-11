using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public static class StaticCollectionCached
    {
        public static List<FactionCultivationDef> FactionCultivationDef = new List<FactionCultivationDef>();

        public static List<CultivationHediffDef> CultivationHediffDefs = new List<CultivationHediffDef>();

        public static Dictionary<HediffDef, int> CultivationRealmPower = new Dictionary<HediffDef, int>();

        public static Dictionary<HediffDef, float> CultivationRealmWeight = new Dictionary<HediffDef, float>();

        public static Dictionary<Pawn,float> PawnHealthScaleCached = new Dictionary<Pawn,float>();

        public static void UpdateHealthScale(Pawn pawn,float value)
        {
            PawnHealthScaleCached.SetOrAdd(pawn, value);
        }
        public static void CleanUpCache()
        {
            int num = 0;
            foreach (var item in PawnHealthScaleCached.Keys.ToList())
            {
                if (item.DestroyedOrNull())
                {
                    PawnHealthScaleCached.Remove(item);
                    num++;
                }
            }
            if (Prefs.DevMode)
            {
                Log.Message($"Cultivator healthScale cur count: " + PawnHealthScaleCached.Count);
                Log.Message($"{num} has been cleared from cache");
            }
        }
    }
}
