using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultivatorOfTheRim
{
    public class CompFormation : ThingComp
    {
        public CompProperties_Formation Props => (CompProperties_Formation)props;

        public CompGlower glowerComp;
        public CompGlower Glower => glowerComp ?? (glowerComp = parent.TryGetComp<CompGlower>()); 

        public int durationLeft = 0;

        public int activeDelay;

        public int activationDelay;

        public bool isFormationActive = false;

        public override void PostPostMake()
        {
            base.PostPostMake();
            activeDelay = Props.activeDelay;
        }

        public Dictionary<string,int> baseValuePerTier = new Dictionary<string, int>()
        {
            {"Qi_Source_Tier1" , 25 },
            {"Qi_Source_Tier2" , 500 },
            {"Qi_Source_Tier3" , 1000 },
            {"Qi_Source_Tier4" , 1500 },
            {"Qi_Source_Tier5" , 5000 },
            {"Qi_Source_Tier6" , 20000 },

        };
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref durationLeft, "durationLeft",0);
            Scribe_Values.Look(ref activeDelay, "activeDelay", 0);
            Scribe_Values.Look(ref activationDelay, "activationDelay", 0);
            Scribe_Values.Look(ref isFormationActive, "isFormationActive", false);
        }
        public override void CompTick()
        {
            if(activationDelay > 0)
            {
                activationDelay--;
                if(activationDelay <= 0)
                {
                    if (durationLeft <= 0)
                    {
                        isFormationActive = false;
                    }                    
                }
                return;
            }
            else
            {
                if(isFormationActive)
                {
                    if (durationLeft > 0)
                    {
                        durationLeft--;
                    }
                    else
                    {
                        isFormationActive = false;
                    }
                }                
            }
            int num = (isFormationActive ? 1 : 0);
            if (parent.overrideGraphicIndex != num)
            {
                parent.overrideGraphicIndex = num;
                parent.DirtyMapMesh(parent.Map);
                Glower?.UpdateLit(parent.Map);
                Glower.glowOnInt = isFormationActive;
            }
            if (isFormationActive && durationLeft > 0)
            {                
                if(parent.IsHashIntervalTick(Props.checkInterval))
                {
                    if(Props.heddiffDef != null)
                    {
                        GiveHediff();
                    }
                    if(Props.isShootingProjectile)
                    {
                        ShootProjectile();
                    }
                    if(Props.isSpeedUpPlant)
                    {
                        SpeedUpPlant();
                    }
                }
            }
            
        }
        public void SpeedUpPlant()
        {
            if (Props.isPlayingEffector)
            {
                Effecter effecter = Props.effecterDef.Spawn(parent.Position, parent.Map);
                effecter.Cleanup();
            }
            foreach (Thing item in GenRadial.RadialDistinctThingsAround(parent.Position,parent.Map,Props.radius,true))
            {
                if(!CultivatorOfTheRimMod.settings.isFertilityFormationAffectSpiritPlant && item is Plant_SpiritPlant)
                {
                    continue;
                }
                if(item is Plant plant)
                {
                    if (plant.Growth < 1f)
                    {
                        plant.Growth += Props.speedUpPercent.RandomInRange;
                    }
                }
                else
                {
                    continue;
                }
            }
            if(CultivatorOfTheRimMod.settings.isFertilityFormationAgePawn)
            {
                foreach (var item in Cultivation_Utility.GetNearbyPawnFriendAndFoe(parent.Position, parent.Map, Props.radius))
                {
                    if (Rand.Value <= 0.1f)
                    {
                        item.ageTracker.AgeBiologicalTicks += Mathf.RoundToInt(Mathf.FloorToInt(60000) * Find.Storyteller.difficulty.adultAgingRate);
                    }
                }
            }            
        }

        public void ShootProjectile()
        {
            if(Props.isShootingProjectile && Props.projectile == null)
            {
                return;
            }
            bool soundPlayed = false;
            if(Props.isShootingProjectile)
            {
                int num = 0;
                foreach (var item in Cultivation_Utility.GetNearbyPawnFriendAndFoeNeedSight(parent.Position, parent.Map, Props.radius, true))
                {
                    if (item.Downed || item.Crawling)
                    {
                        continue;
                    }
                    if(!Cultivation_Utility.isFriendly(item,parent))
                    {
                        Projectile projectile = (Projectile)GenSpawn.Spawn(Props.projectile, parent.Position, parent.Map);
                        IntVec3 origin = parent.Position;
                        if (Props.randomSpawnPosition)
                        {
                            origin = GenRadial.RadialCellsAround(parent.Position, 4f, true).RandomElement();
                        }                        
                        projectile.Launch(parent, origin.ToVector3Shifted(), item, item, ProjectileHitFlags.IntendedTarget);
                        if (Props.shootingSound != null && !soundPlayed)
                        {
                            Props.shootingSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
                            soundPlayed = true;
                        }
                        num++;
                    }
                    else
                    {
                        continue;
                    }
                    if (num >= Props.targetLimit)
                    {
                        break;
                    }
                };
            }
        }
        public void GiveHediff()
        {
            foreach(var item in Cultivation_Utility.GetNearbyPawnFriendAndFoeNeedSight(parent.Position,parent.Map,Props.radius,false))
            {
                if(item.health.hediffSet.HasHediff(Props.heddiffDef))
                {
                    continue;
                }
                if(Props.isOnlyTargetHostile && Cultivation_Utility.isFriendly(item,parent))
                {
                    continue;
                }
                if(Props.isOnlyTargetFriendly && !Cultivation_Utility.isFriendly(item,parent))
                {
                    continue;
                }
                Hediff hediff = HediffMaker.MakeHediff(Props.heddiffDef,item);
                if (hediff.TryGetComp<HediffComp_Disappears>() != null)
                {
                    hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(Props.hediffDuration);
                }
                item.health.AddHediff(hediff);
                if(Props.isPlayingEffector)
                {
                    Effecter effecter = Props.effecterDef.Spawn(item.Position, parent.Map);
                    effecter.Cleanup();
                    SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(item.Position, parent.Map));
                }
                
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if(activationDelay > 0)
            {
                stringBuilder.AppendLine("Activating");
                stringBuilder.AppendLine("Activation time: " + activationDelay);
            }
            if (isFormationActive && activationDelay <= 0)
            {
                stringBuilder.AppendLine("Enabled");
            }
            else
            {
                stringBuilder.AppendLine("Disabled");
            }
            if(durationLeft < 2500)
            {
                stringBuilder.AppendLine("duration left: " + durationLeft.ToStringSecondsFromTicks() + " / " + Props.totalDuration.ToStringTicksToPeriod(true, true, true, true));
            }
            else
            {
                stringBuilder.AppendLine("duration left: " + durationLeft.ToStringTicksToPeriod(true, true, true, true) + " / " + Props.totalDuration.ToStringTicksToPeriod(true, true, true, true));
            }            
            if(durationLeft > Props.totalDuration)
            {
                stringBuilder.AppendLine("OVERLOADED!");
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(!isFormationActive)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Absorb Spirit Stone",
                    defaultDesc = "Absorb Qi from spirit stone",
                    icon = ContentFinder<Texture2D>.Get(Props.uiIcon) ?? Widgets.GetIconFor(parent.def),
                    hotKey = KeyBindingDefOf.Command_ItemForbid,
                    action = delegate
                    {
                        Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate (LocalTargetInfo t)
                        {
                            ThingDef thingDef = t.Thing.def;
                            if (IsValid(thingDef) && t.Thing.stackCount >= Props.spiritStoneCost)
                            {
                                string tier = Cultivation_Utility.getItemQiTier(t.Thing);
                                int num = baseValuePerTier[tier];

                                int totalAvailableInStack = t.Thing.stackCount * num;
                                if(durationLeft < Props.totalDuration)
                                {
                                    if (Props.totalDuration > totalAvailableInStack)
                                    {
                                        durationLeft += totalAvailableInStack;
                                        t.Thing.Destroy();

                                    }
                                    else
                                    {
                                        int num2 = Props.totalDuration / num;
                                        if ((t.Thing.stackCount - num2) <= 0)
                                        {
                                            t.Thing.Destroy();
                                        }
                                        else
                                        {
                                            t.Thing.stackCount -= num2;
                                        }
                                        durationLeft += num2 * num;
                                    }
                                }
                                else
                                {
                                    if((t.Thing.stackCount - Props.spiritStoneCost) <= 0)
                                    {
                                        durationLeft += num * Props.spiritStoneCost;
                                        t.Thing.Destroy();
                                    }
                                    else
                                    {
                                        durationLeft += num * Props.spiritStoneCost;
                                        t.Thing.stackCount -= Props.spiritStoneCost;
                                    }
                                }
                                                         

                                Effecter effecter = ModsConfig.RoyaltyActive ? EffecterDefOf.Skip_Entry.Spawn(t.Cell, parent.Map) : EffecterDefOf.ExtinguisherExplosion.Spawn(t.Cell, parent.Map);
                                effecter.Cleanup();
                                SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.Map));
                            }
                            else
                            {
                                Messages.Message("not enough spirit stone", MessageTypeDefOf.NeutralEvent);
                            }
                        });
                    }
                };
            }
            

            Command_Action formationToggle = new Command_Action();
            if (isFormationActive)
            {
                formationToggle.defaultLabel = "Disable Formation";
                formationToggle.defaultDesc = "formation is currently running";
            }
            else
            {
                formationToggle.defaultLabel = "Start Formation";
                formationToggle.defaultDesc = "formation is currently deactivated";
            }
            formationToggle.icon = ContentFinder<Texture2D>.Get(Props.uiIcon) ?? Widgets.GetIconFor(parent.def);
            formationToggle.hotKey = KeyBindingDefOf.Designator_RotateRight;
            formationToggle.action = delegate
            {
                if(activationDelay <= 0)
                {
                    isFormationActive = !isFormationActive;
                    if (isFormationActive)
                    {
                        activationDelay = Props.activeDelay;
                    }
                    else if (!isFormationActive)
                    {

                    }
                }
                else
                {
                    Messages.Message("Formation currently processing",MessageTypeDefOf.NeutralEvent);
                }
                
            };
            yield return formationToggle;

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Fill Duration",
                    defaultDesc = "fill duration to full",
                    action = delegate
                    {
                        durationLeft = Props.totalDuration;
                    }
                };
            }
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
        private bool IsValid(ThingDef thingDef)
        {
            if (thingDef.tradeTags.NullOrEmpty())
            {
                return false;
            }
            if(!thingDef.tradeTags.Contains("SpiritStone"))
            {
                return false;
            }
            return true;
        }
    }
}
