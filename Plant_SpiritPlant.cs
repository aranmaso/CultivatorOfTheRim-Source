using RimWorld;
using RimWorld.Planet;
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
                    return pawnsNearby.OrderBy(x => x.Value).First().Key;
                }
                return null;
            }
        }
        public bool isDayTime => GenLocalDate.DayPercent(Map) >= 0.25f && GenLocalDate.DayPercent(Map) <= 0.75f;
        public int dayOfMonth => GenLocalDate.DayOfQuadrum(Map);
        public int dayOfYear => GenLocalDate.DayOfYear(Map);

        private Season season => GenLocalDate.Season(Map);

        private Quadrum quadrum => GenDate.Quadrum(GenTicks.TicksAbs, Find.WorldGrid.LongLatOf(Map.Tile).x);

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
                if (base.Spawned && !PlantUtility.GrowthSeasonNow(Position,Map,def) && !modExtension.ignoreTemp)
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
                    if(CultivatorOfTheRimMod.settings.isUsingQuadrum)
                    {
                        switch(quadrum)
                        {
                            case Quadrum.Aprimay:
                                Season_Bonus = modExtension.seasonCurves.Evaluate(1);
                                break;
                            case Quadrum.Jugust:
                                Season_Bonus = modExtension.seasonCurves.Evaluate(2);
                                break;
                            case Quadrum.Septober:
                                Season_Bonus = modExtension.seasonCurves.Evaluate(3);
                                break;
                            case Quadrum.Decembary:
                                Season_Bonus = modExtension.seasonCurves.Evaluate(4);
                                break;
                            default:
                                Season_Bonus = 1f;
                                break;

                        }
                    }
                    else
                    {
                        switch (season)
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
            if (modExtension.requireSource)
            {
                RequireSource();
            }
            if (modExtension.requireSpecialSource)
            {
                RequireSpecialSource();
            }
            if (modExtension.speedUpNearbyPlant)
            {
                SpeedUpPlantGrow();
            }
            if (modExtension.isStopGrowingIfThingInRange)
            {
                PreventGrowthIfForbiddenInRange();
            }
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (!thingNearby.NullOrEmpty())
            {
                thingNearby.Clear();
            }
            if(!thingNearbyWithCurMul.NullOrEmpty())
            {
                thingNearbyWithCurMul.Clear();
            }
            if(!specialThingNearby.NullOrEmpty())
            {
                specialThingNearby.Clear();
            }
            if(!forbiddenThingNearby.NullOrEmpty())
            {
                forbiddenThingNearby.Clear();
            }
            if(!pawnsNearby.EnumerableNullOrEmpty())
            {
                pawnsNearby.Clear();
            }
        }
        protected override void Tick()
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
                    if(CultivatorOfTheRimMod.settings.isUsingQuadrum)
                    {
                        Season curSeason = Season.Undefined;
                        switch(quadrum)
                        {
                            case Quadrum.Aprimay:
                                curSeason = Season.Spring;
                                break;
                            case Quadrum.Jugust:
                                curSeason = Season.Summer;
                                break;
                            case Quadrum.Septober:
                                curSeason = Season.Fall;
                                break;
                            case Quadrum.Decembary:
                                curSeason = Season.Winter;
                                break;
                        }
                        if (!modExtension.specificMonthHarvestable.Contains(curSeason))
                        {
                            return false;
                        }

                    }
                    else
                    {
                        if (!modExtension.specificMonthHarvestable.Contains(season))
                        {
                            return false;
                        }
                    }                    
                    return base.HarvestableNow;
                }
                return base.HarvestableNow;
            }
        }
        public void PreventGrowthIfForbiddenInRange()
        {
            //clear thing out of range or null
            /*IReadOnlyList<Thing> list = new List<Thing>(forbiddenThingNearby);
            for (int i = 0; i < list.Count; i++)
            {
                if (forbiddenThingNearby[i].DestroyedOrNull() || forbiddenThingNearby[i].Position.DistanceToSquared(Position) > (modExtension.radius * modExtension.radius))
                {
                    forbiddenThingNearby.Remove(forbiddenThingNearby[i]);
                }
            }*/
            //update item
            if (Map == null)
            {
                return;
            }
            forbiddenThingNearby.Clear();
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
                try
                {
                    IReadOnlyList<Thing> list = new List<Thing>(specialThingNearby);
                    foreach (var item in list)
                    {
                        try
                        {
                            if (!specialThingNearby.Contains(item)) continue;
                            if (item.DestroyedOrNull() || item.Position.DistanceToSquared(Position) > (modExtension.radius * modExtension.radius))
                            {
                                specialThingNearby.Remove(item);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[CultivatorOfTheRim] Error ticking RequireSpecialSource for {LabelShort}. iterating item: {item}. {ex}");
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.Error($"[CultivatorOfTheRim] Error ticking RequireSpecialSource for {LabelShort}. {ex}");

                }
            }
            

            //update item
            if (Map == null)
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
                HitPoints += Rand.RangeInclusive(1,10);
                if (HitPoints > MaxHitPoints)
                {
                    HitPoints = MaxHitPoints;
                }
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
                IReadOnlyList<Thing> tempList = new List<Thing>(thingNearby);
                float num = modExtension.radius * modExtension.radius;
                for (var i = 0; i < tempList.Count; i++)
                {
                    var item = tempList[i];
                    try
                    {
                        if (item == null || !item.Spawned || item.Destroyed)
                        {
                            if (!thingNearbyWithCurMul.NullOrEmpty())
                            {
                                if (thingNearbyWithCurMul.Keys.Contains(item))
                                {
                                    curMul -= thingNearbyWithCurMul[item];
                                    thingNearbyWithCurMul.Remove(item);
                                }
                            }

                            thingNearby.Remove(item);
                        }
                        else if (item.Position.DistanceToSquared(Position) >
                                 (modExtension.radius * modExtension.radius))
                        {
                            if (thingNearbyWithCurMul.Keys.Contains(item))
                            {
                                if (thingNearbyWithCurMul.Keys.Contains(item))
                                {
                                    curMul -= thingNearbyWithCurMul[item];
                                    thingNearbyWithCurMul.Remove(item);
                                }
                            }

                            //Log.Message("remove from Nearby list(too far away)");
                            thingNearby.Remove(item);
                        }
                        else
                        {
                            if (modExtension.consumeSource)
                            {
                                if (item.DestroyedOrNull()) continue;
                                if (!item.def.tradeTags.NullOrEmpty() &&
                                    item.def.tradeTags.Contains("Unlimited_Source")) continue;
                                DoDamageToSource(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[CultivatorOfTheRim] Error checking for nearby thing{this}({PositionHeld}). {ex}");
                    }
                }
            }            
            if (!thingNearbyWithCurMul.NullOrEmpty())
            {
                IReadOnlyList<Thing> tempList = new List<Thing>(thingNearbyWithCurMul.Keys);
                for (var i = 0; i < tempList.Count; i++)
                {
                    var item = tempList[i];
                    if (item.DestroyedOrNull())
                    {
                        curMul -= thingNearbyWithCurMul[item];
                        thingNearbyWithCurMul.Remove(item);
                    }

                    float num2 = modExtension.radius * modExtension.radius;
                    if (item.Position.DistanceToSquared(Position) > (modExtension.radius * modExtension.radius))
                    {
                        curMul -= thingNearbyWithCurMul[item];
                        thingNearbyWithCurMul.Remove(item);
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

            List<Thing> tempNearby = [.. GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, true)];
            for (var i = 0; i < tempNearby.Count; i++)
            {
                var item = tempNearby[i];
                if(item.DestroyedOrNull()) continue;
                if (thingNearby.Contains(item))
                {
                    continue;
                }

                /*if(item == this || item.def == def)
                {
                    continue;
                }*/
                if (!modExtension.excludedThing.NullOrEmpty() && modExtension.excludedThing.Contains(item.def))
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
                else continue;
            }

            for (var i = 0; i < tempNearby.Count; i++)
            {
                var item = tempNearby[i];
                if(item.DestroyedOrNull()) continue;
                if (thingNearbyWithCurMul.ContainsKey(item))
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

                        if (!thingNearby.Contains(item))
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
                if(!Destroyed || this == null)
                {
                    HitPoints -= 5;
                    if (HitPoints <= 0)
                    {
                        Destroy();
                    }
                    //TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 5));
                }
                
            }
        }
        public void DoDamageToSource(Thing thing)
        {
            float num = 0f;
            FleckDef fleckDef = null;
            if (!thing.def.tradeTags.NullOrEmpty())
            {
                foreach (var item in thing.def.tradeTags)
                {
                    num = Cultivation_Utility.getItemQiValueForPawn(item);
                    fleckDef = Cultivation_Utility.getQiTypeFleck(item);
                    if (num > 0f)
                    {
                        break;
                    }
                }
            }
            else
            {
                num = 0.01f;
                fleckDef = CTR_DefOf.CTR_AbsorbQiOrbPure;
            }
            num *= 2f;
            Cultivation_Utility.ThrowObjectAt(Map, thing.DrawPos, DrawPos, fleckDef ?? CTR_DefOf.CTR_AbsorbQiOrbPure, 0.25f + num, 0.25f + num);
            if (!CultivatorOfTheRimMod.settings.isSpiritPlantDamageSource) return;

            if (modExtension.destroyOnConsume)
            {
                thing?.Destroy();
            }
            else if (thing.def.useHitPoints)
            {
                if (!thing.def.tradeTags.NullOrEmpty() && thing.def.tradeTags.Contains("Unlimited_Source"))
                {
                    return;
                }
                if (thing.stackCount <= 1)
                {
                    thing.HitPoints -= modExtension.sourceDamagePerTrigger;
                    if (thing.HitPoints <= 0) thing.Destroy();
                    //thing.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, modExtension.sourceDamagePerTrigger));
                }
                else if (thing.stackCount > 1)
                {
                    if (thing.HitPoints - modExtension.sourceDamagePerTrigger >= 1)
                    {
                        thing.HitPoints -= modExtension.sourceDamagePerTrigger;
                        //thing.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, modExtension.sourceDamagePerTrigger));
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
                IReadOnlyList<Thing> tempLists = GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, false).ToList();
                foreach (var item in tempLists)
                {
                    if (item.DestroyedOrNull()) continue;
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
            IReadOnlyList<Thing> tempThings = GenRadial.RadialDistinctThingsAround(Position, Map, modExtension.radius, false).ToList();
            foreach (var item in tempThings)
            {
                try
                {
                    if (item == this)
                    {
                        continue;
                    }
                    if (item.def == def)
                    {
                        continue;
                    }
                    if (item == null)
                    {
                        continue;
                    }
                    /*if(!GenSight.LineOfSightToThing(Position,item,Map))
                    {
                        continue;
                    }*/
                    if (modExtension.disallowedDef.Contains(item.def))
                    {
                        continue;
                    }
                    if (modExtension.onlyAffectSpiritPlant && item is not Plant_SpiritPlant)
                    {
                        continue;
                    }
                    if (modExtension.onlyAffectCommonPlant && item is Plant_SpiritPlant)
                    {
                        continue;
                    }
                    if (item is Plant plant)
                    {
                        if (plant.LifeStage != PlantLifeStage.Sowing && plant.Growth < 1f)
                        {
                            plant.Growth += modExtension.growthBoost.RandomInRange;
                            HitPoints--;
                            //TakeDamage(dinfo);
                            SpawnQiOrb(item.DrawPos);
                            if (HitPoints <= 0)
                            {
                                break;
                            }
                        }
                    }
                    else if (item is Plant_SpiritPlant spiritPlant)
                    {
                        if (spiritPlant.LifeStage != PlantLifeStage.Sowing && spiritPlant.Growth < 1f)
                        {
                            spiritPlant.Growth += modExtension.growthBoost.RandomInRange;
                            HitPoints--;
                            //TakeDamage(dinfo);
                            SpawnQiOrb(item.DrawPos);
                            if (HitPoints <= 0)
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    Log.Error($"[CultivatorOfTheRim]. {this} failed to speed up growth for {item}. {ex}");
                }
            }
            if (HitPoints <= 0)
            {
                Destroy();
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
            if (modExtension.requireSource)
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
                    if (thingNearbyWithCurMul.ContainsKey(item))
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
            if (modExtension.isStopGrowingIfThingInRange)
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
                        if (num > curMul)
                        {
                            curMul = num;
                        }
                        if (!thingNearby.Contains(item))
                        {
                            thingNearby.Add(item);
                        }
                        if (!thingNearbyWithCurMul.ContainsKey(item))
                        {
                            thingNearbyWithCurMul.Add(item, num);
                            curMul += num;
                        }
                        numberOfSource++;
                    }
                }
            }
            if (modExtension.requireSpecialSource)
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
                            if (Mathf.Approximately(GrowthRateFactor_Temperature, 0f) || !PlantUtility.GrowthSeasonNow(Position, Map, def))
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
                if(CultivatorOfTheRimMod.settings.isUsingQuadrum)
                {
                    switch (quadrum)
                    {
                        case Quadrum.Aprimay:
                            stringBuilder.AppendInNewLine("season multiplier: " + modExtension.seasonCurves.Evaluate(1).ToStringPercent("0"));
                            break;
                        case Quadrum.Jugust:
                            stringBuilder.AppendInNewLine("season multiplier: " + modExtension.seasonCurves.Evaluate(2).ToStringPercent("0"));
                            break;
                        case Quadrum.Septober:
                            stringBuilder.AppendInNewLine("season multiplier: " + modExtension.seasonCurves.Evaluate(3).ToStringPercent("0"));
                            break;
                        case Quadrum.Decembary:
                            stringBuilder.AppendInNewLine("season multiplier: " + modExtension.seasonCurves.Evaluate(4).ToStringPercent("0"));
                            break;
                        default:
                            stringBuilder.AppendInNewLine("Undefined Quadrum");
                            break;

                    }
                }
                else
                {
                    if (season == Season.PermanentWinter && modExtension.seasonCurves.Evaluate(5) == 0)
                    {
                        stringBuilder.AppendInNewLine("PermanentSummerDisabledGrow".Translate());
                    }
                    if (season == Season.PermanentWinter && modExtension.seasonCurves.Evaluate(6) == 0)
                    {
                        stringBuilder.AppendInNewLine("PermanentWinterDisabledGrow".Translate());
                    }
                    stringBuilder.AppendInNewLine("season multiplier: " + modExtension.seasonCurves.Evaluate((int)season).ToStringPercent("0"));
                }
                
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
