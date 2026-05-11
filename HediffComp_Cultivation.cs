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
using System.Threading.Tasks;
using UnityEngine.Assertions.Must;
using System.Security.Cryptography;

namespace CultivatorOfTheRim
{
    public class HediffComp_Cultivation : HediffComp
    {
        public HediffCompProperties_Cultivation Props => (HediffCompProperties_Cultivation)props;
        public Hediff_CultivationBase cultivationBase => parent as Hediff_CultivationBase;
        public CultivationHediffDef CulDef => cultivationBase.cultivationDef;

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
                if(requireQiSource)
                {
                    int value = CulDef.realmPower;
                    if (value >= 7)
                    {
                        return true;
                    }
                }                
                return false;
            }
        }

        private bool isFixedAge
        {
            get
            {
                if(CultivatorOfTheRimMod.settings.isDeAgingPawn)
                {
                    return CulDef.realmPower >= 7;
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
                return CulDef.realmPower;
            }
        }

        private List<Thing> QiSourceList = new List<Thing>();

        private List<Thing> QiCrystalList = new List<Thing>();

        private Dictionary<Thing, float> QiSourceWithType = new Dictionary<Thing, float>();

        private Dictionary<Thing,float> QiSourceWithValue = new Dictionary<Thing, float>();

        private Dictionary<Thing, string> QisourceTypeWithStringCached = new Dictionary<Thing, string>();

        private Dictionary<Vector3,Thing> itemToSpawnFleckList = new Dictionary<Vector3, Thing>();
        private IDictionary<Thing,string> QisourceTypeWithStringGet
        {
            get
            {                
                return QisourceTypeWithStringCached;
            }
        }
        private float multiplierForQiType = 1f;

        private float totalSeverityChange = 0f;
        public override void CompPostMake()
        {
            changeToNeedsEmptyCached = Props.changeToNeeds.NullOrEmpty();
            interval = Props.tickInterval;
            requireQiSource = Props.requireQiSource;
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            ApplyBonusHediff();
            //UpdateHealthScaleCache();
        }
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            //StaticCollectionCached.PawnHealthScaleCached.Remove(Pawn);
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref changeToNeedsEmptyCached, "changeToNeedsEmptyCached",true);
            Scribe_Values.Look(ref interval, "interval", 60);
            Scribe_Values.Look(ref requireQiSource, "requireQiSource", false);
            Scribe_Values.Look(ref multiplierForQiType, "multiplierForQiType", 1f);
            Scribe_Values.Look(ref totalSeverityChange, "totalSeverityChange", 0.1f);
            Scribe_Values.Look(ref HealthScaleMul, "HealthScaleMul", 1f);
            Scribe_Values.Look(ref CultivationSpeed, "CultivationSpeed", 1f);
            if(Props.requireQiSource)
            {
                if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    if (!QiSourceList.NullOrEmpty())
                    {
                        QiSourceList.Clear();
                    }
                    if (!QiSourceWithType.NullOrEmpty())
                    {
                        QiSourceWithType.Clear();
                    }
                    if (!QiSourceWithValue.NullOrEmpty())
                    {
                        QiSourceWithValue.Clear();
                    }
                    if (!QisourceTypeWithStringCached.NullOrEmpty())
                    {
                        QisourceTypeWithStringCached.Clear();
                    }
                    if (!itemToSpawnFleckList.NullOrEmpty())
                    {
                        itemToSpawnFleckList.Clear();
                    }
                    multiplierForQiType = 1f;
                    totalSeverityChange = 0f;
                    isInitialChecked = false;
                }
            }
        }
        private IEnumerable<NeedListInfoInner> NeedsGet
        {
            get
            { 
                if(NeedListCached.NullOrEmpty() && !changeToNeedsEmptyCached)
                {
                    foreach (var item in Props.changeToNeeds)
                    {
                        Need newNeed = Pawn.needs.TryGetNeed(item.needDef);
                        NeedListInfoInner info = new NeedListInfoInner(newNeed,item.minValue);
                        NeedListCached.Add(info);
                    };
                }
                return NeedListCached;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (Pawn.IsHashIntervalTick(250,delta))
            {
                //Log.Message("get new Health Scale: " + HealthScaleMul);                
                CultivationSpeed = Pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
                //Log.Message("get new Cul Speed: " + CultivationSpeed);
            }
            if (Pawn.RaceProps.IsMechanoid)
            {
                return;
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
            if (Pawn.RaceProps.Humanlike)
            {
                if (!Pawn.Spawned)
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
                    return;
                }
                if (parent.pawn.IsHashIntervalTick(interval, delta) && parent.Severity < parent.def.maxSeverity)
                {
                    if (Pawn.story?.traits?.GetTrait(CTR_DefOf.CTR_CultivationProdigy) != null || Pawn.story.AllBackstories.Contains(CTR_DefOf.CTR_ImmortalChild))
                    {
                        ProdigyCultivate();
                    }
                    if (Pawn.story?.GetBackstory(BackstorySlot.Childhood) == CTR_DefOf.CTR_ImmortalChild)
                    {
                        ProdigyCultivate();
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
                if (requireQiSource && parent.Severity < parent.def.maxSeverity && CultivatorOfTheRimMod.settings.isCulSpeedAffectedByEnviaronment)
                {
                    if (parent.pawn.IsHashIntervalTick(2500, delta))
                    {
                        if (parent.pawn.psychicEntropy.IsCurrentlyMeditating && isNearbyQi)
                        {
                            UpdateSourceList();
                        }
                        else if (!Pawn.psychicEntropy.IsCurrentlyMeditating && !QiSourceList.NullOrEmpty())
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
                    if (Pawn.IsHashIntervalTick(250, delta))
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
                if (parent.Severity < parent.def.maxSeverity)
                {
                    if (CultivatorOfTheRimMod.settings.isSpawnQiFleck)
                    {
                        if (isSpawningFleck && Pawn.psychicEntropy.IsCurrentlyMeditating)
                        {
                            if (Pawn.IsHashIntervalTick(tickTillNextFleck, delta))
                            {
                                if (!QiSourceList.NullOrEmpty())
                                {
                                    SpawnOrb();
                                }
                            }
                        }
                        else if (!Pawn.psychicEntropy.IsCurrentlyMeditating && isSpawningFleck)
                        {
                            isSpawningFleck = false;
                            itemToSpawnFleckList.Clear();
                        }
                    }   
                    else
                    {
                        isSpawningFleck = false;
                        itemToSpawnFleckList.Clear();
                    }
                }
            }
            if (Pawn.RaceProps.Animal || Pawn.RaceProps.IsAnomalyEntity)
            {
                if (CultivatorOfTheRimMod.settings.isWildAnimalAutoCultivate)
                {
                    if (parent.Severity < parent.def.maxSeverity)
                    {
                        if (CultivatorOfTheRimMod.settings.isOnlyColonyAnimalAutoCultivate)
                        {
                            if (Pawn.Faction == Faction.OfPlayer)
                            {
                                if (parent.pawn.IsHashIntervalTick(interval, delta))
                                {
                                    AnimalCultivate();
                                }
                            }
                        }
                        else
                        {
                            if (parent.pawn.IsHashIntervalTick(interval, delta))
                            {
                                AnimalCultivate();
                            }
                        }

                    }
                }
                if (CultivatorOfTheRimMod.settings.isColonyAnimalAutoBreakthrough)
                {
                    if (Pawn.IsHashIntervalTick(250, delta))
                    {
                        if (parent.Severity >= parent.def.maxSeverity)
                        {
                            if (Pawn.Faction == Faction.OfPlayer)
                            {
                                if (Pawn.RaceProps.Animal || Pawn.RaceProps.IsAnomalyEntity)
                                {
                                    if (!Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                                    {
                                        if (CultivatorOfTheRimMod.settings.isColonyAnimalIgnoreSafetyThresholdForBreakthrough)
                                        {
                                            if (Pawn.GetStatValue(CTR_DefOf.TribulationChance, cacheStaleAfterTicks: 250) <= CultivatorOfTheRimMod.settings.tribulationSafety)
                                            {
                                                AnimalBreakthrought();
                                            }
                                        }
                                        else
                                        {
                                            AnimalBreakthrought();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (CultivatorOfTheRimMod.settings.isWildAnimalAutoBreakthrought)
                {
                    if (Pawn.IsHashIntervalTick(250, delta))
                    {
                        if (Pawn.Faction != Faction.OfPlayer)
                        {
                            if (Pawn.RaceProps.Animal || Pawn.RaceProps.IsAnomalyEntity)
                            {
                                if (parent.Severity >= parent.def.maxSeverity)
                                {
                                    if (!Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                                    {
                                        if (CultivatorOfTheRimMod.settings.isWildAnimalIgnoreSafetyThresholdForBreakthrough)
                                        {
                                            if (Pawn.GetStatValue(CTR_DefOf.TribulationChance, cacheStaleAfterTicks: 250) <= CultivatorOfTheRimMod.settings.tribulationSafety)
                                            {
                                                AnimalBreakthrought();
                                            }
                                        }
                                        else
                                        {
                                            AnimalBreakthrought();
                                        }
                                    }
                                }
                            }
                        }
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
        }

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
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
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (!Pawn.Dead) return;
            if(Pawn.Corpse == null) return;
            if(!Pawn.Corpse.Spawned) return;
            if((Pawn.RaceProps.Animal || Pawn.RaceProps.IsAnomalyEntity) && CultivatorOfTheRimMod.settings.isAnimalDropBeastCore)
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
                        specialDrop.TryGetComp<CompBeastCore>().ownerCultivation = CulDef;
                    }
                    GenSpawn.Spawn(specialDrop, pos, map);
                }
            }
            else if (Pawn.RaceProps.Humanlike)
            {
                Map map = parent?.pawn?.Corpse?.Map;
                IntVec3 pos = parent.pawn.Corpse.Position;
                if (map != null)
                {
                    int level = CulDef.realmPower;
                    if (level >= 7 && level < 14)
                    {
                        Thing specialDrop = ThingMaker.MakeThing(CTR_DefOf.CTR_GoldenCore_Core);
                        if (specialDrop.TryGetComp<CompBeastCore>() != null)
                        {
                            specialDrop.TryGetComp<CompBeastCore>().ownerName = Pawn.LabelShort;
                            specialDrop.TryGetComp<CompBeastCore>().ownerDef = Pawn.def;
                            specialDrop.TryGetComp<CompBeastCore>().ownerCultivation = CulDef;
                        }
                        GenSpawn.Spawn(specialDrop, pos, map);
                    }
                    if (level >= 14)
                    {
                        Thing specialDrop = ThingMaker.MakeThing(CTR_DefOf.CTR_ImmortalCore);
                        if (specialDrop.TryGetComp<CompBeastCore>() != null)
                        {
                            specialDrop.TryGetComp<CompBeastCore>().ownerName = Pawn.LabelShort;
                            specialDrop.TryGetComp<CompBeastCore>().ownerDef = Pawn.def;
                            specialDrop.TryGetComp<CompBeastCore>().ownerCultivation = CulDef;
                        }
                        GenSpawn.Spawn(specialDrop, pos, map);
                    }
                }
            }
        }
        public void SpawnOrb()
        {
            if (itemToSpawnFleckList.NullOrEmpty())
            {
                foreach (var item in QiSourceWithValue)
                {
                    if (item.Key != null && !item.Key.DestroyedOrNull())
                    {
                        itemToSpawnFleckList.SetOrAdd(item.Key.DrawPos, item.Key);
                    }                    
                }
            }
            if(!itemToSpawnFleckList.NullOrEmpty())
            {                
                Thing tempItem = itemToSpawnFleckList.First().Value;
                Vector3 origin = itemToSpawnFleckList.FirstOrDefault().Key;
                string tier = Cultivation_Utility.getItemQiTier(tempItem);
                string type = QisourceTypeWithStringCached.ContainsKey(tempItem) ? QisourceTypeWithStringCached[tempItem] : "Neutral_Qi";
                if (Find.CameraDriver.InViewOf(Pawn))
                {
                    SpawnQiOrbColor(tier, type, origin);
                }
                amountSpawned++;
                itemToSpawnFleckList.Remove(origin);
                if (amountSpawned >= amountToSpawn)
                {
                    itemToSpawnFleckList.Clear();
                    isSpawningFleck = false;
                }
            }
            
        }
        public void ConsumeSource()
        {
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Deterioration,1,instigator: Pawn);            
            isSpawningFleck = true;
            amountToSpawn = QiSourceList.Count;
            amountSpawned = 0;
            if(!QiCrystalList.NullOrEmpty())
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

            for (var i = 0; i < QiSourceList.Count; i++)
            {
                var item = QiSourceList[i];
                if (item.DestroyedOrNull())
                {
                    continue;
                }

                //SpawnQiOrbColor(item);
                CompQiStorage comps = item.TryGetComp<CompQiStorage>();
                if (comps != null && comps.curStored > 0)
                {
                    comps.DistributeQi(QiSourceWithValue[item]);
                }

                if (item.def.useHitPoints && !item.def.tradeTags.Contains("Unlimited_Source"))
                {
                    /*MoteMovingToPoint moteMovingToPoint = (MoteMovingToPoint)ThingMaker.MakeThing(CTR_DefOf.CTR_AbsorbQiOrb);
                    moteMovingToPoint.Attach(item, Pawn);
                    moteMovingToPoint.exactPosition = item.DrawPos;
                    moteMovingToPoint.exactRotation = item.DrawPos.AngleToFlat(Pawn.DrawPos) + 90f;
                    GenPlace.TryPlaceThing(moteMovingToPoint, item.Position, Pawn.Map,ThingPlaceMode.Direct);*/
                    if (item.stackCount <= 1)
                    {
                        //item.TakeDamage(dinfo);
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
                            //item.TakeDamage(dinfo);
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
                /*float num = 6f * 6f;
                if (item.DestroyedOrNull() || item.Position.DistanceToSquared(Pawn.Position) > num)
                {
                    QiSourceList.Remove(item);
                    QiSourceWithType.Remove(item);
                    totalSeverityChange -= QiSourceWithValue[item];
                    QiSourceWithValue.Remove(item);

                    continue;
                }
                if (item?.stackCount > 1)
                {
                    item.stackCount--;
                }
                else
                {
                    item?.Destroy();
                }*/
            }
        }
        public FleckDef GetQiOrbType(string item)
        {
            switch (item)
            {
                case "Neutral_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbPure;
                case "Yang_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbPure;
                case "Yin_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbPure;
                case "Cold_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbWater;
                case "Metal_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbMetal;
                case "Water_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbWater;
                case "Wood_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbWood;
                case "Fire_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbFire;
                case "Earth_Qi":
                    return CTR_DefOf.CTR_AbsorbQiOrbEarth;
                default:
                    return CTR_DefOf.CTR_AbsorbQiOrbPure;
            }
            //return CTR_DefOf.CTR_AbsorbQiOrbPure;
        }
        public void SpawnQiOrbColor(string tier,string type ,Vector3 vector3)
        {
            Vector3 vector = vector3 + new Vector3(Rand.Range(-0.5f, 0.5f), 0.5f, Rand.Range(-0.5f, 0.5f));
            float num = Cultivation_Utility.getItemQiValueForPawn(tier);
            num *= 12f;
            Map map = Pawn.Map;
            Cultivation_Utility.ThrowObjectAt(map, vector3, Pawn.DrawPos, GetQiOrbType(type), 0.25f + num, 0.25f + num);
        }
        public void CheckForItemChange()
        {
            //List<string> st = new List<string>() { "a","b","c","d"};
            IReadOnlyList<Thing> crystalList = new List<Thing>(QiCrystalList);
            for(int i = 0;i < crystalList.Count;i++)
            {
                Thing tempThing = crystalList[i];                
                if (tempThing.DestroyedOrNull())
                {
                    QiCrystalList.Remove(tempThing);
                }
                float num = 6f * 6f;
                if (tempThing?.Position.DistanceToSquared(Pawn.Position) > num || tempThing?.TryGetComp<CompQiStorage>()?.curStored <= 0)
                {
                    QiCrystalList.Remove(tempThing);
                }
                /*if (!GenSight.LineOfSightToThing(Pawn.Position, tempThing, Pawn.Map))
                {
                    QiCrystalList.Remove(tempThing);
                }*/
            }
            IReadOnlyList<Thing> sourceList = new List<Thing>(QiSourceList);
            for (int i = 0;i < sourceList.Count;i++)
            {

                Thing tempThing = sourceList[i];
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
                        if(QiSourceWithValue.Keys.Contains(tempThing))
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
                /*if (!GenSight.LineOfSightToThing(Pawn.Position, tempThing, Pawn.Map))
                {
                    QiSourceList.Remove(tempThing);
                    QiSourceWithType.Remove(tempThing);
                    QisourceTypeWithStringCached.Remove(tempThing);
                    itemToSpawnFleckList.Remove(tempThing.DrawPos);
                    if (totalSeverityChange > 0f)
                    {
                        totalSeverityChange -= QiSourceWithValue[tempThing];
                    }
                    QiSourceWithValue.Remove(tempThing);
                }*/
            }
            IReadOnlyList<Thing> crystalThingList = new List<Thing>(GenRadial.RadialDistinctThingsAround(Pawn.PositionHeld, Pawn.MapHeld, 6f, true));
            for (var i = 0; i < crystalThingList.Count; i++)
            {
                var item = crystalThingList[i];
                if (item.DestroyedOrNull()) continue;
                if (QiCrystalList.Contains(item))
                {
                    continue;
                }

                if (item?.TryGetComp<CompQiStorage>()?.curStored <= 0)
                {
                    continue;
                }

                if (!item.HasTradeTag("Qi_Source_Crystal"))
                {
                    QiCrystalList.Add(item);
                }
            }

            bool flag = CultivatorOfTheRimMod.settings.isCultivatorNeedHighTierQi;
            int numTier = currentRealmTier;
            bool isQiGathering = Props.requireQiSource;
            string neededQiTier = Cultivation_Utility.neededQiTier(numTier);
            IReadOnlyList<Thing> itemThingList = new List<Thing>(GenRadial.RadialDistinctThingsAround(Pawn.PositionHeld, Pawn.MapHeld, 6f, true));
            for (var i = 0; i < itemThingList.Count; i++)
            {
                var item = itemThingList[i];
                //if (item is Plant_SpiritPlant) continue;
                if (item.DestroyedOrNull()) continue;
                if (QiSourceList.Contains(item))
                {
                    continue;
                }

                /*if (!GenSight.LineOfSightToThing(Pawn.Position, item, Pawn.Map))
                {
                    continue;
                }*/
                if (item.def.HasTradeTag("Qi_Source"))
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
                IReadOnlyDictionary<Thing, float> keyValuePairs = new Dictionary<Thing, float>(QiSourceWithType);
                foreach (var item in keyValuePairs)
                {
                    if (item.Key.DestroyedOrNull())
                    {
                        QiSourceWithType.Remove(item.Key);
                        continue;
                    }
                    multiplierForQiType *= item.Value;
                }
            }
            if (!QiSourceWithValue.NullOrEmpty())
            {
                totalSeverityChange = 0f;
                IReadOnlyDictionary<Thing,float> keyValuePairs = new Dictionary<Thing,float>(QiSourceWithValue);
                foreach (var item in keyValuePairs)
                {
                    if(item.Key.DestroyedOrNull())
                    {
                        QiSourceWithValue.Remove(item.Key);
                        continue;
                    }
                    totalSeverityChange += item.Value;
                }
            }
        }
        public void UpdateSourceList()
        {
            if(Pawn.Map == null)
            {
                return;
            }
            try
            {
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
                IReadOnlyList<Thing> crystalThings = new List<Thing>(GenRadial.RadialDistinctThingsAround(Pawn.PositionHeld, Pawn.MapHeld, 6f, true));
                for (var i = 0; i < crystalThings.Count; i++)
                {
                    var item = crystalThings[i];
                    if (item.DestroyedOrNull()) continue;
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

                IReadOnlyList<Thing> itemThings = [.. GenRadial.RadialDistinctThingsAround(Pawn.PositionHeld, Pawn.MapHeld, 6f, true)];
                for (var i = 0; i < itemThings.Count; i++)
                {
                    var item = itemThings[i];
                    //if (item is Plant_SpiritPlant) continue;
                    if (item.DestroyedOrNull()) continue;
                    if (QiSourceList.Contains(item))
                    {
                        continue;
                    }

                    /*if (!GenSight.LineOfSightToThing(Pawn.Position, item, Pawn.Map))
                    {
                        continue;
                    }*/
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
            catch (Exception ex)
            {
                Log.Message($"[CultivatorOfTheRim] Error ticking {Pawn.LabelShort}, something went wrong when cultivating." +
                    $"\n{ex}");
            }
            
        }
        public override IEnumerable<Gizmo> CompGetGizmos()
		{
            Command_Action breakthrough = new Command_Action();
            breakthrough.defaultLabel = CultivatorOfTheRimMod.settings.isBreakthroughCanFailForHumanlike ? "Attempt to Breakthrough: " + Cultivation_Utility.GetBreakthroughChance(Pawn, false).ToStringPercent("0.00") : "Attempt to Breakthrough";
            if(CulDef.realmPower >= 6)
            {
                breakthrough.defaultDesc = "Attempt to Breakthrough" + "\n" + "Tribulation Chance: " + Pawn.GetStatValue(CTR_DefOf.TribulationChance, cacheStaleAfterTicks: 250).ToStringPercent();
            }
            else
            {
                breakthrough.defaultDesc = "Attempt to Breakthrough";
            }
            breakthrough.icon = Props.uiIconTexture;
            breakthrough.action = delegate
            {
                CultivationHediffDef nextLevel = (CultivationHediffDef)(Props.nextLevel != null ? Props.nextLevel : Props.nextLevels.RandomElementByWeight(x => x.weight).nextRealm);
                if (nextLevel == null || nextLevel == Props.currentLevel)
                {
                    Messages.Message("pawn has reach the peak, there no more higher realm than " + parent.Label, MessageTypeDefOf.NeutralEvent);
                }
                else if (parent.Severity < parent.def.maxSeverity)
                {
                    Messages.Message("realm level hasn't reach it peak state yet", MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    if (parent.Severity >= parent.def.maxSeverity && !Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (nextLevel != null && Def != nextLevel))
                    {
                        ((Hediff_CultivationLevel)parent).Cultivation_Advance(Props.shouldGetTribulation, CulDef, nextLevel, Props.breakingThroughDuration.RandomInRange, Props.TribulationStrikeInterval);
                        Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, Pawn);
                        job.count = 1;
                        Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
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
                        for(int i = 0;i < parent.def.stages.Count;i++)
                        {
                            if (parent.def.stages[i].minSeverity > curStage)
                            {
                                parent.Severity = parent.def.stages[i].minSeverity;
                                if(i == parent.def.stages.Count)
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
            float sev = Props.severityPerTriggerRange.RandomInRange * CultivationSpeed * CultivatorOfTheRimMod.settings.animalCultivationSpeedMultiplier;
            if(parent.Severity < parent.def.maxSeverity)
            {
                parent.Severity += sev;
            }            
            
        }
        public void AnimalBreakthrought()
        {
            if (parent.Severity >= parent.def.maxSeverity && !Pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (Props.nextLevel != null && Props.currentLevel != Props.nextLevel))
            {
                ((Hediff_CultivationLevel)parent).Cultivation_Advance(Props.shouldGetTribulation, CulDef, (CultivationHediffDef)Props.nextLevel, Props.breakingThroughDuration.RandomInRange, Props.TribulationStrikeInterval);
                Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, Pawn);
                job.count = 1;
                Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }
        public void IncreaseSeverity()
        {
            /*float sev = Props.severityPerTrigger > 0 ? Props.severityPerTrigger * Pawn.GetStatValue(CTR_DefOf.CultivationSpeed) : Props.severityPerTriggerRange.RandomInRange * Pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
            parent.Severity += sev;*/
            if (requireQiSource && CultivatorOfTheRimMod.settings.isCulSpeedAffectedByEnviaronment)
            {                
                float num = Props.severityPerTriggerRange.RandomInRange;
                float sev = num;
                //sev += Mathf.Min(Mathf.Max(totalSeverityChange,0f), Props.severityPerTriggerRange.max * 5);
                float num2 = Mathf.Min(Mathf.Max(totalSeverityChange / 10, 0f), 3f);
                float num3 = multiplierForQiType * CultivatorOfTheRimMod.settings.globalQiTypeMultiplier;
                num3 = Mathf.Max(num3,1f);
                sev += num2;
                sev *= num3;
                sev *= CultivationSpeed;
                if (QiSourceList.NullOrEmpty() && CultivatorOfTheRimMod.settings.isCultivatorNeedQiSourceToImprove)
                {
                    sev = 0;
                    if (CultivatorOfTheRimMod.settings.isShowingCultivateExpText)
                    {
                        if (Find.CameraDriver.CurrentViewRect.Contains(Pawn.Position))
                        {
                            MoteMaker.ThrowText(Pawn.Position.ToVector3Shifted(), Pawn.Map, sev.ToString("0.000") + " ( " + "no Qi source" + " ) " , Color.red);
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
                            string text = $"{sev.ToString("0.00")} ( {num.ToString("0.00")} + {num2} * {num3} * {CultivationSpeed} )";
                            MoteMaker.ThrowText(Pawn.Position.ToVector3Shifted(), Pawn.Map, text, Color.green);
                        }
                    }
                }                                               
            }
            else
            {
                float sev = Props.severityPerTriggerRange.RandomInRange * Pawn.GetStatValue(CTR_DefOf.CultivationSpeed,cacheStaleAfterTicks: 250);
                parent.Severity += sev;
                if(CultivatorOfTheRimMod.settings.isShowingCultivateExpText)
                {
                    if (Find.CameraDriver.CurrentViewRect.Contains(Pawn.Position))
                    {
                        MoteMaker.ThrowText(Pawn.Position.ToVector3Shifted(), Pawn.Map, sev.ToString("0.000"), Color.green);
                    }
                }                               
            }
            /*float num = 6f * 6f;
            foreach (var thing in Pawn.Map.listerThings.AllThings)
            {
                if (!thing.DestroyedOrNull())
                {
                    float num2 = thing.Position.DistanceToSquared(Pawn.Position);
                    if (num2 <= num && !thing.def.tradeTags.NullOrEmpty() && thing.def.tradeTags.Contains("Qi_Source"))
                    {
                        float sev = Props.severityPerTrigger > 0 ? Props.severityPerTrigger * Pawn.GetStatValue(CTR_DefOf.CultivationSpeed) : Props.severityPerTriggerRange.RandomInRange * Pawn.GetStatValue(CTR_DefOf.CultivationSpeed);
                        parent.Severity += sev;
                    }
                }
            }*/
        }
    }
}
