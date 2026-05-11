using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class GameComponent_CultivatorCacheManager : GameComponent
    {
        public GameComponent_CultivatorCacheManager(Game game)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            StaticCollectionCached.CleanUpCache();
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame % 60000 == 0)
            {
                StaticCollectionCached.CleanUpCache();
            }
        }
    }
}
