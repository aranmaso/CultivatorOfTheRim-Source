using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateInitialHediffs")]
    public class PawnGenerator_GenerateInitialHediffsPatch_FactionCultivation
    {
        public static List<FactionCultivationDef> list => StaticCollectionCached.FactionCultivationDef;
        public static void Postfix(Pawn pawn)
        {
            if (pawn == null) return;
            if (list.EnumerableNullOrEmpty())
            {
                return;
            }
            if (FactionCultivationMatch(pawn, out var factionCultivation))
            {
                GiveCultivation(pawn, factionCultivation);
            }
        }
        public static bool FactionCultivationMatch(Pawn pawn, out FactionCultivationDef factionCultivationDef)
        {
            foreach (var item in list)
            {
                if (item.isBlacklist)
                {
                    continue;
                }
                if (item.isHumanlikeOnly && !pawn.RaceProps.Humanlike)
                {
                    continue;
                }
                if (ModsConfig.BiotechActive && pawn.RaceProps.Humanlike)
                {
                    if (item.xenotypeDefs.NotNullAndContains(pawn.genes.Xenotype))
                    {
                        factionCultivationDef = item;
                        return true;
                    }
                }
                if (item.pawnKindDef == pawn.kindDef)
                {
                    factionCultivationDef = item;
                    return true;
                }
                else
                {
                    if (pawn.Faction != null)
                    {
                        if (item.factionDefs.NotNullAndContains(pawn.Faction?.def))
                        {
                            factionCultivationDef = item;
                            return true;
                        }
                    }
                }
            }
            factionCultivationDef = null;
            return false;
        }
        public static void GiveCultivation(Pawn pawn, FactionCultivationDef factionCultivationDef)
        {
            if(factionCultivationDef.isBlacklist) return;
            if (pawn.HaveAnyCultivation())
            {
                return;
            }
            if (Rand.Chance(factionCultivationDef.chance))
            {
                Hediff hediff = null;
                if (!factionCultivationDef.cultivationWeights.NullOrEmpty())
                {
                    hediff = HediffMaker.MakeHediff(factionCultivationDef.cultivationWeights.RandomElementByWeight(x => x.weight).cultivationRealm, pawn);
                }
                else
                {
                    if (factionCultivationDef.realmRank.IsValid)
                    {
                        hediff = factionCultivationDef.isAllowBodyCultivation ?
                                 HediffMaker.MakeHediff(StaticCollectionCached.CultivationRealmPower.Where(x => x.Value >= factionCultivationDef.realmRank.min && x.Value <= factionCultivationDef.realmRank.max).RandomElement().Key, pawn)
                                 : HediffMaker.MakeHediff(StaticCollectionCached.CultivationRealmPower.Where(x => x.Key.hediffClass != typeof(Hediff_BodyCultivaton) && x.Value >= factionCultivationDef.realmRank.min && x.Value <= factionCultivationDef.realmRank.max).RandomElement().Key, pawn);
                    }
                    else
                    {
                        hediff = factionCultivationDef.isAllowBodyCultivation ?
                            HediffMaker.MakeHediff(StaticCollectionCached.CultivationRealmWeight.RandomElementByWeight(x => x.Value).Key, pawn)
                            : HediffMaker.MakeHediff(StaticCollectionCached.CultivationRealmWeight.Where(x => x.Key.hediffClass != typeof(Hediff_BodyCultivaton)).RandomElementByWeight(x => x.Value).Key, pawn);
                    }
                }
                Hediff hediff2 = HediffMaker.MakeHediff(CTR_DefOf.CTR_BreakthroughCounter, pawn);
                int selectedStage = Rand.RangeInclusive(1, hediff.def.stages.Count);
                if (selectedStage > hediff.def.stages.Count())
                {
                    selectedStage = hediff.def.stages.Count();
                }
                float sev = 0f;
                sev = hediff.def.stages[selectedStage - 1].minSeverity;
                hediff.Severity = sev;
                pawn.health.AddHediff(hediff);
                pawn.health.AddHediff(hediff2);
                Messages.Message(pawn.LabelShort + " spawned with " + hediff.Label,pawn, MessageTypeDefOf.SilentInput);
            }
        }
    }
}
