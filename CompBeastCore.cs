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

        public CultivationHediffDef ownerCultivation;

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
                if (parent.def == CTR_DefOf.CTR_BeastCore)
                {
                    ownerDef = DefDatabase<ThingDef>.AllDefs.Where(PawnPredicate).RandomElement();
                    ownerName = ownerDef.LabelCap;
                    ownerCultivation = StaticCollectionCached.CultivationHediffDefs.Where(x => !x.IsBodyCultivation() && x.realmPower <= 17).RandomElement();
                }
                else if (parent.def == CTR_DefOf.CTR_GoldenCore_Core)
                {
                    ownerDef = DefDatabase<ThingDef>.AllDefs.Where(PawnPredicate).RandomElement();
                    ownerName = ownerDef.LabelCap;
                    ownerCultivation = StaticCollectionCached.CultivationHediffDefs.Where(x => !x.IsBodyCultivation() && x.realmPower >= 7 && x.realmPower < 14).RandomElement();
                }
                else if (parent.def == CTR_DefOf.CTR_ImmortalCore)
                {
                    ownerDef = DefDatabase<ThingDef>.AllDefs.Where(PawnPredicate).RandomElement();
                    ownerName = ownerDef.LabelCap;
                    ownerCultivation = StaticCollectionCached.CultivationHediffDefs.Where(x => !x.IsBodyCultivation() && x.realmPower >= 14 && x.realmPower <= 17).RandomElement();
                }
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
                stringBuilder.AppendLine("owner: " + ownerDef.label + "(" + ownerName + ")");
                stringBuilder.AppendLine("owner cultivation: " + ownerCultivation.LabelCap);
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
