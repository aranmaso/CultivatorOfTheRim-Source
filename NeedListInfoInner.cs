using RimWorld;
using UnityEngine;
using Verse;
using static Verse.DamageInfo;

namespace CultivatorOfTheRim
{
    public class NeedListInfoInner
    {
        public Need need;

        public float minValue;

        public NeedListInfoInner(Need needIn, float min)
        {
            need = needIn;            
            minValue = min;
        }
    }
}
