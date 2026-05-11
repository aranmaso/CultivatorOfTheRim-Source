using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class FactionCultivationDef : Def
    {
        public bool isBlacklist = false;

        public string packageId;
        
        public List<FactionDef> factionDefs;

        public PawnKindDef pawnKindDef;

        [MayRequireBiotech]
        public List<XenotypeDef> xenotypeDefs = [];

        public List<CultivationWeight> cultivationWeights;

        public IntRange realmRank;

        public bool isAllowBodyCultivation = true;

        public bool isHumanlikeOnly = false;

        public float chance = 0.1f;
    }

    public class CultivationWeight
    {
        public HediffDef cultivationRealm;

        public float weight;
    }
}
