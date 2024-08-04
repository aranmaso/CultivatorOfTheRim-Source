using RimWorld;
using System;
using Verse;

namespace CultivatorOfTheRim
{
    public class StatPart_GradeWorth : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {            
            CompItemGrade grade = req.Thing?.TryGetComp<CompItemGrade>();
            if (req.HasThing && grade != null)
            {
                float num = req.Thing.def.BaseMarketValue;
                switch (grade.Grade)
                {
                    case ItemGrade.Mortal:
                        break;
                    case ItemGrade.Ordinary:
                        val += num * 1.1f;
                        break;
                    case ItemGrade.Earth:
                        val += num * 2.5f;
                        break;
                    case ItemGrade.Heaven:
                        val += num * 3.5f;
                        break;
                    case ItemGrade.Mysterious:
                        val += num * 5.5f;
                        break;
                    case ItemGrade.Divine:
                        val += num * 7.5f;
                        break;
                    case ItemGrade.Emperor:
                        val += num * 9.5f;
                        break;
                    case ItemGrade.Dao:
                        val += num * 10f;
                        break;
                }
            }
            CompPillGrade pillGrade = req.Thing?.TryGetComp<CompPillGrade>();
            if(req.HasThing && pillGrade != null)
            {
                switch (pillGrade.Grade)
                {
                    case PillGrade.Spirit:
                        val += (req.Thing.def.BaseMarketValue * 0.1f);
                        break;
                    case PillGrade.Earth:
                        val += (req.Thing.def.BaseMarketValue * 0.25f);
                        break;
                    case PillGrade.Heaven:
                        val += (req.Thing.def.BaseMarketValue * 0.25f);
                        break;
                    case PillGrade.Mysterious:
                        val += (req.Thing.def.BaseMarketValue * 0.5f);
                        break;
                    case PillGrade.Divine:
                        val += (req.Thing.def.BaseMarketValue * 0.75f);
                        break;
                    case PillGrade.Emperor:
                        val += req.Thing.def.BaseMarketValue;
                        break;
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            CompItemGrade grade = req.Thing?.TryGetComp<CompItemGrade>();
            if (req.HasThing && grade != null)
            {
                switch (grade.Grade)
                {
                    case ItemGrade.Mortal:
                        return null;
                    case ItemGrade.Ordinary:
                        return "Item Grade: " + req.Thing.def.BaseMarketValue * 1.1f;
                    case ItemGrade.Earth:
                        return "Item Grade: " + req.Thing.def.BaseMarketValue * 2.5f;
                    case ItemGrade.Heaven:
                        return "Item Grade: " + req.Thing.def.BaseMarketValue * 3.5f;
                    case ItemGrade.Mysterious:
                        return "Item Grade: " + req.Thing.def.BaseMarketValue * 5.5f;
                    case ItemGrade.Divine:
                        return "Item Grade: " + req.Thing.def.BaseMarketValue * 7.5f;
                    case ItemGrade.Emperor:
                        return "Item Grade: " + req.Thing.def.BaseMarketValue * 9.5f;
                    case ItemGrade.Dao:
                        return "Item Grade: " + req.Thing.def.BaseMarketValue * 10f;
                }                
            }
            CompPillGrade pillGrade = req.Thing?.TryGetComp<CompPillGrade>();
            if (req.HasThing && pillGrade != null)
            {
                return pillGrade.Grade switch
                {
                    PillGrade.Spirit => "Pill Grade: " + req.Thing.def.BaseMarketValue * 0.1f,
                    PillGrade.Earth => "Pill Grade: " + req.Thing.def.BaseMarketValue * 0.25f,
                    PillGrade.Heaven => "Pill Grade: " + req.Thing.def.BaseMarketValue * 0.25f,
                    PillGrade.Mysterious => "Pill Grade: " + req.Thing.def.BaseMarketValue * 0.5f,
                    PillGrade.Divine => "Pill Grade: " + req.Thing.def.BaseMarketValue * 0.75f,
                    PillGrade.Emperor => "Pill Grade: " + req.Thing.def.BaseMarketValue * 1f,
                    _ => throw new ArgumentException(),
                };
            }
            return null;
        }
    }
}
