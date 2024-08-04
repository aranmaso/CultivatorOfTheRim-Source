using Verse;
using RimWorld;
using System.Collections.Generic;
using Verse.AI;
using UnityEngine;
using Verse.Noise;
using System;
using System.Net.NetworkInformation;
using System.Linq;
using HarmonyLib;

namespace CultivatorOfTheRim
{
    public class HediffComp_Cultivation_New : HediffComp
    {
        private HediffCompProperties_Cultivation Props => (HediffCompProperties_Cultivation)props;

        private List<NeedListInfoInner> NeedListCached = new List<NeedListInfoInner>();

        private bool changeToNeedsEmptyCached = false;

        private int interval;

        public bool requireQiSource;

        public bool isNearbyQi = false;

        public bool isInitialChecked = false;

        public int tickTillNextFleck = 5;

        public bool isSpawningFleck = false;

        public int amountToSpawn;

        public int amountSpawned;

        private bool consumeSource
        {
            get
            {
                if (requireQiSource)
                {
                    int value = Cultivation_Utility.realmListAll[Def];
                    if (value >= 7)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public float HealthScaleMul = 1f;
        public float CultivationSpeed = 1f;
        private int currentRealmTier
        {
            get
            {
                return Cultivation_Utility.realmListAll[Def];
            }
        }

        private List<Thing> QiSourceList = new List<Thing>();

        private List<Thing> QiCrystalList = new List<Thing>();

        private Dictionary<Thing, float> QiSourceWithType = new Dictionary<Thing, float>();

        private Dictionary<Thing, float> QiSourceWithValue = new Dictionary<Thing, float>();

        private Dictionary<Thing, string> QisourceTypeWithStringCached = new Dictionary<Thing, string>();

        private Dictionary<Vector3, Thing> itemToSpawnFleckList = new Dictionary<Vector3, Thing>();
        private IDictionary<Thing, string> QisourceTypeWithStringGet
        {
            get
            {
                return QisourceTypeWithStringCached;
            }
        }
        private float multiplierForQiType = 1f;

        private float totalSeverityChange = 0f;

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
                    };
                }
                return NeedListCached;
            }
        }
        public override void CompPostMake()
        {
            changeToNeedsEmptyCached = Props.changeToNeeds.NullOrEmpty();
            interval = Props.tickInterval;
            requireQiSource = Props.requireQiSource;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref changeToNeedsEmptyCached, "changeToNeedsEmptyCached", true);
            Scribe_Values.Look(ref interval, "interval", 60);
            Scribe_Values.Look(ref requireQiSource, "requireQiSource", false);
            Scribe_Values.Look(ref multiplierForQiType, "multiplierForQiType", 1f);
            Scribe_Values.Look(ref totalSeverityChange, "totalSeverityChange", 0.1f);
            Scribe_Values.Look(ref HealthScaleMul, "HealthScaleMul", 1f);
            Scribe_Values.Look(ref CultivationSpeed, "CultivationSpeed", 1f);
            if (Props.requireQiSource)
            {
                if (!QiSourceList.NullOrEmpty())
                {
                    Scribe_Collections.Look(ref QiSourceList, "QiSourceList", LookMode.Reference);
                }
                if (!QiSourceWithType.NullOrEmpty())
                {
                    Scribe_Collections.Look(ref QiSourceWithType, "QiSourceWithType", LookMode.Reference, LookMode.Value);
                }
                if (!QiSourceWithValue.NullOrEmpty())
                {
                    Scribe_Collections.Look(ref QiSourceWithValue, "QiSourceWithValue", LookMode.Reference, LookMode.Value);
                }
                if (!QisourceTypeWithStringCached.NullOrEmpty())
                {
                    Scribe_Collections.Look(ref QisourceTypeWithStringCached, "QisourceTypeWithStringCached", LookMode.Reference, LookMode.Value);
                }
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
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if ((Pawn.RaceProps.Animal || Pawn.RaceProps.IsAnomalyEntity) && CultivatorOfTheRimMod.settings.isAnimalDropBeastCore)
            {
                Map map = parent?.pawn?.Corpse?.Map;
                IntVec3 pos = parent.pawn.Corpse.Position;
                if (map != null)
                {
                    Thing specialDrop = ThingMaker.MakeThing(CTR_DefOf.CTR_BeastCore);
                    if (specialDrop.TryGetComp<CompBeastCore>() != null)
                    {
                        specialDrop.TryGetComp<CompBeastCore>().ownerName = Pawn.LabelShort;
                        specialDrop.TryGetComp<CompBeastCore>().ownerDef = Pawn.def;
                        specialDrop.TryGetComp<CompBeastCore>().ownerCultivation = Def;
                    }
                    GenSpawn.Spawn(specialDrop, pos, map);
                }
            }
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(250))
            {
                HealthScaleMul = Pawn.GetStatValue(CTR_DefOf.CTR_HealthMultiplier);        
                CultivationSpeed = Pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
            }
            if (Pawn.RaceProps.IsMechanoid)
            {
                return;
            }
            if (parent.pawn.IsHashIntervalTick(interval) && parent.Severity < parent.def.maxSeverity)
            {
                if (Pawn.RaceProps.Humanlike)
                {
                    if (Pawn.story?.traits?.GetTrait(CTR_DefOf.CTR_CultivationProdigy) != null || Pawn.story.AllBackstories.Contains(CTR_DefOf.CTR_ImmortalChild))
                    {
                        ProdigyCultivate();
                    }
                    if (Pawn.story?.GetBackstory(BackstorySlot.Childhood) == CTR_DefOf.CTR_ImmortalChild)
                    {
                        ProdigyCultivate();
                    }
                }
                if (parent.pawn.psychicEntropy.IsCurrentlyMeditating)
                {

                    IncreaseSeverity();
                    if (requireQiSource && CultivatorOfTheRimMod.settings.isCulSpeedAffectedByEnviaronment)
                    {
                        if (!isInitialChecked)
                        {
                            UpdateSourceList();
                        }
                    }
                }
                else
                {
                    if (requireQiSource && CultivatorOfTheRimMod.settings.isCulSpeedAffectedByEnviaronment)
                    {
                        if (isInitialChecked)
                        {
                            isInitialChecked = false;
                        }
                        if (!QiSourceList.NullOrEmpty())
                        {
                            QiSourceList.Clear();
                            QiSourceWithType.Clear();
                            QiSourceWithValue.Clear();
                            QisourceTypeWithStringCached.Clear();
                            itemToSpawnFleckList.Clear();
                            multiplierForQiType = 1f;
                            totalSeverityChange = 0f;
                        }
                    }
                    if (!CultivatorOfTheRimMod.settings.isCulSpeedAffectedByEnviaronment && !QiSourceList.NullOrEmpty())
                    {
                        QiSourceList.Clear();
                        QiSourceWithType.Clear();
                        QiSourceWithValue.Clear();
                        QisourceTypeWithStringCached.Clear();
                        itemToSpawnFleckList.Clear();
                        multiplierForQiType = 1f;
                        totalSeverityChange = 0f;
                    }
                }
            }
            if (CultivatorOfTheRimMod.settings.isWildAnimalAutoCultivate)
            {
                if (Pawn.RaceProps.Animal)
                {
                    if (parent.Severity < parent.def.maxSeverity)
                    {
                        if (parent.pawn.IsHashIntervalTick(interval))
                        {
                            AnimalCultivate();
                        }
                    }
                }
            }
            if (CultivatorOfTheRimMod.settings.isWildAnimalAutoBreakthrought && (Pawn.RaceProps.Animal || Pawn.RaceProps.IsAnomalyEntity))
            {
                if (Pawn.IsHashIntervalTick(interval))
                {
                    if (parent.Severity >= parent.def.maxSeverity)
                    {
                        if (!Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                        {
                            if (CultivatorOfTheRimMod.settings.isWildAnimalIgnoreSafetyThresholdForBreakthrough)
                            {
                                if (Pawn.GetStatValue(CTR_DefOf.TribulationChance, cacheStaleAfterTicks: 250) > CultivatorOfTheRimMod.settings.tribulationSafety)
                                {
                                    return;
                                }
                            }
                            AnimalBreakthrought();
                        }
                    }
                }
            }
            if (requireQiSource && parent.Severity < parent.def.maxSeverity && CultivatorOfTheRimMod.settings.isCulSpeedAffectedByEnviaronment)
            {
                if (parent.pawn.IsHashIntervalTick(2500))
                {
                    if (parent.pawn.psychicEntropy.IsCurrentlyMeditating && isNearbyQi)
                    {
                        UpdateSourceList();
                    }
                    else if (!parent.pawn.psychicEntropy.IsCurrentlyMeditating && !QiSourceList.NullOrEmpty())
                    {
                        QiSourceList.Clear();
                        QiSourceWithType.Clear();
                        QiSourceWithValue.Clear();
                        QisourceTypeWithStringCached.Clear();
                        itemToSpawnFleckList.Clear();
                        multiplierForQiType = 1f;
                        totalSeverityChange = 0f;
                        isInitialChecked = false;
                    }
                }
                if (Pawn.IsHashIntervalTick(250))
                {
                    if (Pawn.psychicEntropy.IsCurrentlyMeditating)
                    {
                        if (!QiSourceList.NullOrEmpty())
                        {
                            CheckForItemChange();
                            if (consumeSource)
                            {
                                ConsumeSource();
                            }
                        }
                    }
                }
            }

            if (parent.pawn.IsHashIntervalTick(2500) && CultivatorOfTheRimMod.settings.isNeedCapped)
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
            if (parent.Severity < parent.def.maxSeverity)
            {
                if (isSpawningFleck && Pawn.psychicEntropy.IsCurrentlyMeditating)
                {
                    if (Pawn.IsHashIntervalTick(tickTillNextFleck))
                    {
                        if (!QiSourceList.NullOrEmpty())
                        {
                            Cultivation_Utility.SpawnOrb(Pawn,itemToSpawnFleckList,QiSourceWithValue,QisourceTypeWithStringCached);
                            amountSpawned++;
                            itemToSpawnFleckList.Remove(itemToSpawnFleckList.First().Key);
                            if (amountSpawned >= amountToSpawn)
                            {
                                itemToSpawnFleckList.Clear();
                                isSpawningFleck = false;
                            }
                        }
                    }
                }
                else if (!Pawn.psychicEntropy.IsCurrentlyMeditating && isSpawningFleck)
                {
                    isSpawningFleck = false;
                    itemToSpawnFleckList.Clear();
                }
            }

        }
        public void ConsumeSource()
        {
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Deterioration, 1, instigator: Pawn);
            isSpawningFleck = true;
            amountToSpawn = QiSourceList.Count;
            amountSpawned = 0;
            if (!QiCrystalList.NullOrEmpty())
            {
                foreach (var item in QiCrystalList)
                {
                    CompQiStorage comps = item.TryGetComp<CompQiStorage>();
                    if (comps != null && comps.curStored > 0)
                    {
                        float num = Cultivation_Utility.getItemQiValueForPawn(item.def.tradeTags.Where(x => x.Contains("Qi_Source_Tier")).FirstOrDefault());
                        comps.DistributeQi(num);
                    }
                }
            }
            foreach (var item in QiSourceList)
            {
                if (item.DestroyedOrNull())
                {
                    continue;
                }
                CompQiStorage comps = item.TryGetComp<CompQiStorage>();
                if (comps != null && comps.curStored > 0)
                {
                    comps.DistributeQi(QiSourceWithValue[item]);
                }
                if (item.def.useHitPoints && !item.def.tradeTags.Contains("Unlimited_Source"))
                {
                    if (item.stackCount <= 1)
                    {
                        item.HitPoints--;
                        if (item?.HitPoints <= 0)
                        {
                            item?.Destroy();
                        }
                    }
                    else if (item.stackCount > 1)
                    {
                        if (item.HitPoints - 1 >= 1)
                        {
                            item.HitPoints--;
                            if (item.HitPoints <= item.MaxHitPoints * 0.01f)
                            {
                                item.stackCount--;
                                item.HitPoints = item.MaxHitPoints;
                            }
                        }
                        else
                        {
                            item.stackCount--;
                            item.HitPoints = item.MaxHitPoints;
                        }

                    }

                }
            }
        }
        public void CheckForItemChange()
        {
            for (int i = 0; i < QiCrystalList.Count - 1; i++)
            {
                Thing tempThing = QiSourceList[i];
                if (tempThing.DestroyedOrNull())
                {
                    QiCrystalList.Remove(tempThing);
                }
                float num = 6f * 6f;
                if (tempThing?.Position.DistanceToSquared(Pawn.Position) > num || tempThing?.TryGetComp<CompQiStorage>()?.curStored <= 0)
                {
                    QiCrystalList.Remove(tempThing);
                }
            }
            for (int i = 0; i < QiSourceList.Count - 1; i++)
            {
                Thing tempThing = QiSourceList[i];
                if (!tempThing.Spawned || tempThing.Map == null)
                {
                    QiSourceList.Remove(tempThing);
                    QiSourceWithType.Remove(tempThing);
                    QisourceTypeWithStringCached.Remove(tempThing);
                    itemToSpawnFleckList.Remove(tempThing.DrawPos);
                    if (QiSourceWithValue.Keys.Contains(tempThing))
                    {
                        totalSeverityChange -= QiSourceWithValue[tempThing];
                    }
                    QiSourceWithValue.Remove(tempThing);
                }
                if (tempThing.DestroyedOrNull())
                {
                    QiSourceList.Remove(tempThing);
                    QiSourceWithType.Remove(tempThing);
                    QisourceTypeWithStringCached.Remove(tempThing);
                    itemToSpawnFleckList.Remove(tempThing.DrawPos);
                    if (totalSeverityChange > 0f)
                    {
                        if (QiSourceWithValue.Keys.Contains(tempThing))
                        {
                            totalSeverityChange -= QiSourceWithValue[tempThing];
                        }
                    }
                    QiSourceWithValue.Remove(tempThing);
                }
                float num = 6f * 6f;
                if (tempThing?.Position.DistanceToSquared(Pawn.Position) > num || tempThing?.TryGetComp<CompQiStorage>()?.curStored <= 0)
                {
                    QiSourceList.Remove(tempThing);
                    QiSourceWithType.Remove(tempThing);
                    QisourceTypeWithStringCached.Remove(tempThing);
                    itemToSpawnFleckList.Remove(tempThing.DrawPos);
                    if (QiSourceWithValue.Keys.Contains(tempThing))
                    {
                        totalSeverityChange -= QiSourceWithValue[tempThing];
                    }
                    QiSourceWithValue.Remove(tempThing);
                }
            }
            foreach (var item in GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, 6f, true))
            {
                if (QiCrystalList.Contains(item))
                {
                    continue;
                }
                if (item?.TryGetComp<CompQiStorage>()?.curStored <= 0)
                {
                    continue;
                }
                if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Contains("Qi_Source_Crystal"))
                {
                    QiCrystalList.Add(item);
                }
            }
            bool flag = CultivatorOfTheRimMod.settings.isCultivatorNeedHighTierQi;
            int numTier = currentRealmTier;
            bool isQiGathering = Props.requireQiSource;
            string neededQiTier = Cultivation_Utility.neededQiTier(numTier);
            foreach (var item in GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, 6f, true))
            {
                if (QiSourceList.Contains(item))
                {
                    continue;
                }
                if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Contains("Qi_Source"))
                {
                    if (flag && isQiGathering)
                    {
                        if (!item.def.tradeTags.Contains(neededQiTier))
                        {
                            continue;
                        }
                    }
                    QiSourceList.Add(item);
                    foreach (var tag in item.def.tradeTags)
                    {
                        float num = Cultivation_Utility.getQiModifierForPawn(Pawn, tag);
                        float num2 = Cultivation_Utility.getItemQiValueForPawn(tag);
                        string text = Cultivation_Utility.getQiTypeString(tag);
                        QiSourceWithType.SetOrAdd(item, num);
                        if (num2 > 0 && !QiSourceWithValue.Keys.Contains(item))
                        {
                            QiSourceWithValue.SetOrAdd(item, num2);
                        }
                        if (text != null && text != "N/A")
                        {
                            QisourceTypeWithStringCached.SetOrAdd(item, text);
                        }
                    }

                }
            }
            if (!QiSourceWithType.NullOrEmpty())
            {
                multiplierForQiType = 1f;
                foreach (var item in QiSourceWithType)
                {
                    multiplierForQiType *= item.Value;
                }
            }
            if (!QiSourceWithValue.NullOrEmpty())
            {
                totalSeverityChange = 0f;
                foreach (var item in QiSourceWithValue)
                {
                    totalSeverityChange += item.Value;
                }
            }
        }
        public void UpdateSourceList()
        {
            if (Pawn.Map == null)
            {
                return;
            }
            isInitialChecked = true;
            QiSourceList.Clear();
            QiCrystalList.Clear();
            QiSourceWithType.Clear();
            QiSourceWithValue.Clear();
            QisourceTypeWithStringCached.Clear();
            itemToSpawnFleckList.Clear();
            multiplierForQiType = 1f;
            totalSeverityChange = 0f;
            bool flag = CultivatorOfTheRimMod.settings.isCultivatorNeedHighTierQi;
            int numTier = currentRealmTier;
            bool isQiGathering = Props.requireQiSource;
            string neededQiTier = Cultivation_Utility.neededQiTier(numTier);
            foreach (var item in GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, 6f, true))
            {
                if (QiCrystalList.Contains(item))
                {
                    continue;
                }
                if (item?.TryGetComp<CompQiStorage>()?.curStored <= 0)
                {
                    continue;
                }
                if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Contains("Qi_Source_Crystal"))
                {
                    QiCrystalList.Add(item);
                }
            }
            foreach (var item in GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, 6f, true))
            {
                if (QiSourceList.Contains(item))
                {
                    continue;
                }
                if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Contains("Qi_Source"))
                {
                    if (flag && isQiGathering)
                    {
                        if (!item.def.tradeTags.Contains(neededQiTier))
                        {
                            continue;
                        }
                    }
                    QiSourceList.Add(item);
                    foreach (string item2 in item.def.tradeTags)
                    {
                        float num = Cultivation_Utility.getQiModifierForPawn(Pawn, item2);
                        float num2 = Cultivation_Utility.getItemQiValueForPawn(item2);
                        string text = Cultivation_Utility.getQiTypeString(item2);
                        QiSourceWithType.SetOrAdd(item, num);
                        if (num2 > 0 && !QiSourceWithValue.Keys.Contains(item))
                        {
                            QiSourceWithValue.Add(item, num2);
                        }
                        if (text != null && text != "N/A")
                        {
                            QisourceTypeWithStringCached.SetOrAdd(item, text);
                        }
                    }

                }
            }
            if (!QiSourceWithType.NullOrEmpty())
            {
                multiplierForQiType = 1f;
                foreach (var item in QiSourceWithType)
                {
                    multiplierForQiType *= item.Value;
                }
            }
            if (!QiSourceWithValue.NullOrEmpty())
            {
                totalSeverityChange = 0f;
                foreach (var item in QiSourceWithValue)
                {
                    totalSeverityChange += item.Value;
                }
            }
            if (QiSourceList.NullOrEmpty())
            {
                isNearbyQi = false;
            }
            else
            {
                isNearbyQi = true;
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            yield return new Command_Action
            {
                defaultLabel = CultivatorOfTheRimMod.settings.isBreakthroughCanFailForHumanlike ? "Attempt to Breakthrough: " + Cultivation_Utility.GetBreakthroughChance(Pawn, Props.nextLevel).ToStringPercent("0.00") : "Attempt to Breakthrough",
                defaultDesc = "Attempt to Breakthrough" + "\n" + "Tribulation Chance: " + Pawn.GetStatValue(CTR_DefOf.TribulationChance, cacheStaleAfterTicks: 250).ToStringPercent(),
                icon = ContentFinder<Texture2D>.Get(Props.uiIcon),
                action = delegate
                {
                    if (Props.nextLevel == null || Props.nextLevel == Props.currentLevel)
                    {
                        Messages.Message("pawn has reach the peak, there no more higher realm than " + parent.Label, MessageTypeDefOf.NeutralEvent);
                    }
                    else if (parent.Severity < parent.def.maxSeverity)
                    {
                        Messages.Message("realm level hasn't reach it peak state yet", MessageTypeDefOf.NeutralEvent);
                    }
                    else
                    {
                        if (parent.Severity >= parent.def.maxSeverity && !Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (Props.nextLevel != null && Props.currentLevel != Props.nextLevel))
                        {
                            ((Hediff_CultivationLevel)parent).Cultivation_Advance(Props.shouldGetTribulation, Props.guaranteedTrib, Props.currentLevel, Props.nextLevel ?? Props.currentLevel, Props.breakingThroughDuration.RandomInRange, Props.TribulationStrikeInterval, Props.curves);
                            Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, Pawn);
                            job.count = 1;
                            Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        }
                        else if (Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                        {
                            Messages.Message("pawn in the process of breakingthrough", MessageTypeDefOf.NeutralEvent);
                        }
                    }
                }
            };
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
        public void IncreaseSeverity()
        {
            if (requireQiSource && CultivatorOfTheRimMod.settings.isCulSpeedAffectedByEnviaronment)
            {
                float num = Props.severityPerTriggerRange.RandomInRange;
                float num2 = Mathf.Min(Mathf.Max(totalSeverityChange / 10, 0f), 3f);
                float sev = Cultivation_Utility.CalculateCultivationGained(parent,QiSourceList,num, num2,multiplierForQiType,CultivationSpeed);
                if (QiSourceList.NullOrEmpty() && CultivatorOfTheRimMod.settings.isCultivatorNeedQiSourceToImprove)
                {
                    if (CultivatorOfTheRimMod.settings.isShowingCultivateExpText)
                    {
                        if (Find.CameraDriver.CurrentViewRect.Contains(Pawn.Position))
                        {
                            Cultivation_Utility.ThrowText(Pawn.DrawPos,Pawn.Map, sev.ToString("0.000") + " ( " + "no Qi source" + " ) ", Color.red);
                        }
                    }
                }
                else
                {
                    parent.Severity += sev;
                    if (CultivatorOfTheRimMod.settings.isShowingCultivateExpText)
                    {
                        if (Find.CameraDriver.CurrentViewRect.Contains(Pawn.Position))
                        {
                            Cultivation_Utility.ThrowText(Pawn.Position.ToVector3Shifted(), Pawn.Map, sev.ToString("0.000") + " ( " + num.ToString("0.000") + " + " + Mathf.Min(Mathf.Max(totalSeverityChange / 10, 0f), 3f) + " * " + multiplierForQiType + " ) ", Color.green);
                        }
                    }
                }
            }
            else
            {
                float sev = Props.severityPerTriggerRange.RandomInRange * Pawn.GetStatValue(CTR_DefOf.CultivationSpeed, cacheStaleAfterTicks: 250);
                parent.Severity += sev;
                if (CultivatorOfTheRimMod.settings.isShowingCultivateExpText)
                {
                    if (Find.CameraDriver.CurrentViewRect.Contains(Pawn.Position))
                    {
                        MoteMaker.ThrowText(Pawn.Position.ToVector3(), Pawn.Map, sev.ToString("0.000"), Color.green);
                    }
                }
            }
        }
        public void ProdigyCultivate()
        {
            float sev = Props.severityPerTriggerRange.RandomInRange;
            if (parent.Severity < parent.def.maxSeverity)
            {
                parent.Severity += sev;
            }

        }
        public void AnimalCultivate()
        {
            float sev = Props.severityPerTriggerRange.RandomInRange;
            if (parent.Severity < parent.def.maxSeverity)
            {
                parent.Severity += sev;
            }

        }
        public void AnimalBreakthrought()
        {
            if (parent.Severity >= parent.def.maxSeverity && !Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (Props.nextLevel != null && Props.currentLevel != Props.nextLevel))
            {
                ((Hediff_CultivationLevel)parent).Cultivation_Advance(Props.shouldGetTribulation, Props.guaranteedTrib, Props.currentLevel, Props.nextLevel ?? Props.currentLevel, Props.breakingThroughDuration.RandomInRange, Props.TribulationStrikeInterval, Props.curves);
                Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, Pawn);
                job.count = 1;
                Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }
    }
}
