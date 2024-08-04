using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace CultivatorOfTheRim
{
    public class CompUseEffect_EscapeTheDomain : CompUseEffect
    {
        public CompProperties_EscapeTheDomain Props => (CompProperties_EscapeTheDomain)props;
        public override void DoEffect(Pawn user)
        {
            /*Hediff outsideTheDomain = Cultivation_Utility.CreateHediffNoDuration(user,CTR_DefOf.CTR_OutsidetheDomain);
            outsideTheDomain.Severity = 0.001f;*/
            Hediff Creation = user.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_Creation_Realm);
            if(Creation != null )
            {
                ((Hediff_CultivationLevel)Creation).Cultivation_Advance(true, true, CTR_DefOf.CTR_Creation_Realm, CTR_DefOf.CTR_OutsidetheDomain, 10000, 250);
            }
            parent.stackCount = 1;
            /*Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, user);
            job.count = 1;
            user.jobs.TryTakeOrderedJob(job, JobTag.Misc);*/
            //parent.Destroy();
            //user.health.RemoveHediff(Creation);
            //user.health.AddHediff(outsideTheDomain);
            //user.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(user.Map, user.Position, 0, 3));
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            string failReason = null;
            if(parent.stackCount < parent.def.stackLimit)
            {
                failReason = "not enough flame source, require " + parent.def.stackLimit;
                Messages.Message(failReason,MessageTypeDefOf.NeutralEvent);
                return false;
            }
            if (!Cultivation_Utility.HaveCultivation(p))
            {
                failReason = "pawn is not a cultivator";
                Messages.Message(failReason, MessageTypeDefOf.NeutralEvent);
                return false;
            }
            if(Cultivation_Utility.HaveCultivation(p) && Cultivation_Utility.FindCultivationLevel(p).def != CTR_DefOf.CTR_Creation_Realm && Cultivation_Utility.FindCultivationLevel(p).def != CTR_DefOf.CTR_OutsidetheDomain)
            {
                failReason = "pawn has not reach the peak of their domain";
                Messages.Message(failReason, MessageTypeDefOf.NeutralEvent);
                return false;
            }
            Hediff creation = p.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_Creation_Realm);
            if(creation != null && creation.Severity != creation.def.maxSeverity)
            {
                failReason = "pawn has not reach the peak of creation realm";
                Messages.Message(failReason, MessageTypeDefOf.NeutralEvent);
                return false;
            }
            Hediff otd = p.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_OutsidetheDomain);
            if(otd != null)
            {
                failReason = "pawn has already step outside the domain";
                Messages.Message(failReason, MessageTypeDefOf.NeutralEvent);
                return false;
            }
            failReason = null;
            return true;
        }
    }
}
