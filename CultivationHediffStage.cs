using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class CultivationHediffStage
    {
        public float minSeverity;

        public string label;

        public string overrideLabel;

        public bool empty = false;

        public List<BonusHediff> hediffChance;

        public List<HediffDef> hediffsGained;

        public List<HediffDef> hediffsRemoved;

        public List<AbilityDef> abilities;

        public List<AbilityDef> removeAbilities;

        public List<GeneDef> geneDefs;

        public List<GeneDef> geneDefsRemove;

        public bool isXenogene = false;

        public List<TraitRequirement> traitDefs;

        public List<TraitRequirement> removeTraitDefs;

        public List<ThingDef> attachableThing;

        public List<ThingDef> removeAttachedThing;

        public bool isRemoveOnHediffLost = false;

        public bool sendLetter = false;

        public string letterLabel;

        public string letterDesc;

        public IEnumerable<StatDrawEntry> SpecialDisplayStats(int priority)
        {
            return SpecialDisplayStats(this, priority);
        }

        public IEnumerable<StatDrawEntry> SpecialDisplayStats(CultivationHediffStage stage,int priority)
        {
            int num = priority;
            if (hediffChance.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation,$"<b>Chance to gain hediff</b>".Colorize(Color.cyan),"","hediff that have a chance to be gained in this cultivation stage", num);
                num--;
                foreach (var entry in hediffChance)
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation,entry.hediffDef.LabelCap,entry.chance.ToStringPercent("0"),entry.hediffDef.description, num);
                    num--;
                }                
            }
            if (hediffsGained.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Hediff Gained</b>".Colorize(Color.cyan), "", "hediff gained in this cultivation stage", num);
                num--;
                foreach (var entry in hediffsGained)
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.LabelCap, "", entry.description, num);
                    num--;
                }
            }
            if (hediffsRemoved.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Hediff Removed</b>".Colorize(Color.cyan), "", "hediff that will be removed in this cultivation stage", num);
                num--;
                foreach (var entry in hediffsRemoved)
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.LabelCap, "", entry.description, num);
                    num--;
                }
            }
            if (abilities.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Ability Gained</b>".Colorize(Color.cyan), "", "ability that will be gained in this cultivation stage", num);
                num--;
                foreach (var entry in abilities)
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.LabelCap, "", entry.description, num);
                    num--;
                }
            }
            if (removeAbilities.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Ability Removed</b>".Colorize(Color.cyan), "", "ability that will be removed in this cultivation stage", num);
                num--;
                foreach (var entry in removeAbilities)
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.LabelCap, "", entry.description, num);
                    num--;
                }
            }
            if (ModsConfig.BiotechActive)
            {
                if (geneDefs.NotNullOrEmpty())
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Gene Gained</b>".Colorize(Color.cyan), "", "gene that will be added in this cultivation stage", num);
                    num--;
                    foreach (var entry in geneDefs)
                    {
                        yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.LabelCap, "xenogene: " + isXenogene, entry.description, num);
                        num--;
                    }
                }
                if (geneDefsRemove.NotNullOrEmpty())
                {
                    yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Gene Removed</b>".Colorize(Color.cyan), "", "gene that will be removed in this cultivation stage", num);
                    num--;
                    foreach (var entry in geneDefsRemove)
                    {
                        yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.LabelCap, "", entry.description, num);
                        num--;
                    }
                }
            }
            if (traitDefs.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Trait Gained</b>".Colorize(Color.cyan), "", "trait that will be added in this cultivation stage", num);
                num--;
                foreach (var entry in traitDefs)
                {
                    if (entry.degree.HasValue)
                    {
                        yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.def.DataAtDegree(entry.degree.Value).LabelCap, entry.def.DataAtDegree(entry.degree.Value).LabelCap, entry.def.DataAtDegree(entry.degree.Value).description, num);
                    }
                    else
                    {
                        yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.def.DataAtDegree(0).LabelCap, entry.def.DataAtDegree(0).LabelCap, entry.def.DataAtDegree(0).description, num);
                    }
                    num--;
                }
            }
            if (removeTraitDefs.NotNullOrEmpty())
            {
                yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, "<b>Trait Removed</b>".Colorize(Color.cyan), "", "trait that will be removed in this cultivation stage", num);
                num--;
                foreach (var entry in removeTraitDefs)
                {
                    if (entry.degree.HasValue)
                    {
                        yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.def.DataAtDegree(entry.degree.Value).LabelCap, entry.def.DataAtDegree(entry.degree.Value).LabelCap, entry.def.DataAtDegree(entry.degree.Value).description, num);
                    }
                    else
                    {
                        yield return new StatDrawEntry(CTR_DefOf.CTR_PawnCultivation, entry.def.DataAtDegree(0).LabelCap, entry.def.DataAtDegree(0).LabelCap, entry.def.DataAtDegree(0).description, num);
                    }
                    num--;
                }
            }
        }

        /*public List<CompProperties> comps;

        public List<CompProperties> compsRemove;*/

    }

}
