using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class StatPart_GradeDamage : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing)
            {
                CompItemGrade grade = req.Thing?.TryGetComp<CompItemGrade>();
                if (grade != null)
                {
                    switch (grade.Grade)
                    {
                        case ItemGrade.Mortal:
                            break;
                        case ItemGrade.Ordinary:
                            val *= 1.1f;
                            break;
                        case ItemGrade.Earth:
                            val *= 1.25f;
                            break;
                        case ItemGrade.Heaven:
                            val *= 1.5f;
                            break;
                        case ItemGrade.Mysterious:
                            val *= 1.75f;
                            break;
                        case ItemGrade.Divine:
                            val *= 2.0f;
                            break;
                        case ItemGrade.Emperor:
                            val *= 2.25f;
                            break;
                        case ItemGrade.Dao:
                            val *= 2.5f;
                            break;
                    }
                }
            }
        }
        public override string ExplanationPart(StatRequest req)
        {
            if(req.HasThing)
            {
                CompItemGrade grade = req.Thing?.TryGetComp<CompItemGrade>();
                if(grade != null)
                {
                    switch (grade.Grade)
                    {
                        case ItemGrade.Mortal:
                            return null;
                        case ItemGrade.Ordinary:
                            return "Item Grade: x1.1";
                        case ItemGrade.Earth:
                            return "Item Grade: x1.25";
                        case ItemGrade.Heaven:
                            return "Item Grade: x1.5";
                        case ItemGrade.Mysterious:
                            return "Item Grade: x1.75";
                        case ItemGrade.Divine:
                            return "Item Grade: x2.0";
                        case ItemGrade.Emperor:
                            return "Item Grade: x2.25";
                        case ItemGrade.Dao:
                            return "Item Grade: x2.5";
                    }
                }                
            }
            return null;
        }
    }            
}
