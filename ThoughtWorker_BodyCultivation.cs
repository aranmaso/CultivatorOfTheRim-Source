using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class ThoughtWorker_BodyCultivation : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.HaveBodyCultivation())
            {
                return ThoughtState.Inactive;
                
            }

            if (CultivatorOfTheRimMod.settings.isBodyCulCanUseBionic)
            {
                return ThoughtState.Inactive;
            }
            int num = GeneUtility.AddedAndImplantedPartsWithXenogenesCount(p);
            if (num > 0)
            {
                return ThoughtState.ActiveAtStage(num - 1);
            }
            return false;
        }

        public override string PostProcessDescription(Pawn p, string description)
        {
            string text = base.PostProcessDescription(p, description);
            Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(def.hediff);
            if (firstHediffOfDef == null || !firstHediffOfDef.Visible)
            {
                return text;
            }
            return text + "\n\n" + "CausedBy".Translate() + ": " + firstHediffOfDef.LabelBase.CapitalizeFirst();
        }
    }
}
