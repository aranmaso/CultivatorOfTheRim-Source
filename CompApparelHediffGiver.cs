using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace CultivatorOfTheRim
{
    public class CompApparelHediffGiver : ThingComp
    {
        public CompProperties_ApparelHediffGiver Props => (CompProperties_ApparelHediffGiver)props;

        Pawn wearer = null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref wearer,"wearer",false);
        }
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            wearer = pawn;
            foreach (var item in Props.hediffDefs)
            {
                if(!pawn.health.hediffSet.HasHediff(item))
                {
                    Hediff hediff = Cultivation_Utility.CreateHediffNoDuration(pawn, item);
                    hediff.Severity = Props.severity;
                    pawn.health.AddHediff(hediff);
                }
                else
                {
                    Hediff rh = pawn.health.hediffSet.GetFirstHediffOfDef(item);
                    pawn.health.RemoveHediff(rh);

                    Hediff hediff = Cultivation_Utility.CreateHediffNoDuration(pawn, item);
                    hediff.Severity = Props.severity;
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            wearer = null;
            if(Props.removeOnUnequip)
            {
                foreach (var item in Props.hediffDefs)
                {
                    if (pawn.health.hediffSet.HasHediff(item))
                    {
                        Hediff rh = pawn.health.hediffSet.GetFirstHediffOfDef(item);
                        pawn.health.RemoveHediff(rh);
                    }
                }
            }
            
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);            
            if (Props.removeOnUnequip && wearer != null)
            {
                foreach (var item in Props.hediffDefs)
                {
                    if (wearer.health.hediffSet.HasHediff(item))
                    {
                        Hediff rh = wearer.health.hediffSet.GetFirstHediffOfDef(item);
                        wearer.health.RemoveHediff(rh);
                    }
                }
            }
            wearer = null;
        }
    }
}
