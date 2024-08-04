using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class StatPart_BeastCore : StatPart
    {
        public SimpleCurve curveValue = new SimpleCurve()
        {
            new CurvePoint(1,1.01f),
            new CurvePoint(3,1.03f),
            new CurvePoint(4,1.1f),
            new CurvePoint(7,1.5f),
            new CurvePoint(10,2.0f),
            new CurvePoint(12,3.0f),
            new CurvePoint(17,10.0f),
            new CurvePoint(19,20.00f),
        };
        public override void TransformValue(StatRequest req, ref float val)
        {
            if(req.HasThing && req.Thing != null)
            {
                CompBeastCore comp = req.Thing.TryGetComp<CompBeastCore>();
                if(comp != null)
                {
                    HediffDef hediff = null;
                    if (comp.ownerCultivation != null)
                    {
                        hediff = comp.ownerCultivation;
                    }
                    else
                    {
                        hediff = CTR_DefOf.CTR_BodyTempering;
                    }
                    int num = Cultivation_Utility.realmListAll[hediff];
                    float num2 = curveValue.Evaluate(num);
                    val *= num2;
                }
            }
        }


        public override string ExplanationPart(StatRequest req)
        {
            CompBeastCore comp = req.Thing.TryGetComp<CompBeastCore>();
            if (comp != null)
            {
                HediffDef hediff = null;
                if (comp.ownerCultivation != null)
                {
                    hediff = comp.ownerCultivation;
                }
                else
                {
                    hediff = CTR_DefOf.CTR_BodyTempering;
                }

                return "Beast Cultivation: " + hediff.LabelCap;
            }
            return null;
        }
    }
}
