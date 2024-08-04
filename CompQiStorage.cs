using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse.Sound;

namespace CultivatorOfTheRim
{
    public class CompQiStorage : ThingComp
    {

        public CompProperties_QiStorage Props => (CompProperties_QiStorage)props;

        public float maxQiStored;

        public float curStored = 0;

        public override void PostPostMake()
        {
            base.PostPostMake();
            maxQiStored = Props.maxStorage;
        }
        public void IncreaseStored(float num)
        {
            curStored += num;
        }
        public void DistributeQi(float num)
        {
            curStored -= num * Props.usageMultiplier;
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "Absorb Qi",
                defaultDesc = "Absorb Qi from spiritual item",
                icon = Widgets.GetIconFor(parent.def),
                action = delegate
                {                    
                    Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate (LocalTargetInfo t)
                    {
                        ThingDef thingDef = t.Thing.def;
                        if (IsValid(thingDef))
                        {
                            float num = Cultivation_Utility.getItemQiValueForPawn(Cultivation_Utility.getItemQiTier(t.Thing));
                            num *= t.Thing.stackCount;
                            IncreaseStored(num);
                            Effecter effecter = ModsConfig.RoyaltyActive ? EffecterDefOf.Skip_Entry.Spawn(t.Cell, parent.Map) : EffecterDefOf.ExtinguisherExplosion.Spawn(t.Cell, parent.Map);
                            effecter.Cleanup();
                            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.Map));
                            t.Thing?.Destroy();
                        }
                        else
                        {
                            Messages.Message("target is not spiritual item", MessageTypeDefOf.NeutralEvent);
                        }
                    });
                }
            };
        }
        private bool IsValid(ThingDef thingDef)
        {
            if (thingDef.IsCorpse)
            {
                return false;
            }
            if (thingDef.IsEgg)
            {
                return false;
            }
            if (thingDef.isUnfinishedThing)
            {
                return false;
            }
            if (thingDef.tradeTags != null && !thingDef.tradeTags.Contains("Qi_Source"))
            {
                return false;
            }
            return true;
        }
        public TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetBuildings = false,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is not Pawn
            };
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref maxQiStored, "maxQiStored",100);
            Scribe_Values.Look(ref curStored, "curStored", 0);
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendInNewLine("Stored Qi: " + curStored.ToString("0.00") + "/" + maxQiStored.ToString("0.00"));

            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
