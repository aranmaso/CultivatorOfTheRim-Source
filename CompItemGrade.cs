using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;
using RimWorld.Planet;

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

        public override string GetDescriptionPart()
        {
            float val = 1.00f;
            if(parent is Apparel)
            {
                if (CultivatorOfTheRimMod.settings.isArmorGradeStackMultiplicatively)
                {
                    switch (Grade)
                    {
                        case ItemGrade.Mortal:
                            break;
                        case ItemGrade.Ordinary:
                            val = 0.95f;
                            break;
                        case ItemGrade.Earth:
                            val = 0.85f;
                            break;
                        case ItemGrade.Heaven:
                            val = 0.825f;
                            break;
                        case ItemGrade.Mysterious:
                            val = 0.80f;
                            break;
                        case ItemGrade.Divine:
                            val = 0.75f;
                            break;
                        case ItemGrade.Emperor:
                            val = 0.725f;
                            break;
                        case ItemGrade.Dao:
                            val = 0.5f;
                            break;
                    }
                }
                else
                {
                    switch (Grade)
                    {
                        case ItemGrade.Mortal:
                            break;
                        case ItemGrade.Ordinary:
                            val = 0.8f;
                            break;
                        case ItemGrade.Earth:
                            val = 0.5f;
                            break;
                        case ItemGrade.Heaven:
                            val = 0.5f;
                            break;
                        case ItemGrade.Mysterious:
                            val = 0.5f;
                            break;
                        case ItemGrade.Divine:
                            val = 0.25f;
                            break;
                        case ItemGrade.Emperor:
                            val = 0.25f;
                            break;
                        case ItemGrade.Dao:
                            val = 0.1f;
                            break;
                    }
                }                
            }
            else
            {
                switch (Grade)
                {
                    case ItemGrade.Mortal:
                        break;
                    case ItemGrade.Ordinary:
                        val = 1.1f;
                        break;
                    case ItemGrade.Earth:
                        val = 1.25f;
                        break;
                    case ItemGrade.Heaven:
                        val = 1.5f;
                        break;
                    case ItemGrade.Mysterious:
                        val = 1.75f;
                        break;
                    case ItemGrade.Divine:
                        val = 2.00f;
                        break;
                    case ItemGrade.Emperor:
                        val = 2.25f;
                        break;
                    case ItemGrade.Dao:
                        val = 2.5f;
                        break;
                }
            }
            string text = "\n";
            if (parent is Apparel)
            {
                text += "Damage Reduction: x" + val;
            }      
            else
            {
                text += "Damage: x" + val;
            }
            return base.GetDescriptionPart() + text.Colorize(Color.green);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref gradeInt, "grade", ItemGrade.Mortal);
        }
        public override void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
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
