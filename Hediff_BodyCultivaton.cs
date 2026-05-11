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
    public class Hediff_BodyCultivaton : Hediff_CultivationBase
    {
        public bool canGetTrib = false;
        public HediffComp_BodyCultivation comp_BodyCultivation
        {
            get
            {
                return this.TryGetComp<HediffComp_BodyCultivation>();
            }
        }
        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }
        protected override void OnStageIndexChanged(int stageIndex)
        {
            base.OnStageIndexChanged(stageIndex);
            if (pawn.Faction == Faction.OfPlayerSilentFail)
            {
                Messages.Message($"{pawn.LabelShort} has reached stage {CurStage.label}", MessageTypeDefOf.NeutralEvent, false);
                if (stageIndex == def.stages.Count - 1 && nextRealm != cultivationDef)
                {
                    string label = $"{pawn.LabelShort} ready for breakthrough";
                    string desc = $"{pawn.LabelShort} has reach the peak for current realm and is ready for breakthrough.";
                    if (comp_BodyCultivation.Props.canGetTrib)
                    {
                        desc += $"\nif breakthrough now, pawn have {pawn.GetStatValue(CTR_DefOf.TribulationChance).ToStringPercent("0")} chance of tribulation.";
                    }
                    Find.LetterStack.ReceiveLetter(label, desc, LetterDefOf.PositiveEvent, pawn);
                }
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            for (int i = 0; i < comps.Count; i++)
            {
                IEnumerable<Gizmo> enumerable = comps[i].CompGetGizmos();
                if (enumerable == null)
                {
                    continue;
                }

                foreach (Gizmo item in enumerable)
                {
                    yield return item;
                }
            }
        }
        public override void Cultivation_Advance(bool canGetTrib, CultivationHediffDef curHediff, CultivationHediffDef nextHediff, int duration, int strikeInterval = 250)
        {
            if (!pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughCounter))
            {
                Hediff hediff2 = Cultivation_Utility.CreateHediffNoDuration(pawn, CTR_DefOf.CTR_BreakthroughCounter);
                pawn.health.AddHediff(hediff2);
            }
            if (!pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
            {
                Hediff hediff2 = Cultivation_Utility.CreateHediff(pawn, CTR_DefOf.CTR_BreakthroughProcess, duration);
                hediff2.TryGetComp<HediffComp_BreakthroughProcess>().curLevel = curHediff;
                hediff2.TryGetComp<HediffComp_BreakthroughProcess>().nextLevel = nextHediff;
                pawn.health.AddHediff(hediff2);
            }
            if (canGetTrib)
            {
                float tribChance = pawn.GetStatValue(CTR_DefOf.TribulationChance);
                if (Rand.Chance(tribChance) || nextHediff.guaranteedTribulation)
                {
                    TribulationInfo tinfo = new TribulationInfo();
                    tinfo = Cultivation_Utility.RandomizeTribulation();
                    Hediff hediff = Cultivation_Utility.CreateHediff(pawn, CTR_DefOf.CTR_Tribulation, duration);
                    hediff.Severity = tinfo.TribulationType;
                    hediff.TryGetComp<HediffComp_Tribulation>().OriginalDuration = duration;
                    hediff.TryGetComp<HediffComp_Tribulation>().Duration = duration;
                    hediff.TryGetComp<HediffComp_Tribulation>().StrikeInterval = strikeInterval;
                    hediff.TryGetComp<HediffComp_Tribulation>().Severity = tinfo.TribulationType;
                    hediff.TryGetComp<HediffComp_Tribulation>().curLevel = curHediff;
                    hediff.TryGetComp<HediffComp_Tribulation>().nextlevel = nextHediff;
                    pawn.health.AddHediff(hediff);
                    string text = "Heavenly Tribulation!";
                    string text2 = pawn.LabelShort + " " + "has attract the jealousy of heaven, and attract a " + hediff.CurStage.label + " Stage.";
                    Find.LetterStack.ReceiveLetter(text, text2, LetterDefOf.NegativeEvent, pawn);
                }
                else
                {
                    pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().curLevel = curHediff;
                    pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().nextLevel = nextHediff;
                }
            }
            else
            {
                pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().curLevel = curHediff;
                pawn.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_BreakthroughProcess>().nextLevel = nextHediff;
            }
        }
    }
}
