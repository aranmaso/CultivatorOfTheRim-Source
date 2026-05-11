using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CultivatorOfTheRim
{
    public class HediffComp_BodyCultivation : HediffComp
    {
        public HediffCompProperties_BodyCultivation Props => (HediffCompProperties_BodyCultivation)props;

        public Hediff_CultivationBase cultivationBase => parent as Hediff_CultivationBase;
        public CultivationHediffDef CulDef => cultivationBase.cultivationDef;

        private List<NeedListInfoInner> NeedListCached = new List<NeedListInfoInner>();
        private IEnumerable<NeedListInfoInner> NeedsGet
        {
            get
            {
                if (NeedListCached.NullOrEmpty() && !changeToNeedsEmptyCached)
                {
                    foreach (var item in Props.changeToNeeds)
                    {
                        Need newNeed = Pawn.needs.TryGetNeed(item.needDef);
                        NeedListInfoInner info = new NeedListInfoInner(newNeed, item.minValue);
                        NeedListCached.Add(info);
                    }
                    ;
                }
                return NeedListCached;
            }
        }

        private bool changeToNeedsEmptyCached = false;

        private int interval;

        public bool requireQiSource;

        public float HealthScaleMul = 1f;

        public float CultivationSpeed = 1f;
        private bool isFixedAge
        {
            get
            {
                if (CultivatorOfTheRimMod.settings.isDeAgingPawn)
                {
                    return CulDef.realmPower >= 7 || Props.isFixedAge;
                }
                return false;
            }
        }
        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (parent.Severity >= 0)
                {
                    return base.CompLabelInBracketsExtra + "(" + parent.Severity.ToString("0.000") + "/" + Def.maxSeverity + ")";
                }
                return base.CompLabelInBracketsExtra;
            }
        }
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ApplyBonusHediff();
            if (ModsConfig.BiotechActive)
            {
                ApplyBonusGene();
            }
        }
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            StaticCollectionCached.PawnHealthScaleCached.Remove(Pawn);
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref changeToNeedsEmptyCached, "changeToNeedsEmptyCached",true);
            Scribe_Values.Look(ref interval, "interval", 60);
            Scribe_Values.Look(ref HealthScaleMul, "HealthScaleMul", 1f);
            Scribe_Values.Look(ref CultivationSpeed, "CultivationSpeed", 1f);
        }
        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (Pawn.IsHashIntervalTick(interval, delta))
            {
                CultivationSpeed = Pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
            }
            if (Pawn.IsHashIntervalTick(60000, delta))
            {
                if (isFixedAge)
                {
                    if (Pawn.ageTracker.AgeBiologicalYears > 21)
                    {
                        Pawn.ageTracker.AgeBiologicalTicks = 75600000;
                        parent.pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment);
                    }
                }
            }
            if (parent.pawn.IsHashIntervalTick(2500, delta) && CultivatorOfTheRimMod.settings.isNeedCapped)
            {
                if (NeedsGet != null && !changeToNeedsEmptyCached)
                {
                    foreach (var item in NeedsGet)
                    {
                        if (item.need == null) continue;
                        if (item.need.CurLevel < item.minValue)
                        {
                            item.need.CurLevelPercentage = item.minValue;
                        }
                    }
                }
            }
            if (Pawn.IsHashIntervalTick(2500))
            {
                ClearAllBionic();
                if (Pawn.equipment?.Primary != null && Pawn.Spawned)
                {
                    TryDropWeapon();
                    
                }
            }
        }
        public void TryDropWeapon()
        {
            if(CultivatorOfTheRimMod.settings.isBodyCulCanUseWeapon) return;
            if (Pawn.Faction == Faction.OfPlayerSilentFail)
            {
                var primary = Pawn.equipment?.Primary;
                Pawn.equipment?.TryDropEquipment(primary,out var wep,Pawn.PositionHeld);
                Messages.Message($"{Pawn.LabelShort} refuse to wield weapon, and dropped {primary?.LabelShort}",Pawn,MessageTypeDefOf.NegativeEvent,false);
            }
            else
            {
                var primary = Pawn.equipment?.Primary;
                Pawn.equipment?.TryTransferEquipmentToContainer(primary, Pawn.inventory.innerContainer);
                Messages.Message($"{Pawn.LabelShort} refuse to wield weapon, and put away {primary?.LabelShort}",Pawn,MessageTypeDefOf.NegativeEvent,false);
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            Command_Action breakthrough = new Command_Action();
            breakthrough.defaultLabel = CultivatorOfTheRimMod.settings.isBreakthroughCanFailForHumanlike ? "Attempt to Breakthrough: " + 1f.ToStringPercent("0.00") : "Attempt to Breakthrough";
            if (Props.canGetTrib)
            {
                breakthrough.defaultDesc = "Attempt to Breakthrough" + "\n" + "Tribulation Chance: " + Pawn.GetStatValue(CTR_DefOf.TribulationChance, cacheStaleAfterTicks: 250).ToStringPercent();
            }
            else
            {
                breakthrough.defaultDesc = "Attempt to Breakthrough";
            }
            breakthrough.icon = Props._uiIcon;
            breakthrough.action = delegate
            {
                CultivationHediffDef nextLevel = (CultivationHediffDef)(Props.nextLevel != null ? Props.nextLevel : Props.nextLevels.RandomElementByWeight(x => x.weight).nextRealm);
                if (nextLevel == null || nextLevel == Def)
                {
                    //Messages.Message("AlreadyLearned".Translate(reader.LabelShort, Props.benefitString, reader.Named("USER")), reader, MessageTypeDefOf.PositiveEvent);
                    Messages.Message("CTR_BodyCultivationBreakthrough_Maxed".Translate(Pawn.LabelShort,parent.Label),MessageTypeDefOf.NeutralEvent,false);
                }
                else if (parent.Severity < Def.maxSeverity)
                {
                    Messages.Message("CTR_BodyCultivationBreakthrough_RealmNotMaxed".Translate(Pawn.LabelShort), MessageTypeDefOf.NeutralEvent, false);
                }
                else
                {
                    if (parent.Severity >= Def.maxSeverity && !Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (nextLevel != null && Def != nextLevel))
                    {
                        ((Hediff_BodyCultivaton)parent).Cultivation_Advance(Props.canGetTrib,CulDef,nextLevel,Props.breakingThroughDuration.RandomInRange,Props.TribulationStrikeInterval);
                        Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough,Pawn);
                        job.count = 1;
                        Pawn.jobs.TryTakeOrderedJob(job);
                    }
                    else if (Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                    {
                        Messages.Message("pawn in the process of breakingthrough", MessageTypeDefOf.NeutralEvent);
                    }
                }
            };
            yield return breakthrough;
            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: +1 stage",
                    defaultDesc = "advance to next stage",
                    action = delegate
                    {
                        float curStage = parent.CurStage.minSeverity;
                        for (int i = 0; i < parent.def.stages.Count; i++)
                        {
                            if (parent.def.stages[i].minSeverity > curStage)
                            {
                                parent.Severity = parent.def.stages[i].minSeverity;
                                if (i == parent.def.stages.Count)
                                {
                                    parent.Severity = parent.def.maxSeverity;
                                }
                                break;
                            }
                        }
                    }
                };
            }
        }
        
        public override void CompPostMake()
        {
            changeToNeedsEmptyCached = Props.changeToNeeds.NullOrEmpty();
            interval = Props.tickInterval;
            requireQiSource = Props.requireQiSource;
        }
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            if (Props.severityPerDamageTaken.min > 0)
            {
                float num = Props.severityPerDamageTaken.RandomInRange;
                num *= totalDamageDealt;
                num *= CultivationSpeed;
                parent.Severity += num;
            }
        }

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
        }
        public void ApplyBonusHediff()
        {
            if (!Props.bonusHediff.NullOrEmpty())
            {
                foreach (var item in Props.bonusHediff.OrderBy(x => x.chance))
                {
                    if (Pawn.health.hediffSet.HasHediff(item.hediffDef)) continue;
                    if (Rand.Chance(item.chance))
                    {
                        Hediff hediff = HediffMaker.MakeHediff(item.hediffDef, Pawn);
                        Pawn.health.AddHediff(hediff);
                        if (item.onlyOne)
                        {
                            break;
                        }
                    }
                }
            }
        }
        public void ApplyBonusGene()
        {
            if (Props.bonusGenes.NotNullOrEmpty())
            {
                if (Pawn.RaceProps.Humanlike)
                {
                    if (Pawn.genes != null)
                    {
                        foreach (var item in Props.bonusGenes)
                        {
                            if(CultivatorOfTheRimMod.settings.isBodyCulCanUseWeapon && item.defName == "BS_UnarmedOnly")  continue;
                            if (Pawn.genes.HasActiveGene(item)) continue;
                            Pawn.genes.AddGene(item, false);
                        }
                    }
                }
            }
        }
        public void ClearAllBionic()
        {
            if (CultivatorOfTheRimMod.settings.isBodyCulCanUseBionic) return;
            if (!Pawn.Spawned) return;
            IReadOnlyList<Hediff> hediffs = [.. Pawn.health.hediffSet.hediffs];
            foreach (var item in hediffs)
            {
                if (item is Hediff_AddedPart addedPart)
                {
                    if (Pawn.Spawned)
                    {
                        MedicalRecipesUtility.SpawnThingsFromHediffs(Pawn,addedPart.Part,Pawn.PositionHeld,Pawn.MapHeld);
                    }
                    Pawn.health.RestorePart(addedPart.Part);
                    addedPart?.Notify_SurgicallyReplaced(Pawn);
                    Messages.Message($"{item.Label} has been purged from {Pawn.LabelShort} body", MessageTypeDefOf.NeutralEvent, false);
                }
            }
        }
    }
}
