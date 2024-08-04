using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using static HarmonyLib.Code;

namespace CultivatorOfTheRim
{
    public static class Cultivation_Utility
    {

        public static TribulationInfo RandomizeTribulation()
        {
            TribulationInfo tinfo = new TribulationInfo();
            tinfo.TribulationType = Rand.Range(1, 6);
            tinfo.TribulationStrikeInterval = 1000;

            return tinfo;
        }
        public static Hediff_CultivationLevel FindCultivationLevel(Pawn pawn)
        {
            Hediff_CultivationLevel hediff_CultivationLevel = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_CultivationLevel hediff_CultivationLevel2 && hediffs[i].Visible)
                {
                    hediff_CultivationLevel = hediff_CultivationLevel2;
                    return hediff_CultivationLevel;
                }
            }
            return null;
            
        }
        public static bool HaveCultivation(Pawn p)
        {
            for(int i = 0;i < p.health.hediffSet.hediffs.Count; i++)
            {
                if (p.health.hediffSet.hediffs[i] is Hediff_CultivationLevel c2 && c2.Visible)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool HaveCultivationOutHediff(Pawn p,out Hediff hediff)
        {
            for(int i = 0;i < p.health.hediffSet.hediffs.Count; i++)
            {
                if (p.health.hediffSet.hediffs[i] is Hediff_CultivationLevel c2 && c2.Visible)
                {
                    hediff = p.health.hediffSet.hediffs[i];
                    return true;
                }
            }
            hediff = null;
            return false;
        }
        public static bool HaveCultivationOutHediffDef(Pawn p,out HediffDef hediffDef)
        {
            for(int i = 0;i < p.health.hediffSet.hediffs.Count; i++)
            {
                if (p.health.hediffSet.hediffs[i] is Hediff_CultivationLevel c2 && c2.Visible)
                {
                    hediffDef = p.health.hediffSet.hediffs[i].def;
                    return true;
                }
            }
            hediffDef = null;
            return false;
        }

        public static HediffDef GetHighestCultivationHediff(HediffDef h1,HediffDef h2)
        {
            IDictionary<HediffDef,int> rlist = realmListAll;
            int num = Mathf.Max(rlist[h1], rlist[h2]);
            HediffDef h3 = realmListRanking[num];
            return h3;
        }

        public static HediffDef GetHighestCultivationFromList(List<HediffDef> hlist)
        {
            HediffDef hediff = null;
            int num = 0;
            foreach(var item in hlist)
            {
                if (realmListAll[item] > num)
                {
                    num = realmListAll[item];
                }
                else
                {
                    continue;
                }
            }
            hediff = realmListRanking[num];
            return hediff;
        }

        public static int GetRealmDifferent(HediffDef h1, HediffDef h2)
        {
            int num = realmListAll[h1];
            int num2 = realmListAll[h2];
            int num3 = Mathf.Max(num,num2) - Mathf.Min(num,num2);
            return num3;
        }
        public static List<Pawn> GetNearbyPawnFriendAndFoe(IntVec3 center, Map map, float radius)
        {
            List<Pawn> list = new List<Pawn>();
            float num = radius * radius;
            foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
            {
                if (item.Spawned && !item.Dead)
                {
                    float num2 = item.Position.DistanceToSquared(center);
                    if (num2 <= num)
                    {
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public static List<Pawn> GetNearbyPawnFriendAndFoeNeedSight(IntVec3 center, Map map, float radius,bool needLoS)
        {
            List<Pawn> list = new List<Pawn>();
            float num = radius * radius;
            foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
            {
                if (item.Spawned && !item.Dead)
                {
                    float num2 = item.Position.DistanceToSquared(center);
                    if (num2 <= num)
                    {
                        if(needLoS && !GenSight.LineOfSightToThing(center, item, map))
                        {
                            continue;
                        }
                        list.Add(item);
                    }
                }
            }
            return list;
        }
        public static bool isFriendly(Pawn pawn,Thing parent)
        {
            if (pawn.HostileTo(parent))
            {
                return false;
            }
            if (pawn.HostileTo(parent.Faction))
            {
                return false;
            }
            if (pawn.Faction.HostileTo(parent.Faction))
            {
                return false;
            }
            return true;
        }
        public static IDictionary<Pawn,float> GetNearbyPawnFriendAndFoeDict(IntVec3 center,Map map,float radius)
        {
            Dictionary<Pawn,float> list = new Dictionary<Pawn, float>();
            float num = radius * radius;
            foreach (Pawn item in map.mapPawns.AllPawnsSpawned)
            {
                if (item.Spawned && !item.Dead)
                {
                    float num2 = item.Position.DistanceToSquared(center);
                    if (num2 <= num)
                    {
                        list.Add(item, item.Position.DistanceTo(center));
                    }
                }
            }
            return list;
        }
        public static Hediff GetFirstHediffOfDef(Pawn pawn, HediffDef hediffDef)
        {
            return pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
        }
        public static BodyPartRecord GetBodyPartFromDef(Pawn pawn, BodyPartDef bodyDef)
        {
            if (bodyDef == CTR_DefOf.Brain)
            {
                return pawn.health.hediffSet.GetBrain();
            }
            return pawn.RaceProps.body.AllParts.FirstOrFallback((BodyPartRecord x) => x.def == bodyDef);
        }
        public static BodyPartRecord GetBodyPartFromHediffDef(Pawn pawn, HediffDef hediffDef)
        {
            return pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef).Part;
        }
        public static FleckCreationData GetDataStatic(Vector3 loc, Map map, FleckDef fleckDef, float scale = 1f)
        {
            FleckCreationData result = default(FleckCreationData);
            result.def = fleckDef;
            result.spawnPosition = loc;
            result.scale = scale;
            result.ageTicksOverride = -1;
            return result;
        }

        public static void ThrowObjectAt(Map map,Vector3 origin, Vector3 targetCell, FleckDef fleck,float scaleMin,float scaleMax)
        {
            if (origin.ToIntVec3().ShouldSpawnMotesAt(map))
            {
                float num = 3f;
                Vector3 vector = targetCell;
                vector.y = origin.y;
                FleckCreationData dataStatic = GetDataStatic(origin, map, fleck);
                dataStatic.rotationRate = Rand.Range(-300, 300);
                dataStatic.velocityAngle = (vector - dataStatic.spawnPosition).AngleFlat();
                dataStatic.velocitySpeed = num;
                dataStatic.scale = Rand.Range(scaleMin,scaleMax);
                dataStatic.airTimeLeft = (vector - dataStatic.spawnPosition).magnitude;
                map.flecks.CreateFleck(dataStatic);
            }
        }
        public static Hediff CreateHediff(Pawn pawn, HediffDef hediffDef, int ticks, StatDef statDef = null)
        {
            float num = ticks;
            num *= statDef != null ? pawn.GetStatValue(statDef) : 1f;
            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            if (hediff.TryGetComp<HediffComp_Disappears>() != null)
            {
                hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = Mathf.FloorToInt(num);
            }
            return hediff;
        }
        public static Hediff CreateHediffNoDuration(Pawn pawn, HediffDef hediffDef)
        {
            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            return hediff;
        }

        public static bool CellFilter(IntVec3 cell, Map map)
        {
            return cell.InBounds(map) || !cell.Fogged(map);
        }

        public static bool TryGetGrade(this Thing t, out ItemGrade gc)
        {
            CompItemGrade compItemGrade = ((t is MinifiedThing minifiedThing) ? minifiedThing.InnerThing.TryGetComp<CompItemGrade>() : t.TryGetComp<CompItemGrade>());
            if (compItemGrade == null)
            {
                gc = ItemGrade.Mortal;
                return false;
            }
            gc = compItemGrade.Grade;
            return true;
        }
        public static bool TryGetPillGrade(this Thing t, out PillGrade gc)
        {
            CompPillGrade compPillGrade = ((t is MinifiedThing minifiedThing) ? minifiedThing.InnerThing.TryGetComp<CompPillGrade>() : t.TryGetComp<CompPillGrade>());
            if (compPillGrade == null)
            {
                gc = PillGrade.Spirit;
                return false;
            }
            gc = compPillGrade.Grade;
            return true;
        }
        public static PillGrade GeneratePillGradeTraderItem()
        {
            return GeneratePillGradeFromGaussian(1f, PillGrade.Emperor, PillGrade.Earth, PillGrade.Spirit);
        }
        private static PillGrade GeneratePillGradeFromGaussian(float widthFactor, PillGrade max = PillGrade.Emperor, PillGrade center = PillGrade.Mysterious, PillGrade min = PillGrade.Spirit)
        {
            float num = Rand.Gaussian((float)(int)center + 0.5f, widthFactor);
            if (num < (float)(int)min)
            {
                num = (int)min;
            }
            if (num > (float)(int)max)
            {
                num = (int)max;
            }
            return (PillGrade)(int)num;
        }
        public static ItemGrade GenerateGradeTraderItem()
        {
            return GenerateFromGaussian(1f, ItemGrade.Dao, ItemGrade.Earth, ItemGrade.Mortal);
        }
        private static ItemGrade GenerateFromGaussian(float widthFactor, ItemGrade max = ItemGrade.Dao, ItemGrade center = ItemGrade.Ordinary, ItemGrade min = ItemGrade.Mortal)
        {
            float num = Rand.Gaussian((float)(int)center + 0.5f, widthFactor);
            if (num < (float)(int)min)
            {
                num = (int)min;
            }
            if (num > (float)(int)max)
            {
                num = (int)max;
            }
            return (ItemGrade)(int)num;
        }
        public static ItemGrade GenerateGradeCreatedByPawn(Pawn pawn)
        {
            int cultivationLevel = 0;
            Hediff_CultivationLevel level = FindCultivationLevel(pawn);
            if(level != null)
            {
                cultivationLevel = realmListAll[level.def];
                /*foreach (var item in realmListAll)
                {
                    if (item.Key == level.def)
                    {
                        cultivationLevel = item.Value;
                        break;
                    }
                }*/
            }
            bool flag = pawn.InspirationDef == InspirationDefOf.Inspired_Creativity;            
            ItemGrade gradeCategory = GenerateQualityCreatedByCultivator(cultivationLevel, flag);
            if (ModsConfig.IdeologyActive && pawn.Ideo != null)
            {
                Precept_Role role = pawn.Ideo.GetRole(pawn);
                if (role != null && role.def.roleEffects != null)
                {
                    RoleEffect roleEffect = role.def.roleEffects.FirstOrDefault((RoleEffect eff) => eff is RoleEffect_ProductionQualityOffset);
                    if (roleEffect != null)
                    {
                        gradeCategory = AddLevels(gradeCategory, ((RoleEffect_ProductionQualityOffset)roleEffect).offset);
                    }
                }
            }
            if (flag)
            {
                pawn.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Creativity);
            }
            return gradeCategory;
        }

        public static ItemGrade GenerateQualityCreatedByCultivator(int cultivationLevel, bool inspired)
        {
            float num = 0f;
            switch (cultivationLevel)
            {
                case 0:
                    num += 0.7f;
                    break;
                case 1:
                    num += 1.1f;
                    break;
                case 2:
                    num += 1.5f;
                    break;
                case 3:
                    num += 1.8f;
                    break;
                case 4:
                    num += 2f;
                    break;
                case 5:
                    num += 2.2f;
                    break;
                case 6:
                    num += 2.4f;
                    break;
                case 7:
                    num += 2.6f;
                    break;
                case 8:
                    num += 3.5f;
                    break;
                case 9:
                    num += 3.6f;
                    break;
                case 10:
                    num += 3.7f;
                    break;
                case 11:
                    num += 3.8f;
                    break;
                case 12:
                    num += 5.5f;
                    break;
                case 13:
                    num += 5.6f;
                    break;
                case 14:
                    num += 5.7f;
                    break;
                case 15:
                    num += 6.5f;
                    break;
                case 16:
                    num += 6.6f;
                    break;
                case 17:
                    num += 6.7f;
                    break;
                case 18:
                    num += 9.0f;
                    break;
                case 19:
                    num += 20f;
                    break;
            }
            int value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.8f);
            value = Mathf.Clamp(value, 0, 7);
            if (value == 7 && Rand.Value < 0.5f)
            {
                value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.95f);
                value = Mathf.Clamp(value, 0, 7);
            }
            ItemGrade GradeCategory = (ItemGrade)value;
            if (inspired)
            {
                GradeCategory = AddLevels(GradeCategory, 2);
            }
            return GradeCategory;
        }

        private static ItemGrade AddLevels(ItemGrade grade, int levels)
        {
            return (ItemGrade)Mathf.Min((int)grade + levels, 7);
        }

        //PillGrade
        public static PillGrade GeneratePillGradeCreatedByPawn(Pawn pawn, Hediff_CultivationLevel level)
        {
            int cultivationLevel = 0;
            if (level != null)
            {
                cultivationLevel = realmListAll[level.def];
                /*foreach (var item in realmListAll)
                {
                    if (item.Key == level.def)
                    {
                        cultivationLevel = item.Value;
                        break;
                    }
                }*/
            }
            bool flag = pawn.InspirationDef == InspirationDefOf.Inspired_Creativity;
            PillGrade gradeCategory = GeneratePillQualityCreatedByCultivator(cultivationLevel, flag);
            if (ModsConfig.IdeologyActive && pawn.Ideo != null)
            {
                Precept_Role role = pawn.Ideo.GetRole(pawn);
                if (role != null && role.def.roleEffects != null)
                {
                    RoleEffect roleEffect = role.def.roleEffects.FirstOrDefault((RoleEffect eff) => eff is RoleEffect_ProductionQualityOffset);
                    if (roleEffect != null)
                    {
                        gradeCategory = AddPillLevels(gradeCategory, ((RoleEffect_ProductionQualityOffset)roleEffect).offset);
                    }
                }
            }
            if (flag)
            {
                pawn.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Creativity);
            }
            return gradeCategory;
        }

        public static PillGrade GeneratePillQualityCreatedByCultivator(int cultivationLevel, bool inspired)
        {
            float num = 0f;
            switch (cultivationLevel)
            {
                case 0:
                    num += 0.7f;
                    break;
                case 1:
                    num += 1.1f;
                    break;
                case 2:
                    num += 1.5f;
                    break;
                case 3:
                    num += 1.8f;
                    break;
                case 4:
                    num += 2f;
                    break;
                case 5:
                    num += 2.2f;
                    break;
                case 6:
                    num += 2.4f;
                    break;
                case 7:
                    num += 2.6f;
                    break;
                case 8:
                    num += 3.5f;
                    break;
                case 9:
                    num += 3.6f;
                    break;
                case 10:
                    num += 3.7f;
                    break;
                case 11:
                    num += 3.8f;
                    break;
                case 12:
                    num += 5.5f;
                    break;
                case 13:
                    num += 5.6f;
                    break;
                case 14:
                    num += 5.7f;
                    break;
                case 15:
                    num += 6.5f;
                    break;
                case 16:
                    num += 6.6f;
                    break;
                case 17:
                    num += 6.7f;
                    break;
                case 18:
                    num += 9.0f;
                    break;
                case 19:
                    num += 20f;
                    break;
            }
            int value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.8f);
            value = Mathf.Clamp(value, 0, 5);
            if (value == 5 && Rand.Value < 0.5f)
            {
                value = (int)Rand.GaussianAsymmetric(num, 0.6f, 0.95f);
                value = Mathf.Clamp(value, 0, 5);
            }
            PillGrade GradeCategory = (PillGrade)value;
            if (inspired)
            {
                GradeCategory = AddPillLevels(GradeCategory, 2);
            }
            return GradeCategory;
        }

        private static PillGrade AddPillLevels(PillGrade grade, int levels)
        {
            return (PillGrade)Mathf.Min((int)grade + levels, 5);
        }
        public static string GetLabel(this ItemGrade cat)
        {
            return cat switch
            {
                ItemGrade.Mortal => "ItemGrade_Mortal".Translate(),
                ItemGrade.Ordinary => "ItemGrade_Ordinary".Translate(),
                ItemGrade.Earth => "ItemGrade_Earth".Translate(),
                ItemGrade.Heaven => "ItemGrade_Heaven".Translate(),
                ItemGrade.Mysterious => "ItemGrade_Mysterious".Translate(),
                ItemGrade.Divine => "ItemGrade_Divine".Translate(),
                ItemGrade.Emperor => "ItemGrade_Emperor".Translate(),
                ItemGrade.Dao => "ItemGrade_Dao".Translate(),
                _ => throw new ArgumentException(),
            };
        }

        public static string PillGetLabel(this PillGrade cat)
        {
            return cat switch
            {
                PillGrade.Spirit => "PillGrade_Spirit".Translate(),
                PillGrade.Earth => "PillGrade_Earth".Translate(),
                PillGrade.Heaven => "PillGrade_Heaven".Translate(),
                PillGrade.Mysterious => "PillGrade_Mysterious".Translate(),
                PillGrade.Divine => "PillGrade_Divine".Translate(),
                PillGrade.Emperor => "PillGrade_Emperor".Translate(),
                _ => throw new ArgumentException(),
            };
        }
        public static string neededQiTier(int num)
        {
            string text = "none";
            switch(num)
            {
                case 1:
                    text = "none";
                    break;
                case 2:
                    text = "none";
                    break;
                case 3:
                    text = "none";
                    break;
                case 4:
                    text = "Pure_Qi";
                    break;
                case 5:
                    text = "Pure_Qi";
                    break;
                case 6:
                    text = "Pure_Qi";
                    break;
                case 7:
                    text = "Pure_Qi";
                    break;
                case 8:
                    text = "Nascent_Qi";
                    break;
                case 9:
                    text = "Nascent_Qi";
                    break;
                case 10:
                    text = "Saint_Qi";
                    break;
                case 11:
                    text = "Saint_Qi";
                    break;
                case 12:
                    text = "Saint_Qi";
                    break;
                case 13:
                    text = "Immortal_Qi";
                    break;
                case 14:
                    text = "Immortal_Qi";
                    break;
                case 15:
                    text = "Immortal_Qi";
                    break;
                case 16:
                    text = "Immortal_Qi";
                    break;
                case 17:
                    text = "Immortal_Qi";
                    break;
                case 18:
                    text = "Immortal_Qi";
                    break;
                case 19:
                    text = "Immortal_Qi";
                    break;

            }
            return text;
        }

        public static IDictionary<HediffDef,int > realmListAll = new Dictionary<HediffDef, int>()
        {
            { CTR_DefOf.CTR_BodyTempering,1},
            { CTR_DefOf.CTR_MarrowCleansing,2},
            { CTR_DefOf.CTR_BoneForging,3},
            { CTR_DefOf.CTR_Qi_Gathering ,4},
            { CTR_DefOf.CTR_FoundationEstablishment ,5},
            { CTR_DefOf.CTR_CoreShaping, 6 },
            { CTR_DefOf.CTR_GoldenCore, 7 },
            { CTR_DefOf.CTR_NascentSoul, 8 },
            { CTR_DefOf.CTR_Transcendent, 9 },
            { CTR_DefOf.CTR_HalfStep_Saint, 10 },
            { CTR_DefOf.CTR_SaintRealm, 11 },
            { CTR_DefOf.CTR_SaintKing, 12 },
            { CTR_DefOf.CTR_ImmortalAscension, 13 },
            { CTR_DefOf.CTR_TrueImmortal, 14 },
            { CTR_DefOf.CTR_ImmortalSaint, 15 },
            { CTR_DefOf.CTR_HalfStep_God, 16 },
            { CTR_DefOf.CTR_True_God, 17 },
            { CTR_DefOf.CTR_Creation_Realm, 18 },
            { CTR_DefOf.CTR_OutsidetheDomain, 19 }
        };

        public static IDictionary<int,HediffDef> realmListRanking = new Dictionary<int,HediffDef>()
        {
            {1, CTR_DefOf.CTR_BodyTempering},
            {2, CTR_DefOf.CTR_MarrowCleansing},
            {3, CTR_DefOf.CTR_BoneForging},
            {4, CTR_DefOf.CTR_Qi_Gathering},
            {5, CTR_DefOf.CTR_FoundationEstablishment},
            {6, CTR_DefOf.CTR_CoreShaping},
            {7, CTR_DefOf.CTR_GoldenCore},
            {8, CTR_DefOf.CTR_NascentSoul},
            {9, CTR_DefOf.CTR_Transcendent},
            {10, CTR_DefOf.CTR_HalfStep_Saint},
            {11, CTR_DefOf.CTR_SaintRealm},
            {12, CTR_DefOf.CTR_SaintKing},
            {13, CTR_DefOf.CTR_ImmortalAscension},
            {14, CTR_DefOf.CTR_TrueImmortal},
            {15, CTR_DefOf.CTR_ImmortalSaint},
            {16, CTR_DefOf.CTR_HalfStep_God},
            {17, CTR_DefOf.CTR_True_God},
            {18, CTR_DefOf.CTR_Creation_Realm},
            {19, CTR_DefOf.CTR_OutsidetheDomain}
        };

        public static Dictionary<HediffDef, float> RealmList = new Dictionary<HediffDef, float>()
        {
            { CTR_DefOf.CTR_BodyTempering, 0.15f},
            { CTR_DefOf.CTR_MarrowCleansing, 0.14f},
            { CTR_DefOf.CTR_BoneForging, 0.13f},
            { CTR_DefOf.CTR_Qi_Gathering, 0.12f},
            { CTR_DefOf.CTR_FoundationEstablishment, 0.11f},
            { CTR_DefOf.CTR_CoreShaping, 0.10f},
            { CTR_DefOf.CTR_GoldenCore, 0.09f},
            { CTR_DefOf.CTR_NascentSoul, 0.08f},
            { CTR_DefOf.CTR_Transcendent, 0.07f},
            { CTR_DefOf.CTR_HalfStep_Saint, 0.06f},
            { CTR_DefOf.CTR_SaintRealm, 0.05f},
            { CTR_DefOf.CTR_SaintKing, 0.04f},
            { CTR_DefOf.CTR_ImmortalAscension, 0.03f},
            { CTR_DefOf.CTR_TrueImmortal, 0.02f},
            { CTR_DefOf.CTR_ImmortalSaint, 0.01f},
            { CTR_DefOf.CTR_HalfStep_God, 0.005f},
            { CTR_DefOf.CTR_True_God, 0.003f},
            { CTR_DefOf.CTR_Creation_Realm, 0.0005f},
            { CTR_DefOf.CTR_OutsidetheDomain, 0.0005f}
        };

        public static Dictionary<HediffDef, float> RealmListMortalTransforming = new Dictionary<HediffDef, float>()
        {
            { CTR_DefOf.CTR_BodyTempering, 0.15f},
            { CTR_DefOf.CTR_MarrowCleansing, 0.14f},
            { CTR_DefOf.CTR_BoneForging, 0.13f},
        };
        public static Dictionary<HediffDef, float> RealmListQiCultivating = new Dictionary<HediffDef, float>()
        {
            { CTR_DefOf.CTR_BodyTempering, 0.15f},
            { CTR_DefOf.CTR_MarrowCleansing, 0.14f},
            { CTR_DefOf.CTR_BoneForging, 0.13f},
            { CTR_DefOf.CTR_Qi_Gathering, 0.12f},
            { CTR_DefOf.CTR_FoundationEstablishment, 0.11f},
            { CTR_DefOf.CTR_CoreShaping, 0.10f},
            { CTR_DefOf.CTR_GoldenCore, 0.09f},
            { CTR_DefOf.CTR_NascentSoul, 0.08f},
            { CTR_DefOf.CTR_Transcendent, 0.07f},
            { CTR_DefOf.CTR_HalfStep_Saint, 0.06f},
            { CTR_DefOf.CTR_SaintRealm, 0.05f},
            { CTR_DefOf.CTR_SaintKing, 0.04f},
        };
        public static Dictionary<HediffDef, float> RealmListImmortal = new Dictionary<HediffDef, float>()
        {
            { CTR_DefOf.CTR_BodyTempering, 0.15f},
            { CTR_DefOf.CTR_MarrowCleansing, 0.14f},
            { CTR_DefOf.CTR_BoneForging, 0.13f},
            { CTR_DefOf.CTR_Qi_Gathering, 0.12f},
            { CTR_DefOf.CTR_FoundationEstablishment, 0.11f},
            { CTR_DefOf.CTR_CoreShaping, 0.10f},
            { CTR_DefOf.CTR_GoldenCore, 0.09f},
            { CTR_DefOf.CTR_NascentSoul, 0.08f},
            { CTR_DefOf.CTR_Transcendent, 0.07f},
            { CTR_DefOf.CTR_HalfStep_Saint, 0.06f},
            { CTR_DefOf.CTR_SaintRealm, 0.05f},
            { CTR_DefOf.CTR_SaintKing, 0.04f},
            { CTR_DefOf.CTR_ImmortalAscension, 0.03f},
            { CTR_DefOf.CTR_TrueImmortal, 0.02f},
            { CTR_DefOf.CTR_ImmortalSaint, 0.01f},
            { CTR_DefOf.CTR_HalfStep_God, 0.005f},
            { CTR_DefOf.CTR_True_God, 0.003f},
        };

        public static Dictionary<HediffDef, float> GetRealmDict()
        {
            return RealmList;
        }

        public static Dictionary<string, float> qiSourceMultiplier = new Dictionary<string, float>()
        {
            {"Qi_Source",0.1f },
            {"Qi_Source_Tier2",0.15f},
            {"Qi_Source_Tier3",0.2f},
            {"Qi_Source_Tier4",0.25f},
            {"Qi_Source_Tier5",0.3f},
            {"Qi_Source_Tier6",0.35f}
            
        };
        /*{"Qi_Source",1f },
            {"Qi_Source_Tier2",2f},
            {"Qi_Source_Tier3",3f},
            {"Qi_Source_Tier4",4f},
            {"Qi_Source_Tier5",5f},
            {"Qi_Source_Tier6",6f},*/
        public static string getAllowedTagFromThing(List<string> tagList,string tag)
        {
            string text = "N/A";
            foreach(var item in tagList)
            {
                if(item == tag || item.Contains(tag))
                {
                    text = item;
                    break;
                }
            }
            return text;
        }
        public static float getQiSouceMultiplierForPlant(string tag)
        {
            float num = 0f;

            switch (tag)
            {
                case null:
                    break;
                case "Qi_Source_Tier1":
                    num = 0.1f;
                    break;
                case "Qi_Source_Tier2":
                    num = 0.15f;
                    break;
                case "Qi_Source_Tier3":
                    num = 0.2f;
                    break;
                case "Qi_Source_Tier4":
                    num = 0.25f;
                    break;
                case "Qi_Source_Tier5":
                    num = 0.3f;
                    break;
                case "Qi_Source_Tier6":
                    num = 0.35f;
                    break;
                default:
                    num = 0f;
                    break;
            }

            return num;
        }
        /*public static Dictionary<string, float> qiSourceMultiplierForPawn = new Dictionary<string, float>()
        {
            {"Qi_Source",0.1f },
            {"Qi_Source_Tier2",0.15f},
            {"Qi_Source_Tier3",0.2f},
            {"Qi_Source_Tier4",0.25f},
            {"Qi_Source_Tier5",0.3f},
            {"Qi_Source_Tier6",0.35f},
        };*/

        public static float getQiModifierForPawn(Pawn p,string tag)
        {
            float num = 1f;
            switch (tag)
            {
                case null:
                    break;
                case "Neutral_Qi":
                    num *= 1f;
                    break;
                case "Yang_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_YangQi_AbsorptionMultiplier);
                    break;
                case "Yin_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_YinQi_AbsorptionMultiplier);
                    break;
                case "Cold_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_ColdQi_AbsorptionMultiplier);
                    break;
                case "Metal_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_MetalQi_AbsorptionMultiplier);
                    break;
                case "Water_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_WaterQi_AbsorptionMultiplier);
                    break;
                case "Wood_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_WoodQi_AbsorptionMultiplier);
                    break;
                case "Fire_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_FireQi_AbsorptionMultiplier);
                    break;
                case "Earth_Qi":
                    num *= p.GetStatValue(CTR_DefOf.CTR_EarthQi_AbsorptionMultiplier);
                    break;
            }
            return num;
        }

        public static string getQiTypeString(string tag)
        {
            string text = "N/A";
            switch (tag)
            {
                case null:
                    break;
                case "Neutral_Qi":
                    text = "Neutral_Qi";
                    break;
                case "Yang_Qi":
                    text = "Neutral_Qi";
                    break;
                case "Yin_Qi":
                    text = "Neutral_Qi";
                    break;
                case "Cold_Qi":
                    text = "Water_Qi";
                    break;
                case "Metal_Qi":
                    text = "Metal_Qi";
                    break;
                case "Water_Qi":
                    text = "Water_Qi";
                    break;
                case "Wood_Qi":
                    text = "Wood_Qi";
                    break;
                case "Fire_Qi":
                    text = "Fire_Qi";
                    break;
                case "Earth_Qi":
                    text = "Earth_Qi";
                    break;
            }
            return text;
        }

        public static FleckDef getQiTypeFleck(string tag)
        {
            FleckDef text = null;
            switch (tag)
            {
                case null:
                    break;
                case "Neutral_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbPure;
                    break;
                case "Yang_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbPure;
                    break;
                case "Yin_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbPure;
                    break;
                case "Cold_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbWater;
                    break;
                case "Metal_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbMetal;
                    break;
                case "Water_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbWater;
                    break;
                case "Wood_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbWood;
                    break;
                case "Fire_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbFire;
                    break;
                case "Earth_Qi":
                    text = CTR_DefOf.CTR_AbsorbQiOrbEarth;
                    break;
            }
            return text;
        }

        public static float getItemQiValueForPawn(string tag)
        {
            float num = 0f;
            switch (tag)
            {
                case null:
                    break;
                case "Qi_Source_Tier1":
                    num = 0.01f;
                    break;
                case "Qi_Source_Tier2":
                    num = 0.015f;
                    break;
                case "Qi_Source_Tier3":
                    num = 0.02f;
                    break;
                case "Qi_Source_Tier4":
                    num = 0.025f;
                    break;
                case "Qi_Source_Tier5":
                    num = 0.03f;
                    break;
                case "Qi_Source_Tier6":
                    num = 0.035f;
                    break;
                default:
                    num = 0f; 
                    break;
            }
            return num;
        }

        public static string getItemQiTier(Thing thing)
        {
            string text = "N/A";
            foreach(var item in thing.def.tradeTags)
            {
                switch (item)
                {
                    case "Qi_Source_Tier1":
                        text = "Qi_Source_Tier1";
                        return text;
                    case "Qi_Source_Tier2":
                        text = "Qi_Source_Tier2";
                        return text;
                    case "Qi_Source_Tier3":
                        text = "Qi_Source_Tier3";
                        return text;
                    case "Qi_Source_Tier4":
                        text = "Qi_Source_Tier4";
                        return text;
                    case "Qi_Source_Tier5":
                        text = "Qi_Source_Tier5";
                        return text;
                    case "Qi_Source_Tier6":
                        text = "Qi_Source_Tier6";
                        return text;
                }
            }            
            return text;
        }

        public static string qiMultiplierFromGrade(ItemGrade x)
        {
            string text = null;
            switch(x)
            {
                case ItemGrade.Mortal:
                    text = "N/A";
                    break;
                case ItemGrade.Ordinary: 
                    text = "Qi_Source_Tier1"; 
                    break;
                case ItemGrade.Earth: 
                    text = "Qi_Source_Tier1"; 
                    break;
                case ItemGrade.Heaven: 
                    text = "Qi_Source_Tier2"; 
                    break;
                case ItemGrade.Mysterious: 
                    text = "Qi_Source_Tier3"; 
                    break;
                case ItemGrade.Divine:
                    text = "Qi_Source_Tier4"; 
                    break;
                case ItemGrade.Emperor: 
                    text = "Qi_Source_Tier5"; 
                    break;
                case ItemGrade.Dao: 
                    text = "Qi_Source_Tier6"; 
                    break;
            }
            return text;
        }

        public static HediffDef GetRandomRealmByWeight()
        {
            
            float rand = Rand.Value;
            if(rand < 0.30f)
            {
                IDictionary<HediffDef, float> IlistGet = RealmListMortalTransforming;
                HediffDef hediffDef = IlistGet.RandomElementByWeight(x => x.Value).Key;
                if (hediffDef == null) hediffDef = IlistGet.First().Key;
                return hediffDef;
            }
            else if(rand < 0.20f)
            {
                IDictionary<HediffDef, float> IlistGet = RealmListQiCultivating;
                HediffDef hediffDef = IlistGet.RandomElementByWeight(x => x.Value).Key;
                if (hediffDef == null) hediffDef = IlistGet.First().Key;
            }
            else if(rand < 0.10f)
            {
                IDictionary<HediffDef, float> IlistGet = RealmListImmortal;               
                HediffDef hediffDef = IlistGet.RandomElementByWeight(x => x.Value).Key;
                if (hediffDef == null) hediffDef = IlistGet.First().Key;
            }
            
            
            return null;
        }

        public static float GetBreakthroughChance(Pawn pawn, HediffDef nextLevel = null)
        {
            if (pawn.RaceProps.Humanlike)
            {
                float baseNum = 1f;
                float num = pawn.health.summaryHealth.SummaryHealthPercent;
                float num2 = pawn.needs.mood.CurLevelPercentage;
                float num3 = Mathf.Clamp(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness), 0.01f, 1f);
                float final = baseNum * num * num2 * num3;
                if (nextLevel != null && nextLevel == CTR_DefOf.CTR_OutsidetheDomain)
                {
                    float num4 = pawn.GetStatValue(CTR_DefOf.TribulationChance);
                    float num5 = 1f - num4;
                    final *= num5;
                }
                final *= CultivatorOfTheRimMod.settings.breakthroughSuccessOverallModifier;
                return final;
            }
            return 1f;
        }
        
        public static float CalculateCultivationGained(Hediff cultivationHediff,List<Thing> sourceList ,float baselineValue,float totalSeverityGainedFromNearbySource ,float multiplierForQiType,float cultivationSpeed)
        {
            //get base value specified in hediffComp Props. each realm have different value.
            float value = baselineValue;
            
            //comulative value from all source of Qi
            value += totalSeverityGainedFromNearbySource;

            //how efficient is the pawn able to absorb said Qi type, 
            value *= multiplierForQiType;

            //cultivation speed stat, this stat can be increase by various source such as Qi pill etc.
            value *= cultivationSpeed;

            //if "need qi source to progress" is enabled in mod setting and no nearby source present, set final value to 0.
            if(CultivatorOfTheRimMod.settings.isCultivatorNeedQiSourceToImprove && sourceList.NullOrEmpty())
            {
                value = 0;
            }
            return value;
        }

        //From this point on is calculation for xp gain
        public static void ThrowText(Vector3 pos,Map map,string text,Color color)
        {
            MoteMaker.ThrowText(pos, map, text, color);
        }
        public static FleckDef GetQiOrbType(string item)
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
        }
        public static void SpawnQiOrbColor(string tier, string type, Vector3 vector3,Vector3 targetPos,Map map)
        {
            Vector3 vector = vector3 + new Vector3(Rand.Range(-0.5f, 0.5f), 0.5f, Rand.Range(-0.5f, 0.5f));
            float num = getItemQiValueForPawn(tier);
            num *= 12f;
            ThrowObjectAt(map, vector3, targetPos, GetQiOrbType(type), 0.25f + num, 0.25f + num);
        }

        public static void SpawnOrb(Pawn pawn,Dictionary<Vector3,Thing> itemToSpawnFleckList,Dictionary<Thing,float> QiSourceWithValue,Dictionary<Thing,string> QisourceTypeWithStringCached)
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
            if (!itemToSpawnFleckList.NullOrEmpty())
            {
                Vector3 tempItem = itemToSpawnFleckList.First().Key;
                float num = QiSourceWithValue[itemToSpawnFleckList.First().Value];
                num *= 2f;
                SpawnQiOrbColor(getItemQiTier(itemToSpawnFleckList.First().Value), QisourceTypeWithStringCached[itemToSpawnFleckList.First().Value], tempItem, pawn.DrawPos,itemToSpawnFleckList.First().Value.Map);
                
            }

        }        
    }

}

