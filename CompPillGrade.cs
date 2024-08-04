using RimWorld;
using Verse;
using System.Collections.Generic;

namespace CultivatorOfTheRim
{
    public class CompPillGrade : ThingComp
    {
        private PillGrade gradeInt = PillGrade.Spirit;

        public PillGrade Grade => gradeInt;

        public void SetGrade(PillGrade g)
        {
            gradeInt = g;
        }
        public override string TransformLabel(string label)
        {
            return base.TransformLabel(label) + "PillLabel".Translate(Grade.PillGetLabel());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref gradeInt, "grade", PillGrade.Spirit);
        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
        {
            SetGrade(Cultivation_Utility.GeneratePillGradeTraderItem());
        }
        public override bool AllowStackWith(Thing other)
        {
            if (Cultivation_Utility.TryGetPillGrade(other, out var qc))
            {
                return gradeInt == qc;
            }
            return false;
        }
        public override void PostSplitOff(Thing piece)
        {
            base.PostSplitOff(piece);
            piece.TryGetComp<CompPillGrade>().gradeInt = gradeInt;
        }
        public override string CompInspectStringExtra()
        {
            return "PillInt".Translate(Grade.PillGetLabel().CapitalizeFirst());
        }
    }
}
