using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class HediffCompProperties_BodyCultivation : HediffCompProperties
    {
        public FloatRange severityPerDamageTaken = new FloatRange(0.01f,0.1f);

        public FloatRange severityPerDamageDealt = new FloatRange(0.01f,0.1f);

        public int tickInterval = 250;

        public HediffDef nextLevel;

        public List<BonusHediff> bonusHediff;

        public List<BreakThroughRealmInfo> nextLevels;

        public bool canGetTrib = false;

        public IntRange breakingThroughDuration;

        public int TribulationStrikeInterval = 250;

        public string uiIcon;

        public List<CultivatorNeedInfo> changeToNeeds;

        public bool requireQiSource = false;

        public bool isFixedAge = false;

        public List<GeneDef> bonusGenes;

        public Texture2D _uiIcon;

        public HediffCompProperties_BodyCultivation()
        {
            compClass = typeof(HediffComp_BodyCultivation);
        }

        public override void ResolveReferences(HediffDef parent)
        {
            base.ResolveReferences(parent);
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                _uiIcon = ContentFinder<Texture2D>.Get(uiIcon);
            });
        }
    }
}
