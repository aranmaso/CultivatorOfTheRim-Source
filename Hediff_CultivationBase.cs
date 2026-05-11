using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace CultivatorOfTheRim
{
    public class Hediff_CultivationBase : HediffWithComps
    {
        public CultivationHediffDef cultivationDef
        {
            get
            {
                if (def is CultivationHediffDef cDef)
                {
                    return cDef;
                }
                return null;
            }
        }
        public CultivationHediffStage CurCulStage
        {
            get
            {
                if (!cultivationDef.cultivationStages.NullOrEmpty())
                {
                    return cultivationDef.cultivationStages[CurCulStageIndex];
                }

                return null;
            }
        }
        public int CurCulStageIndex
        {
            get
            {
                if (cultivationDef != null)
                {
                    return cultivationDef.CultivationStageAtSeverity(Severity);
                }
                return -1;
            }
        }
        public override float Severity
        {
            get
            {
                return severityInt;
            }
            set
            {
                bool flag = false;
                if (IsLethal && value >= def.lethalSeverity)
                {
                    value = def.lethalSeverity;
                    flag = true;
                }

                int curStageIndex = CurStageIndex;
                int curCulStageIndex = CurCulStageIndex;

                severityInt = Mathf.Clamp(value, def.minSeverity, def.maxSeverity);

                if (CurStageIndex != curStageIndex)
                {
                    OnStageIndexChanged(CurStageIndex);
                }
                if (CurCulStageIndex != curCulStageIndex)
                {
                    OnCulStageIndexChanged(CurCulStageIndex);
                }

                if ((CurStageIndex != curStageIndex || flag) && pawn.health.hediffSet.hediffs.Contains(this))
                {
                    pawn.health.Notify_HediffChanged(this);
                    if (!pawn.Dead && pawn.needs.mood != null)
                    {
                        pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                    }
                }
            }
        }
        
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry item in cultivationDef.SpecialCulDisplayStats(req,CurStageIndex))
            {
                yield return item;
            }
            int num = 2000;
            if (cultivationDef.bonusHediff.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, $"<b>Base Bonus Chance</b>".Colorize(Color.green), "", "hediff that have a chance to be gained when reaching this realm.", num);
                num--;
                foreach (var entry in cultivationDef.bonusHediff)
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.hediffDef.LabelCap, entry.chance.ToStringPercent("0"), entry.hediffDef.description, num);
                    num--;
                }
            }
            if (cultivationDef.cultivationStages.NotNullOrEmpty())
            {
                foreach (var CulStage in cultivationDef.cultivationStages)
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, $"<b>Cultivation Stage: {CulStage.label}</b>".Colorize(Color.green), "", "bonus gained this stage", num);
                    num--;
                    if (CulStage.empty)
                    {
                        yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, $"-", "", "-", num);
                        num--;
                    }
                    foreach (StatDrawEntry item in cultivationDef.SpecialCulStat(CulStage, num))
                    {
                        yield return item;
                    }
                }
            }
        }

        public List<Thing> attachedThing = new List<Thing>();

        public List<HediffDef> hediffDefsAdded = new List<HediffDef>();

        public List<AbilityDef> abilitiesAdded = new List<AbilityDef>();

        public List<GeneDef> genesAdded = new List<GeneDef>();

        public Dictionary<TraitDef,int?> traitsAdded = new Dictionary<TraitDef, int?>();
        
        public float HealthScaleMul = 1f;

        public CultivationHediffDef nextRealm
        {
            get
            {
                return cultivationDef.nextLevel != null ? cultivationDef.nextLevel : cultivationDef.nextLevels.RandomElementByWeight(x => x.weight).nextRealm;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref attachedThing, "attachedThing",LookMode.Reference);
            Scribe_Collections.Look(ref hediffDefsAdded, "hediffDefsAdded",LookMode.Def);
            Scribe_Collections.Look(ref abilitiesAdded, "abilitiesAdded", LookMode.Def);
            Scribe_Collections.Look(ref genesAdded, "genesAdded", LookMode.Def);
            Scribe_Collections.Look(ref traitsAdded, "traitsAdded", LookMode.Def,LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                UpdateHealthScaleCache();
            }
        }
        
        public override void TickInterval(int delta)
        {
            base.TickInterval(delta);
            if (pawn.IsHashIntervalTick(250,delta))
            {
                UpdateHealthScaleCache();
            }
        }

        public void UpdateHealthScaleCache()
        {
            HealthScaleMul = pawn.GetStatValue(CTR_DefOf.CTR_HealthMultiplier);
            StaticCollectionCached.UpdateHealthScale(pawn, HealthScaleMul);
        }

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
            UpdateHealthScaleCache();
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (!pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughCounter))
            {
                Hediff hediff2 = Cultivation_Utility.CreateHediffNoDuration(pawn, CTR_DefOf.CTR_BreakthroughCounter);
                pawn.health.AddHediff(hediff2);
            }
            if (cultivationDef.bonusHediff.NotNullOrEmpty())
            {
                foreach (var item in cultivationDef.bonusHediff)
                {
                    if (Rand.Chance(item.chance))
                    {
                        Hediff hediff = pawn.health.GetOrAddHediff(item.hediffDef);
                        if (item.onlyOne)
                        {
                            break;
                        }
                    }
                }
            }
            if (cultivationDef.cultivationStages.NotNullOrEmpty())
            {
                if (Severity >= cultivationDef.cultivationStages[0].minSeverity)
                {
                    OnCulStageIndexChanged(0);
                }
            }
            UpdateHealthScaleCache();
        }
        
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (attachedThing.NotNullOrEmpty())
            {
                foreach (var item in attachedThing.ToList())
                {
                    CompAttachBase comp = pawn.TryGetComp<CompAttachBase>();
                    if (comp != null)
                    {
                        if (comp.HasAttachment(item.def))
                        {
                            comp.RemoveAttachment((AttachableThing)item);
                            item.Destroy();
                        }
                    }
                }
            }
            if (hediffDefsAdded.NotNullOrEmpty())
            {
                foreach (var item in hediffDefsAdded)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(item);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
            if (abilitiesAdded.NotNullOrEmpty())
            {
                foreach (var item in abilitiesAdded)
                {
                    pawn.abilities.RemoveAbility(item);
                }
            }
            if (genesAdded.NotNullOrEmpty())
            {
                foreach (var item in genesAdded)
                {
                    if (pawn.genes.HasActiveGene(item))
                    {
                        pawn.genes.RemoveGene(pawn.genes.GetGene(item));
                    }
                }
            }
            if (traitsAdded.NotNullOrEmpty())
            {
                foreach (var item in traitsAdded)
                {
                    if (item.Value.HasValue)
                    {
                        if (pawn.story.traits.HasTrait(item.Key, item.Value.Value))
                        {
                            pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(item.Key,item.Value.Value));
                        }
                    }
                    else
                    {
                        if (pawn.story.traits.HasTrait(item.Key))
                        {
                            pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(item.Key));
                        }
                    }
                }
            }

            if (!pawn.HaveAnyCultivation())
            {
                StaticCollectionCached.PawnHealthScaleCached.Remove(pawn);
            }
            else
            {
                UpdateHealthScaleCache();   
            }
            /*if (instancedDynamicComps != null)
            {
                if (pawn.Map != null)
                {
                    pawn.Map.listerThings.Remove(pawn);
                    ThingRequestGroup[] allGroups = ThingListGroupHelper.AllGroups;
                    for (int i = 0; i < allGroups.Length; i++)
                    {
                        ThingRequestGroup thingRequestGroup = allGroups[i];
                        if ((pawn.Map.listerThings.use != ListerThingsUse.Region || thingRequestGroup.StoreInRegion()) && pawn.Map.listerThings.GroupIncludes(pawn, thingRequestGroup))
                        {
                            pawn.Map.listerThings.listsByGroup[i].Remove(pawn);
                            pawn.Map.listerThings.stateHashByGroup[(uint)thingRequestGroup]++;
                        }
                    }
                }
                foreach (var item in instancedDynamicComps)
                {
                    ThingComp comp = pawn.comps.FirstOrDefault(x => x.props.GetType().Name == item.Key);
                    if (comp != null)
                    {
                        pawn.comps.Remove(comp);
                    }
                }
                instancedDynamicComps.Clear();
                RebuildCompsByTypeCache();
                if (pawn.Map != null)
                {
                    pawn.Map.listerThings.Add(pawn);
                }
            }
            pawn.Drawer.renderer.SetAllGraphicsDirty();*/
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Command_Action breakthrough = new Command_Action();
            breakthrough.defaultLabel = CultivatorOfTheRimMod.settings.isBreakthroughCanFailForHumanlike ? cultivationDef.breakthroughKeyed.Translate(Cultivation_Utility.GetBreakthroughChance(pawn, false).ToStringPercent("0")) : cultivationDef.breakthroughKeyed.Translate(1f.ToStringPercent("0"));
            if (cultivationDef.canGetTribulation)
            {
                breakthrough.defaultDesc = cultivationDef.breakthroughDescKeyed.Translate() + "\n" + "Tribulation Chance: " + pawn.GetStatValue(CTR_DefOf.TribulationChance, cacheStaleAfterTicks: 250).ToStringPercent();
            }
            else
            {
                breakthrough.defaultDesc = cultivationDef.breakthroughDescKeyed.Translate();
            }
            breakthrough.icon = cultivationDef.breakthroughUiIconTexture;
            breakthrough.action = delegate
            {
                CultivationHediffDef nextLevel = cultivationDef.nextLevel != null ? cultivationDef.nextLevel : cultivationDef.nextLevels.RandomElementByWeight(x => x.weight).nextRealm;
                if (nextLevel == null || nextLevel == cultivationDef)
                {
                    //Messages.Message("AlreadyLearned".Translate(reader.LabelShort, Props.benefitString, reader.Named("USER")), reader, MessageTypeDefOf.PositiveEvent);
                    Messages.Message("CTR_BaseCultivationBreakthrough_Maxed".Translate(pawn.LabelShort, Label), MessageTypeDefOf.NeutralEvent, false);
                }
                else if (Severity < def.maxSeverity)
                {
                    Messages.Message("CTR_BaseCultivationBreakthrough_RealmNotMaxed".Translate(pawn.LabelShort), MessageTypeDefOf.NeutralEvent, false);
                }
                else
                {
                    if (Severity >= def.maxSeverity && !pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess) && (nextLevel != null && def != nextLevel))
                    {
                        Cultivation_Advance(nextLevel.canGetTribulation, cultivationDef, nextLevel, cultivationDef.breakthroughDuration.RandomInRange, cultivationDef.tribulationStrikeInterval);
                        Job job = JobMaker.MakeJob(CTR_DefOf.CTR_BreakingThrough, pawn);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job);
                    }
                    else if (pawn.health.hediffSet.HasHediff(CTR_DefOf.CTR_BreakthroughProcess))
                    {
                        Messages.Message("CTR_BreakthroughInProcess".Translate(pawn.LabelShort), MessageTypeDefOf.NeutralEvent);
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
                        float curStage = CurStage.minSeverity;
                        for (int i = 0; i < def.stages.Count; i++)
                        {
                            if (def.stages[i].minSeverity > curStage)
                            {
                                Severity = def.stages[i].minSeverity;
                                if (i == def.stages.Count)
                                {
                                    Severity = def.maxSeverity;
                                }
                                break;
                            }
                        }
                    }
                };
            }
            foreach (var item in base.GetGizmos())
            {
                yield return item;
            }
        }
        public virtual void OnCulStageIndexChanged(int stageIndex)
        {
            if (CurCulStage == null)
            {
                return;
            }
            if (CurCulStage.hediffsGained.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.hediffsGained)
                {
                    Hediff hediff = pawn.health.GetOrAddHediff(item);
                    if (CurCulStage.isRemoveOnHediffLost)
                    {
                        hediffDefsAdded.Add(item);
                    }
                    Messages.Message($"{pawn.LabelShort} gained {hediff.Label}", pawn, MessageTypeDefOf.NeutralEvent, false);
                }
            }
            if (CurCulStage.hediffsRemoved.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.hediffsRemoved)
                {
                    Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(item);
                    if (hediff != null)
                    {
                        pawn.health.RemoveHediff(hediff);
                        if (CurCulStage.isRemoveOnHediffLost)
                        {
                            hediffDefsAdded.Remove(item);
                        }
                        Messages.Message($"{pawn.LabelShort} lose {hediff.Label}", pawn, MessageTypeDefOf.NeutralEvent, false);
                    }
                }
            }
            if (CurCulStage.hediffChance.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.hediffChance)
                {
                    if (Rand.Chance(item.chance))
                    {
                        Hediff hediff = pawn.health.GetOrAddHediff(item.hediffDef);
                        if (CurCulStage.isRemoveOnHediffLost)
                        {
                            hediffDefsAdded.Add(item.hediffDef);
                        }
                        if (item.onlyOne)
                        {
                            break;
                        }
                    }
                }
            }
            if (CurCulStage.abilities.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.abilities)
                {
                    pawn.abilities.GainAbility(item);
                    if (CurCulStage.isRemoveOnHediffLost)
                    {
                        abilitiesAdded.Add(item);
                    }
                    Messages.Message($"{pawn.LabelShort} gained {item.LabelCap}", pawn, MessageTypeDefOf.NeutralEvent, false);
                }
            }
            if (CurCulStage.removeAbilities.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.removeAbilities)
                {
                    pawn.abilities.RemoveAbility(item);
                    if (CurCulStage.isRemoveOnHediffLost)
                    {
                        abilitiesAdded.Remove(item);
                    }
                    Messages.Message($"{pawn.LabelShort} lose {item.LabelCap}", pawn, MessageTypeDefOf.NeutralEvent, false);
                }
            }
            if (CurCulStage.geneDefs.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.geneDefs)
                {
                    if (!pawn.genes.HasActiveGene(item))
                    {
                        pawn.genes.AddGene(item, CurCulStage.isXenogene);
                        if (CurCulStage.isRemoveOnHediffLost)
                        {
                            genesAdded.Add(item);
                        }
                        Messages.Message($"{pawn.LabelShort} gained {item.LabelCap}", pawn, MessageTypeDefOf.NeutralEvent, false);
                    }
                }
            }
            if (CurCulStage.geneDefsRemove.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.geneDefsRemove)
                {
                    Gene gene = pawn.genes.GetGene(item);
                    if (gene != null)
                    {
                        pawn.genes.RemoveGene(gene);
                        if (CurCulStage.isRemoveOnHediffLost)
                        {
                            genesAdded.Remove(item);
                        }
                        Messages.Message($"{pawn.LabelShort} lose {item.LabelCap}", pawn, MessageTypeDefOf.NeutralEvent, false);
                    }
                }
            }
            if (CurCulStage.traitDefs.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.traitDefs)
                {
                    pawn.story.traits.GainTrait(new Trait(item.def, item.degree.GetValueOrDefault(), true));
                    if (CurCulStage.isRemoveOnHediffLost)
                    {
                        traitsAdded.Add(item.def,item.degree.GetValueOrDefault());
                    }
                }
            }
            if (CurCulStage.removeTraitDefs.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.removeTraitDefs)
                {
                    if (item.degree.HasValue)
                    {
                        if (pawn.story.traits.HasTrait(item.def, item.degree.Value))
                        {
                            pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(item.def, item.degree.Value));
                            if (CurCulStage.isRemoveOnHediffLost)
                            {
                                traitsAdded.Remove(item.def);
                            }
                        }
                    }
                    else
                    {
                        pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(item.def));
                        if (CurCulStage.isRemoveOnHediffLost)
                        {
                            traitsAdded.Remove(item.def);
                        }
                    }
                }
            }
            if (CurCulStage.attachableThing.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.attachableThing)
                {
                    Cultivation_AttachableThing obj = (Cultivation_AttachableThing)ThingMaker.MakeThing(item);
                    obj.inspectString += $"{item},";
                    obj.AttachTo(pawn);
                    GenSpawn.Spawn(obj,pawn.PositionHeld,pawn.MapHeld,Rot4.North);
                    attachedThing.Add(obj);
                }
            }
            if (CurCulStage.removeAttachedThing.NotNullOrEmpty())
            {
                foreach (var item in CurCulStage.removeAttachedThing)
                {
                    CompAttachBase comp = pawn.TryGetComp<CompAttachBase>();
                    if (comp != null)
                    {
                        if (comp.HasAttachment(item))
                        {
                            Thing thing = comp.GetAttachment(item);
                            comp.RemoveAttachment((AttachableThing)thing);
                            thing.Destroy();
                        }
                    }
                }
            }
            if (CurCulStage.sendLetter)
            {
                string label = CurCulStage.letterLabel;
                string desc = CurCulStage.letterDesc;
                Find.LetterStack.ReceiveLetter(label,desc,LetterDefOf.NeutralEvent,pawn);
            }
            /*if (CurCulStage.comps.NotNullOrEmpty())
            {
                if (pawn.Map != null)
                {
                    pawn.Map.listerThings.Remove(pawn);
                }
                for (int i = 0; i < CurCulStage.comps.Count; i++)
                {
                    var compProperties = CurCulStage.comps[i];
                    ThingComp thingComp = (ThingComp)Activator.CreateInstance(compProperties.compClass);
                    thingComp.parent = pawn;
                    thingComp.Initialize(compProperties);
                    thingComp.PostPostMake();
                    pawn.comps.Add(thingComp);
                    instancedDynamicComps.Add(CurCulStage.comps[i].GetType().Name,CurCulStageIndex);
                }
                RebuildCompsByTypeCache();
                if (pawn.Map != null)
                {
                    pawn.Map.listerThings.Add(pawn);
                }
            } */    
            /*if (CurCulStage.compsRemove.NotNullOrEmpty())
            {
                Log.Message($"Enter");
                if (pawn.Map != null)
                {
                    pawn.Map.listerThings.Remove(pawn);
                    ThingRequestGroup[] allGroups = ThingListGroupHelper.AllGroups;
                    for (int i = 0; i < allGroups.Length; i++)
                    {
                        ThingRequestGroup thingRequestGroup = allGroups[i];
                        if ((pawn.Map.listerThings.use != ListerThingsUse.Region || thingRequestGroup.StoreInRegion()) && pawn.Map.listerThings.GroupIncludes(pawn, thingRequestGroup))
                        {
                            pawn.Map.listerThings.listsByGroup[i].Remove(pawn);
                            pawn.Map.listerThings.stateHashByGroup[(uint)thingRequestGroup]++;
                        }
                    }
                }
                Log.Message($"remove {pawn} temporarily from listThing");
                foreach (var comp in pawn.comps)
                {
                    Log.Message($"{comp}. {comp.props}");
                }
                Log.Message($"=======================");
                foreach (var item in pawn.comps.ToList())
                {
                    Log.Message($"Currrently filtering for {item}");
                    if (CurCulStage.compsRemove.FirstOrDefault(x => x.GetType().Name == item.props.GetType().Name) != null)
                    {
                        Log.Message($"Found {item} props.");
                        if (instancedDynamicComps.ContainsKey(item.GetType().Name))
                        {
                            instancedDynamicComps.Remove(item.GetType().Name);
                        }
                        pawn.comps.Remove(item);
                    }
                }
                RebuildCompsByTypeCache();
                if (pawn.Map != null)
                {
                    pawn.Map.listerThings.Add(pawn);
                    Log.Message($"added {pawn} back to listThing");
                }
            }*/
        }

        public virtual void Cultivation_Advance(bool canGetTrib, CultivationHediffDef curHediff, CultivationHediffDef nextHediff, int duration, int strikeInterval = 250)
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
