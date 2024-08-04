using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultivatorOfTheRim
{
    public class JobDriver_Breakthrough : JobDriver
    {
		private Pawn User => job.targetA.Pawn;

		private Mote warmupMote;

		private int duration => User.health.hediffSet.GetFirstHediffOfDef(CTR_DefOf.CTR_BreakthroughProcess).TryGetComp<HediffComp_Disappears>().ticksToDisappear;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(User, job, 1, -1, null, errorOnFailed);
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{			
			Toil toil = Toils_General.Wait(duration);
			toil.WithProgressBarToilDelay(TargetIndex.A);
            toil.FailOnDespawnedOrNull(TargetIndex.A);
			toil.AddEndCondition(() => toil.GetActor().health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) ? JobCondition.Ongoing : JobCondition.Incompletable);
			toil.tickAction = delegate
			{
				warmupMote = MoteMaker.MakeAttachedOverlay(toil.GetActor(), CTR_DefOf.Mote_ResurrectAbility, Vector3.zero);
				warmupMote?.Maintain();
			};				
			yield return toil;
			yield return Toils_General.Do(Breaking);
		}
		private void Breaking()
        {

            pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrikeTribulation(pawn.Map, pawn.Position,0,3));
		}
	}
}
