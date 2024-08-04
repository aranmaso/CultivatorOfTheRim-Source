using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace CultivatorOfTheRim
{
    public class CompGetNearbyPawn : ThingComp
    {

        public CompProperties_GetNearbyPawn Props => (CompProperties_GetNearbyPawn)props;

        public IDictionary<Pawn,float> list => Cultivation_Utility.GetNearbyPawnFriendAndFoeDict(parent.Position,parent.Map,Props.radius);

        public Dictionary<Pawn,float> newList = new Dictionary<Pawn, float>();
        public override void CompTick()
        {
            base.CompTick();
            if(parent.IsHashIntervalTick(Props.checkInterval))
            {
                newList.Clear();
                foreach(var item in list)
                {
                    if(Props.targetOnlyFemale && item.Key.gender != Gender.Female)
                    {
                        continue;
                    }
                    if(Props.onlyTargetCultivator && !Cultivation_Utility.HaveCultivation(item.Key))
                    {
                        continue;
                    }
                    if(Props.hostileOnly && (!item.Key.HostileTo(parent.Faction) || !item.Key.Faction.HostileTo(parent.Faction)))
                    {
                        continue;
                    }
                    if(Props.friendlyOnly && (item.Key.HostileTo(parent.Faction) || item.Key.Faction.HostileTo(parent.Faction)))
                    {
                        continue;
                    }
                    if(!Props.isTargetDowned && item.Key.Downed)
                    {
                        continue;
                    }
                    if(Props.targetSpecificFaction != null && item.Key.Faction.def != Props.targetSpecificFaction)
                    {
                        continue;
                    }
                    newList.Add(item.Key, item.Value);
                }
            }
        }
    }
}
