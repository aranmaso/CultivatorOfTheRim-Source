using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace CultivatorOfTheRim
{
    public class Plant_SpiritPlant : Plant
    {        
        private int numberOfSource = 0;

        private float curMul = 1f;

        private List<Thing> thingNearby = new List<Thing>();

        private Dictionary<Thing, float> thingNearbyWithCurMul = new Dictionary<Thing, float>();

        private List<Thing> specialThingNearby = new List<Thing>();

        private List<Thing> forbiddenThingNearby = new List<Thing>();

        private IDictionary<Pawn,float> pawnsNearby = new Dictionary<Pawn, float>();

        public bool isSimilarPlantNearby = false;
        private Pawn nearestPawn
        {
            get
            {
                if (pawnsNearby != null)
                {
                    return pawnsNearby.First().Key;
                }
                return null;
            }
        }
        public bool isDayTime => GenLocalDate.DayPercent(Map) >= 0.25f && GenLocalDate.DayPercent(Map) <= 0.75f;
        /*public bool isDayTime
        {
            get
            { 
             if(GenLocalDate.DayPercent(curMap) >= 0.25f && GenLocalDate.DayPercent(curMap) <= 0.75f)
             {
                return true;
             }
             return false;
            }
        }*/

        public int dayOfMonth => GenLocalDate.DayOfQuadrum(Map);
        public int dayOfYear => GenLocalDate.DayOfYear(Map);

        private Season season => GenLocalDate.Season(Map);
        private PlantExtension_SpiritPlant modExtensionGet;
        private PlantExtension_SpiritPlant modExtension
        {
            get
            {
                if(modExtensionGet == null)
                {
                    modExtensionGet = def.GetModExtension<PlantExtension_SpiritPlant>();
                    onlyHarvestableAtNight = modExtensionGet.onlyHarvestableAtNight;
                    onlyHarvestableAtDay = modExtensionGet.onlyHarvestableAtDay;
                    endOfMonthHarvestable = modExtensionGet.endOfMonthHarvestable;
                    middleOfMonthHarvestable = modExtensionGet.middleOfMonthHarvestable;
                    if (!modExtensionGet.specificMonthHarvestable.NullOrEmpty())
                    {
                        specificMonthHarvestable = true;
                    }
                    
                }
                return modExtensionGet;
            }
        }

        private float growthMul = 1f;
        public override float GrowthRate
        { 
            get 
            {
                float Source_Bonus = 1f;
                float DayOfMonth_Bonus = 1f;
                float Season_Bonus = 1f;
                if(base.Blighted)
                {
                    return 0f;
                }
                if (base.Spawned && !PlantUtility.GrowthSeasonNow(base.Position, base.Map) && !modExtension.ignoreTemp)
                {
                    return 0f;
                }
                if(modExtension.requireSource)
                {
                    if(numberOfSource <= 0)
                    {
                        return 0f;
                    }
                    Source_Bonus = GrowthRateFactor_Bonus;
                }
                if(modExtension.requireSpecialSource)
                {
                    if(specialThingNearby.NullOrEmpty())
                    {
                        return 0f;
                    }
                }
                if(modExtension.isNightPlant)
                {
                    if (isDayTime)
                    {
                        return 0f;
                    }                   
                }
                if (modExtension.isStopGrowingIfThingInRange)
                {
                    if (!forbiddenThingNearby.NullOrEmpty())
                    {
                        return 0f;
                    }
                }
                if (modExtension.differentEffectBasedOnTimeOfMonth)
                {
                    DayOfMonth_Bonus = modExtension.growthRateCurves.Evaluate(dayOfMonth);
                }
                if(modExtension.isAffectBySeason)
                {
                    switch(season)
                    {
                        case Season.Undefined:
                            Season_Bonus = 1f;
                            break;
                        case Season.Spring:
                            Season_Bonus = modExtension.seasonCurves.Evaluate(1);
                            break;
                        case Season.Summer:
                            Season_Bonus = modExtension.seasonCurves.Evaluate(2);
                            break;
                        case Season.Fall:
                            Season_Bonus = modExtension.seasonCurves.Evaluate(3);
                            break;
                        case Season.Winter:
                            Season_Bonus = modExtension.seasonCurves.Evaluate(4);
                            break;
                        case Season.PermanentSummer:
                            Season_Bonus = modExtension.seasonCurves.Evaluate(5);
                            break;
                        case Season.PermanentWinter:
                            Season_Bonus = modExtension.seasonCurves.Evaluate(6);
                            break;
                    }
                }
                if(modExtension.ignoreTemp)
                {
                    return ((GrowthRateFactor_Fertility * GrowthRateFactor_Light * GrowthRateFactor_NoxiousHaze) + Source_Bonus) * DayOfMonth_Bonus * Season_Bonus;
                }
                return (base.GrowthRate + Source_Bonus) * DayOfMonth_Bonus * Season_Bonus;
                
            } 
        }
        public override bool Resting
        {
            get
            {
                if(modExtension.requireSource)
                {
                    if(numberOfSource <= 0)
                    {
                        return true;
                    }
                    if(!modExtension.isNeedResting)
                    {
                        return false;
                    }
                    return base.Resting;
                }       
                if(modExtension.requireSpecialSource)
                {
                    if (specialThingNearby.NullOrEmpty())
                    {
                        return true;
                    }                    
                }
                if(modExtension.isNightPlant)
                {
                    if(GenLocalDate.HourOfDay(Map) >= 6 && GenLocalDate.HourOfDay(Map) <= 18)
                    {
                        return true;
                    }
                    return false;
                }
                return base.Resting;
            }
        }
        public float GrowthRateFactor_Bonus => 1f + 0.2f * numberOfSource * Mathf.Max(1f, curMul);

        public override void TickLong()
        {
            base.TickLong();      
            if(this.IsHashIntervalTick(1))
            {
                if (modExtension.requireSource)
                {
                    RequireSource();
                }
                if(modExtension.requireSpecialSource)
                {
                    RequireSpecialSource();
                }
                if (modExtension.speedUpNearbyPlant)
                {
                    SpeedUpPlantGrow();
                }
                if(modExtension.isStopGrowingIfThingInRange)
                {
                    PreventGrowthIfForbiddenInRange();
                }
            }                        
        }
        public override void Tick()
        {
            base.Tick(); 
            if(modExtension.isAffectByPawnApproaching)
            {
                if(this.IsHashIntervalTick(60))
                {
                    pawnsNearby = this.TryGetComp<CompGetNearbyPawn>().newList;
                }
                /*if (!pawnsNearby.EnumerableNullOrEmpty())
                {
                    growthMul = Mathf.Max(sizeCurve.Evaluate(pawnsNearby.First().Value), 1f);
                }*/

            }
            
        }
        public SimpleCurve sizeCurve = new SimpleCurve()
        {
            new CurvePoint(0,5f),
            new CurvePoint(6f,1f),
        };

        private SimpleCurve intervalCurve = new SimpleCurve()
        {
            new CurvePoint(1,5),
            new CurvePoint(6,60),
        };
        public override void PlantCollected(Pawn by, PlantDestructionMode plantDestructionMode)
        {            
            if(modExtension.differentYieldBasedOnTimeOfHarvest)
            {
                if(HarvestableNow)
                {
                    if (isDayTime)
                    {
                        Thing thing = ThingMaker.MakeThing(modExtension.dayTimeYield);
                        thing.stackCount = Mathf.FloorToInt(def.plant.harvestYield * by.GetStatValue(StatDefOf.PlantHarvestYield));
                        GenPlace.TryPlaceThing(thing, by.Position, Map, ThingPlaceMode.Near);
                    }
                    else
                    {
                        Thing thing = ThingMaker.MakeThing(modExtension.nightTimeYield);
                        thing.stackCount = Mathf.FloorToInt(def.plant.harvestYield * by.GetStatValue(StatDefOf.PlantHarvestYield));
                        GenPlace.TryPlaceThing(thing, by.Position, Map, ThingPlaceMode.Near);
                    }
                }                
            }
            base.PlantCollected(by, plantDestructionMode);
        }
        public override void PostMake()
        {
            base.PostMake();
            if(modExtensionGet == null)
            {
                modExtensionGet = def.GetModExtension<PlantExtension_SpiritPlant>();
                onlyHarvestableAtNight = modExtensionGet.onlyHarvestableAtNight;
                onlyHarvestableAtDay = modExtensionGet.onlyHarvestableAtDay;
                endOfMonthHarvestable = modExtensionGet.endOfMonthHarvestable;
                middleOfMonthHarvestable = modExtensionGet.middleOfMonthHarvestable;
                if (!modExtensionGet.specificMonthHarvestable.NullOrEmpty())
                {
                    specificMonthHarvestable = true;
                }
            }
        }
        private bool onlyHarvestableAtNight;

        private bool onlyHarvestableAtDay;

        private bool endOfMonthHarvestable;

        private bool middleOfMonthHarvestable;

        private bool specificMonthHarvestable;
        public override bool HarvestableNow
        {
            get
            {
                if(onlyHarvestableAtNight)
                {
                    if(isDayTime)
                    {
                        return false;
                    }
                    return base.HarvestableNow;
                }
                if(onlyHarvestableAtDay)
                {
                    if (isDayTime)
                    {
                        return true;
                    }
                    return base.HarvestableNow;
                }
                if(endOfMonthHarvestable)
                {
                    if(dayOfYear == 1 || dayOfYear == 15 || dayOfYear == 30 || dayOfYear == 45 || dayOfYear == 60)
                    {
                        return base.HarvestableNow;
                    }
                    return false;
                }
                if(middleOfMonthHarvestable)
                {
                    if(dayOfMonth < 6 || dayOfMonth > 8)
                    {
                        return false;
                    }
                    return base.HarvestableNow;
                }
                if(specificMonthHarvestable)
                {
                    if(!modExtension.specificMonthHarvestable.Contains(season))
                    {
                        return false;
                    }
                    return base.HarvestableNow;
                }
                return base.HarvestableNow;
            }
        }
        public void PreventGrowthIfForbiddenInRange()
        {
            //clear thing out of range or null
            for (int i = 0; i < forbiddenThingNearby.Count; i++)
            {
                if (forbiddenThingNearby[i].DestroyedOrNull() || forbiddenThingNearby[i].Position.DistanceToSquared(Position) > (modExtension.radius * modExtension.radius))
                {
                    forbiddenThingNearby.Remove(forbiddenThingNearby[i]);
                }
            }

            //update item
            if (Map == null)
            {
                return;
            }
            if (Position == null)
            {
                return;
            }
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.forbiddenRange, false))
            {
                if (forbiddenThingNearby.Contains(item))
                {
                    continue;
                }                
                if(!modExtension.forbiddenTags.NullOrEmpty())
                {
                    if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Any(x => modExtension.forbiddenTags.Contains(x)))
                    {
                        forbiddenThingNearby.Add(item);
                    }
                }
                if(!modExtension.forbiddenThings.NullOrEmpty())
                {
                    if(modExtension.forbiddenThings.Contains(item.def))
                    {
                        forbiddenThingNearby.Add(item);
                    }
                }
            }
        }
        public void RequireSpecialSource()
        {
            //clear thing out of range or null
            if(!specialThingNearby.NullOrEmpty())
            {
                for (int i = 0; i < specialThingNearby.Count; i++)
                {
                    if (specialThingNearby[i].DestroyedOrNull() || specialThingNearby[i].Position.DistanceToSquared(Position) > (modExtension.radius * modExtension.radius))
                    {
                        specialThingNearby.Remove(specialThingNearby[i]);
                    }
                }
            }
            

            //update item
            if (Map == null)
            {
                return;
            }
            if (Position == null)
            {
                return;
            }
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position,Map,modExtension.radius,false))
            {
                if(specialThingNearby.Contains(item))
                {
                    continue;
                }
                if(modExtension.allowedSpecialTags.NullOrEmpty())
                {
                    continue;
                }
                if (!modExtension.forbiddenThings.NullOrEmpty() && modExtension.forbiddenThings.Contains(item.def))
                {
                    continue;
                }
                if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Any(x => modExtension.allowedSpecialTags.Contains(x)))
                {
                    specialThingNearby.Add(item);
                }
            }
        }
        public void healSelf()
        {
            if (HitPoints < MaxHitPoints)
            {
                HitPoints++;
            }
        }
        public void RequireSource()
        {
            //clear thing out of range or null
            if(!thingNearby.NullOrEmpty())
            {
                //Log.Message("Name: " + def.LabelCap);
                //Log.Message("pos: " + Position);
                //Log.Message("list not empty");                
                //int thingNearbyCount = thingNearby.Count;
                //Log.Message("get list count: " + thingNearbyCount);
                float num = modExtension.radius * modExtension.radius;
                for (int i = 0; i < thingNearby.Count - 1; i++)
                {
                    if (thingNearby[i].DestroyedOrNull())
                    {
                        //Log.Message("is null " + thingNearby[i].LabelShort);
                        //Log.Message("null pos: " + thingNearby[i].Position);
                        //numberOfSource--;
                        //Log.Message("numsource--");
                        if(!thingNearbyWithCurMul.NullOrEmpty())
                        {
                            //Log.Message("thingNearbyCurMulNotEmpty");
                            if (thingNearbyWithCurMul.Keys.Contains(thingNearby[i]))
                            {
                                //Log.Message("curListContain " + thingNearby[i]);
                                curMul -= thingNearbyWithCurMul[thingNearby[i]];
                                //Log.Message("curMul -= " + thingNearbyWithCurMul[thingNearby[i]]);
                                thingNearbyWithCurMul.Remove(thingNearby[i]);
                                //Log.Message("remove from curMul list");
                            }   
                        }
                        
                        thingNearby.Remove(thingNearby[i]);
                        //Log.Message("remove from nearby list");
                    }                    
                    else if (thingNearby[i].Position.DistanceToSquared(Position) > (modExtension.radius * modExtension.radius))
                    {
                        //Log.Message("thing got moved away");
                        //numberOfSource--;
                        if (thingNearbyWithCurMul.Keys.Contains(thingNearby[i]))
                        {
                            if (thingNearbyWithCurMul.Keys.Contains(thingNearby[i]))
                            {
                                curMul -= thingNearbyWithCurMul[thingNearby[i]];
                                thingNearbyWithCurMul.Remove(thingNearby[i]);
                            }
                        }
                        //Log.Message("remove from Nearby list(too far away)");
                        thingNearby.Remove(thingNearby[i]);
                    }
                    else
                    {
                        if (modExtension.consumeSource)
                        {
                            if (!thingNearby[i].DestroyedOrNull() && !thingNearby[i].def.tradeTags.Contains("Unlimited_Source"))
                            {
                                DoDamageToSource(thingNearby[i]);
                            }                            
                        }

                    }
                }                
            }            
            if (!thingNearbyWithCurMul.NullOrEmpty())
            {
                List<Thing> tempList = new List<Thing>(thingNearbyWithCurMul.Keys);
                for (int i = 0; i < tempList.Count - 1; i++)
                {
                    if (tempList[i].DestroyedOrNull())
                    {
                        curMul -= thingNearbyWithCurMul[tempList[i]];
                        thingNearbyWithCurMul.Remove(tempList[i]);
                    }
                    float num2 = modExtension.radius * modExtension.radius;
                    if (tempList[i].Position.DistanceToSquared(Position) > (modExtension.radius * modExtension.radius))
                    {
                        curMul -= thingNearbyWithCurMul[tempList[i]];
                        thingNearbyWithCurMul.Remove(tempList[i]);
                    }
                }
            }
            if (modExtension.healSelf && numberOfSource > 0)
            {
                healSelf();
            }
            //update nearby item
            if (Map == null)
            {
                return;
            }
            if(Position == null)
            {
                return;
            }    
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, true))
            {
                if (thingNearby.Contains(item))
                {
                    continue;
                }
                if(item == this || item.def == def)
                {
                    continue;
                }
                if(!modExtension.excludedThing.NullOrEmpty() && modExtension.excludedThing.Contains(item.def))
                {
                    continue;
                }                
                if (!item.def.tradeTags.NullOrEmpty())
                {
                    if (item.def.tradeTags.Any(x => modExtension.allowedTags.Contains(x)))
                    {
                        //numberOfSource++;
                        thingNearby.Add(item);
                    }
                }
                /*if (itemGrade == null && !item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Any(x => modExtension.allowedTags.Contains(x)))
                {
                    float num = 0f;
                    if(modExtension.allowedTags.Any(x => item.def.tradeTags.Contains(x)))
                    {
                        string text = Cultivation_Utility.qiSourceMultiplier.Keys.Where(x =>  modExtension.allowedTags.Contains(x)).FirstOrDefault();
                        num = Cultivation_Utility.getQiSouceMultiplierForPlant(text);
                    }
                    curMul += num;
                    numberOfSource++;
                    thingNearby.Add(item);
                }*/
                else continue;
            }
            foreach(var item in GenRadial.RadialDistinctThingsAround(Position,Map,modExtension.radius,true))
            {
                if(thingNearbyWithCurMul.ContainsKey(item))
                {
                    continue;
                }
                if (item == this || item.def == def)
                {
                    continue;
                }
                if (!modExtension.excludedThing.NullOrEmpty() && modExtension.excludedThing.Contains(item.def))
                {
                    continue;
                }
                CompItemGrade itemGrade = item.TryGetComp<CompItemGrade>();
                if (itemGrade != null)
                {
                    if (itemGrade.Grade > ItemGrade.Mortal)
                    {
                        string text = Cultivation_Utility.qiMultiplierFromGrade(itemGrade.Grade);
                        float num = 1f;
                        foreach (var t in Cultivation_Utility.qiSourceMultiplier)
                        {
                            if (t.Key == text)
                            {
                                num = t.Value;
                                break;
                            }
                        }
                        /*if(num > curMul)
                        {
                            curMul = num;
                        }*/                        
                        if(!thingNearby.Contains(item))
                        {
                            thingNearby.Add(item);
                        }
                        if (!thingNearbyWithCurMul.Keys.Contains(item))
                        {
                            thingNearbyWithCurMul.SetOrAdd(item, num);
                            curMul += num;
                        }
                        //numberOfSource++;
                    }
                }

            }
            numberOfSource = thingNearby.Count();
            if (numberOfSource <= 0)
            {
                if (HitPoints - 5 <= 0)
                {
                    if(!thingNearby.NullOrEmpty())
                    {
                        thingNearby.Clear();
                    }
                }
                TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 5));
            }
        }
        public void DoDamageToSource(Thing thing)
        {
            float num = 0f;
            FleckDef fleckDef = null;
            foreach (var item in thing.def.tradeTags)
            {
                num = Cultivation_Utility.getItemQiValueForPawn(item);
                fleckDef = Cultivation_Utility.getQiTypeFleck(item);
                if (num > 0f)
                {
                    break;
                }
            }
            num *= 2f;
            Cultivation_Utility.ThrowObjectAt(Map, thing.DrawPos, DrawPos, fleckDef ?? CTR_DefOf.CTR_AbsorbQiOrbPure, 0.25f + num, 0.25f + num);
            if (modExtension.destroyOnConsume)
            {
                thing?.Destroy();
            }
            else if (thing.def.useHitPoints && !thing.def.tradeTags.Contains("Unlimited_Source"))
            {                                
                if (thing.stackCount <= 1)
                {
                    thing.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, modExtension.sourceDamagePerTrigger));
                }
                else if (thing.stackCount > 1)
                {
                    if (thing.HitPoints - modExtension.sourceDamagePerTrigger >= 1)
                    {
                        thing.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, modExtension.sourceDamagePerTrigger));
                        if(thing.HitPoints <= thing.MaxHitPoints * 0.01f)
                        {
                            thing.stackCount--;
                            thing.HitPoints = thing.MaxHitPoints;
                        }
                    }
                    else
                    {
                        thing.stackCount--;
                        thing.HitPoints = thing.MaxHitPoints;
                    }

                }

            }
        }
        public void SpeedUpPlantGrow()
        {
            
            if(LifeStage != PlantLifeStage.Mature)
            {
                return;
            }
            DamageInfo dinfo = new DamageInfo(DamageDefOf.Deterioration, 1);            
            bool isNearbySimilarPlant = false;
            isSimilarPlantNearby = isNearbySimilarPlant;
            if (modExtension.onlyWorkIfNoPlantOfSameTypeInRange)
            {
                foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, false))
                {
                    if(item is not Plant_SpiritPlant)
                    {
                        continue;
                    }
                    if (item.def == def)
                    {
                        isNearbySimilarPlant = true;
                        isSimilarPlantNearby = true;
                        break;
                    }
                }
            }
            if (modExtension.onlyWorkIfNoPlantOfSameTypeInRange && isNearbySimilarPlant)
            {
                return;
            }
            if (modExtension.soundDefOnSpeedUp != null && CultivatorOfTheRimMod.settings.isPlayingTickingSound)
            {
                modExtension.soundDefOnSpeedUp.PlayOneShot(new TargetInfo(Position, Map));
            }
            if (Map == null)
            {
                return;
            }
            if (Position == null)
            {
                return;
            }
            foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, false))
            {
                if (item == this)
                {
                    continue;
                }
                if(item.def == def)
                {
                    continue;
                }
                /*if(!GenSight.LineOfSightToThing(Position,item,Map))
                {
                    continue;
                }*/
                if(modExtension.disallowedDef.Contains(item.def))
                {
                    continue;
                }
                if(modExtension.onlyAffectSpiritPlant && item is not Plant_SpiritPlant)
                {
                    continue;
                }    
                if(modExtension.onlyAffectCommonPlant && item is Plant_SpiritPlant)
                {
                    continue;
                }
                if (item is Plant plant)
                {
                    if (plant.Growth < 1f)
                    {
                        plant.Growth += modExtension.growthBoost.RandomInRange;
                        TakeDamage(dinfo);
                        SpawnQiOrb(item.DrawPos);
                    }
                }
                else if (item is Plant_SpiritPlant spiritPlant)
                {
                    if (spiritPlant.Growth < 1f)
                    {
                        spiritPlant.Growth += modExtension.growthBoost.RandomInRange;
                        TakeDamage(dinfo);
                        SpawnQiOrb(item.DrawPos);
                    }
                }
                else
                {
                    continue;
                }
            }

        }

        public void SpawnQiOrb(Vector3 target)
        {
            Vector3 originLoc = DrawPos + new Vector3(Rand.Range(-0.25f, 0.25f), 0.5f, Rand.Range(-0.25f, 0.25f));
            Vector3 targetLoc = target + new Vector3(Rand.Range(-0.5f, 0.5f), 0.5f, Rand.Range(-0.5f, 0.5f));
            //targetLoc.z += 4f;
            Map map = Map;
            Cultivation_Utility.ThrowObjectAt(map, originLoc, targetLoc, CTR_DefOf.CTR_AbsorbQiOrbPure, 0.5f, 1f);
        }
        /*public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if(modExtension.requireSource)
            {
                foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, true))
                {
                    if (thingNearby.Contains(item))
                    {
                        continue;
                    }
                    if (!modExtension.excludedThing.NullOrEmpty() && modExtension.excludedThing.Contains(item.def))
                    {
                        continue;
                    }                   
                    else if (!item.def.tradeTags.NullOrEmpty() && item.def != def)
                    {
                        if (item.def.tradeTags.Any(x => modExtension.allowedTags.Contains(x)))
                        {
                            numberOfSource++;
                            thingNearby.Add(item);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, true))
                {
                    if(thingNearbyWithCurMul.ContainsKey(item))
                    {
                        continue;
                    }
                    if (item == this || item.def == def)
                    {
                        continue;
                    }
                    if (!modExtension.excludedThing.NullOrEmpty() && modExtension.excludedThing.Contains(item.def))
                    {
                        continue;
                    }
                }
            }
            if(modExtension.isStopGrowingIfThingInRange)
            {
                foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.forbiddenRange, false))
                {
                    if (forbiddenThingNearby.Contains(item))
                    {
                        continue;
                    }
                    if (!modExtension.forbiddenTags.NullOrEmpty())
                    {
                        if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Any(x => modExtension.forbiddenTags.Contains(x)))
                        {
                            forbiddenThingNearby.Add(item);
                        }
                    }
                    if (!modExtension.forbiddenThings.NullOrEmpty())
                    {
                        if (modExtension.forbiddenThings.Contains(item.def))
                        {
                            forbiddenThingNearby.Add(item);
                        }
                    }
                    CompItemGrade itemGrade = item.TryGetComp<CompItemGrade>();
                    if (itemGrade != null && itemGrade.Grade != ItemGrade.Mortal)
                    {
                        string text = Cultivation_Utility.qiMultiplierFromGrade(itemGrade.Grade);
                        float num = 1f;
                        foreach (var t in Cultivation_Utility.qiSourceMultiplier)
                        {
                            if (t.Key == text)
                            {
                                num = t.Value;
                                break;
                            }
                        }
                        *//*if (num > curMul)
                        {
                            curMul = num;
                        }*//*                        
                        if(!thingNearby.Contains(item))
                        {
                            thingNearby.Add(item);
                        }
                        if(!thingNearbyWithCurMul.ContainsKey(item))
                        {
                            thingNearbyWithCurMul.Add(item, num);
                            curMul += num;
                        }                        
                        numberOfSource++;
                    }
                }
            }
            if(modExtension.requireSpecialSource)
            {
                foreach (var item in GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, false))
                {
                    if (specialThingNearby.Contains(item))
                    {
                        continue;
                    }
                    if (modExtension.allowedSpecialTags.NullOrEmpty())
                    {
                        continue;
                    }
                    if (!modExtension.forbiddenThings.NullOrEmpty() && modExtension.forbiddenThings.Contains(item.def))
                    {
                        continue;
                    }
                    if (!item.def.tradeTags.NullOrEmpty() && item.def.tradeTags.Any(x => modExtension.allowedSpecialTags.Contains(x)))
                    {
                        specialThingNearby.Add(item);
                    }
                }
            }
            
        }*/
        public override string GetInspectString()
        {            
            StringBuilder stringBuilder = new StringBuilder();
            if (def.plant.showGrowthInInspectPane)
            {
                if (LifeStage == PlantLifeStage.Growing)
                {
                    stringBuilder.AppendLine("PercentGrowth".Translate(GrowthPercentString));
                    stringBuilder.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
                    if (!Blighted)
                    {
                        if (Resting)
                        {
                            stringBuilder.AppendLine("PlantResting".Translate());
                        }

                        if (!HasEnoughLightToGrow)
                        {
                            stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + def.plant.growMinGlow.ToStringPercent());
                        }

                        if (GrowthRateFactor_Temperature < 0.99f && !modExtension.ignoreTemp)
                        {
                            if (Mathf.Approximately(GrowthRateFactor_Temperature, 0f) || !PlantUtility.GrowthSeasonNow(base.Position, base.Map))
                            {
                                stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
                            }
                            else
                            {
                                stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf.Max(1, Mathf.RoundToInt(GrowthRateFactor_Temperature * 100f)).ToString()));
                            }
                        }
                    }
                }
                else if (LifeStage == PlantLifeStage.Mature)
                {
                    if (HarvestableNow)
                    {
                        stringBuilder.AppendLine("ReadyToHarvest".Translate());
                    }
                    else
                    {
                        stringBuilder.AppendLine("Mature".Translate());
                    }
                }

                if (DyingBecauseExposedToLight)
                {
                    stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
                }

                if (Blighted)
                {
                    stringBuilder.AppendLine("Blighted".Translate() + " (" + Blight.Severity.ToStringPercent() + ")");
                }
            }

            string text = InspectStringPartsFromComps();
            if (!text.NullOrEmpty())
            {
                stringBuilder.Append(text);
            }
            if (modExtension.requireSource)
            {
                if (numberOfSource <= 0)
                {
                    stringBuilder.Append("not enough " + modExtension.sourceName);
                }
                else
                {
                    stringBuilder.Append(modExtension.sourceName + ": " + numberOfSource);
                }
            }
            if(modExtension.requireSpecialSource)
            {
                if (specialThingNearby.NullOrEmpty())
                {
                    stringBuilder.AppendInNewLine("not enough " + modExtension.specialSourceName);
                }
                else
                {
                    stringBuilder.AppendInNewLine(modExtension.specialSourceName + " is in range.");
                }
            }
            if (modExtension.speedUpNearbyPlant)
            {
                if (Growth >= 1f)
                {
                    if (modExtension.onlyWorkIfNoPlantOfSameTypeInRange)
                    {
                        if(isSimilarPlantNearby)
                        {
                            stringBuilder.AppendInNewLine("plant of same type in range. stop working.");
                        }
                    }
                }                
            }
            if(modExtension.endOfMonthHarvestable)
            {
                stringBuilder.AppendInNewLine("Day of Year: " + dayOfYear);
            }
            if(modExtension.isNightPlant)
            {
                if(Growth < 1f)
                {
                    if (isDayTime)
                    {
                        stringBuilder.AppendInNewLine("plant only grow at night");
                    }
                }
                if(Growth >= 1)
                {
                    if (isDayTime)
                    {
                        stringBuilder.AppendInNewLine("plant only harvestable at night");
                    }
                }
                
            }
            if(modExtension.isStopGrowingIfThingInRange)
            {
                if(!forbiddenThingNearby.NullOrEmpty())
                {
                    stringBuilder.AppendInNewLine("can't grow if forbidden thing are in range");
                    stringBuilder.AppendInNewLine("forbidden item: ");
                    for(int i = 0; i < forbiddenThingNearby.Count;i++) 
                    {
                        if(forbiddenThingNearby[i] != forbiddenThingNearby.Last())
                        {
                            stringBuilder.Append(forbiddenThingNearby[i].LabelCap + ",");
                        }                        
                        else
                        {
                            stringBuilder.Append(forbiddenThingNearby[i].LabelCap);
                        }
                    }
                }
            }
            if(modExtension.differentYieldBasedOnTimeOfHarvest && HarvestableNow)
            {
                if (isDayTime)
                {
                    stringBuilder.AppendInNewLine("Current Yield: " + modExtension.dayTimeYield.LabelCap);
                }    
                else
                {
                    stringBuilder.AppendInNewLine("Current Yield: " + modExtension.nightTimeYield.LabelCap);
                }
            }
            if(modExtension.differentEffectBasedOnTimeOfMonth)
            {
                stringBuilder.AppendInNewLine("Day of month multiplier: " + modExtension.growthRateCurves.Evaluate(dayOfMonth).ToStringPercent("0"));
            }
            if(modExtension.isAffectBySeason)
            {
                if(season == Season.PermanentWinter && modExtension.seasonCurves.Evaluate(5) == 0)
                {
                    stringBuilder.AppendInNewLine("PermanentSummerDisabledGrow".Translate());
                }
                if(season == Season.PermanentWinter && modExtension.seasonCurves.Evaluate(6) == 0)
                {
                    stringBuilder.AppendInNewLine("PermanentWinterDisabledGrow".Translate());
                }
                stringBuilder.AppendInNewLine("season multiplier: " + modExtension.seasonCurves.Evaluate((int)season).ToStringPercent("0"));
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            if(modExtension.requireSource)
            {                
                if(!thingNearby.NullOrEmpty())
                {
                    Scribe_Collections.Look(ref thingNearby, "thingNearby", LookMode.Reference);
                }
                if (!thingNearbyWithCurMul.NullOrEmpty())
                {
                    Scribe_Collections.Look(ref thingNearbyWithCurMul, "thingNearbyWithCurMul", LookMode.Reference, LookMode.Value);
                }                
                Scribe_Values.Look(ref numberOfSource, "numberOfSource", 0);                
            }            
            if(modExtension.requireSpecialSource)
            {
                Scribe_Collections.Look(ref specialThingNearby, "specialThingNearby", LookMode.Reference);
            }
            if(modExtension.isStopGrowingIfThingInRange)
            {
                Scribe_Collections.Look(ref forbiddenThingNearby, "forbiddenThingNearby", LookMode.Reference);
            }            
            if(modExtension.onlyWorkIfNoPlantOfSameTypeInRange)
            {
                Scribe_Values.Look(ref isSimilarPlantNearby, "isSimilarPlantNearby",false);
            }
            Scribe_Values.Look(ref onlyHarvestableAtNight, "onlyHarvestableAtNight");
            Scribe_Values.Look(ref onlyHarvestableAtDay, "onlyHarvestableAtDay");
            Scribe_Values.Look(ref endOfMonthHarvestable, "endOfMonthHarvestable");
            Scribe_Values.Look(ref middleOfMonthHarvestable, "middleOfMonthHarvestable");
            Scribe_Values.Look(ref specificMonthHarvestable, "specificMonthHarvestable");
        }
    }
}
