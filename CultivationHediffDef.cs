using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class CultivationHediffDef : HediffDef
    {
        public int realmPower = 0;

        public float realmWeight = 1f;

        public List<BonusHediff> bonusHediff;

        public List<CultivationHediffStage> cultivationStages;

        public CultivationHediffDef nextLevel;

        public List<BreakThroughRealmInfo> nextLevels;

        public bool canGetTribulation = false;

        public bool guaranteedTribulation = false;

        public IntRange breakthroughDuration = new IntRange(2500,5000);

        public int tribulationStrikeInterval = 250;

        public string breakthroughKeyed = "CTR_BreakthroughLabel";

        public string breakthroughDescKeyed = "CTR_BreakthroughDesc";

        public string breakthroughUiIcon;
        
        public Texture2D breakthroughUiIconTexture;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (breakthroughUiIcon != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    breakthroughUiIconTexture = ContentFinder<Texture2D>.Get(breakthroughUiIcon);
                });
            }
        }

        public int CultivationStageAtSeverity(float severity)
        {
            if (cultivationStages == null)
            {
                return 0;
            }

            for (int num = cultivationStages.Count - 1; num >= 0; num--)
            {
                if (severity >= cultivationStages[num].minSeverity)
                {
                    return num;
                }
            }

            return 0;
        }
        public IEnumerable<StatDrawEntry> SpecialCulDisplayStats(StatRequest req,int stage)
        {
            if (stages == null || stages.Count == 0)
            {
                yield break;
            }

            foreach (StatDrawEntry item in stages[stage].SpecialDisplayStats())
            {
                yield return item;
            }
        }

        public IEnumerable<StatDrawEntry> SpecialCulStat(CultivationHediffStage stage,int priority)
        {
            if (cultivationStages == null || cultivationStages.Count == 0)
            {
                yield break; 
            }

            foreach (var item in stage.SpecialDisplayStats(priority))
            {
                yield return item;
            }
        }
    }
}
