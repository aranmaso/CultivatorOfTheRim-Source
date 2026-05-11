using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultivatorOfTheRim
{
    public class HediffComp_CultivatorResurrect : HediffComp
    {
        public HediffCompProperties_CultivatorResurrect Props => (HediffCompProperties_CultivatorResurrect)props;

        public int resurrectionsLeft = 1;
        public override string CompLabelInBracketsExtra
        {
            get
            {
                return base.CompLabelInBracketsExtra + resurrectionsLeft + " lives";
            }
        }
        public override void CompExposeData()
        {
            Scribe_Values.Look(ref resurrectionsLeft, "resurrectionsLeft", 1);
        }
        public override void CompPostMake()
        {
            base.CompPostMake();
            resurrectionsLeft = Props.livesLeft;
        }
        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (!Props.replenishLivesOvertime) return;
            if (Pawn.IsHashIntervalTick(Props.timeToReplenish))
            {
                if (resurrectionsLeft < Props.livesLeft)
                {
                    if (Props.replenishToFull)
                    {
                        resurrectionsLeft = Props.livesLeft;
                        Messages.Message($"Live has been replenish for {Pawn.LabelShort}", MessageTypeDefOf.PositiveEvent, false);
                    }
                    else
                    {
                        resurrectionsLeft++;
                        Messages.Message($"Live has been replenish for {Pawn.LabelShort}", MessageTypeDefOf.PositiveEvent, false);
                    }
                }
            }
        }
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (!Pawn.Dead) return;
            if(resurrectionsLeft <= 0) return;
            resurrectionsLeft--;
            Map map = Pawn.Corpse.MapHeld;
            IntVec3 deadPosition = Pawn.Corpse.PositionHeld;
            while (Pawn.Dead)
            {
                if (!deadPosition.Walkable(map) || deadPosition.Impassable(map))
                {
                    Pawn.Corpse.Position = map.AllCells.Where(x => x.Walkable(map) && !x.Fogged(map)).RandomElement();
                }
                ResurrectionUtility.TryResurrect(Pawn);
                MoteMaker.ThrowText(Pawn.PositionHeld.ToVector3(), map, "Revived");
            }
            if (Pawn.Spawned)
            {
                SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(Pawn.PositionHeld, Pawn.MapHeld));
                FleckMaker.AttachedOverlay(parent.pawn, DefDatabase<FleckDef>.GetNamed("PsycastPsychicEffect"), Vector3.zero);
            }
        }
    }
}
