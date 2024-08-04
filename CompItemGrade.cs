using RimWorld;
using Verse;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class CompItemGrade : ThingComp
    {
        private ItemGrade gradeInt = ItemGrade.Mortal;

        public ItemGrade Grade => gradeInt;

        public void SetGrade(ItemGrade g)
        {
            gradeInt = g;
        }

        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + "GradeLabel".Translate(Grade.GetLabel());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref gradeInt, "grade", ItemGrade.Mortal);
        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
        {
            SetGrade(Cultivation_Utility.GenerateGradeTraderItem());
        }
        public override bool AllowStackWith(Thing other)
        {
            if (Cultivation_Utility.TryGetGrade(other,out var qc))
            {
                return gradeInt == qc;
            }
            return false;
        }
        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            piece.TryGetComp<CompItemGrade>().gradeInt = gradeInt;
        }

        public override string CompInspectStringExtra()
        {
            return "GradeInt".Translate(Grade.GetLabel().CapitalizeFirst());
        }
    }
}
