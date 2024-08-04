using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Assertions.Must;
using Verse;

namespace CultivatorOfTheRim
{
    public class CompBeastCore : ThingComp
    {
        public ThingDef ownerDef;

        public string ownerName;

        public HediffDef ownerCultivation;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref ownerDef, "ownerDef");
            Scribe_Defs.Look(ref ownerCultivation, "ownerCultivation");
            Scribe_Values.Look(ref ownerName,"ownerName",null);
        }
        public override void PostPostMake()
        {
            base.PostPostMake();
            if(ownerDef == null)
            {
                ownerDef = DefDatabase<ThingDef>.AllDefs.Where(PawnPredicate).RandomElement();
                ownerName = ownerDef.LabelCap;
                ownerCultivation = Cultivation_Utility.realmListAll.RandomElement().Key;
            }
        }
        public bool PawnPredicate(ThingDef def)
        {
            if(def.thingClass != typeof(Pawn))
            {
                return false;
            }
            if(def == ThingDefOf.Human)
            {
                return false;
            }
            if(def.race.thinkTreeMain == CTR_DefOf.Humanlike)
            {
                return false;
            }
            if(!def.race.IsFlesh)
            {
                return false;
            }
            return true;
        }
        public override bool AllowStackWith(Thing other)
        {
            return false;
        }
        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + "(" + ownerCultivation.label + ")";
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (ownerCultivation != null)
            {
                stringBuilder.AppendLine("beast: " + ownerDef.label + "(" + ownerName + ")");
                stringBuilder.AppendLine("beast cultivation: " + ownerCultivation.LabelCap);
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
