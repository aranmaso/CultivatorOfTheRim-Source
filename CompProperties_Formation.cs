using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CultivatorOfTheRim
{
    public class CompProperties_Formation : CompProperties
    {
        public HediffDef heddiffDef;

        public int hediffDuration = 2500;

        public bool isPlayingEffector = true;

        public EffecterDef effecterDef;

        public bool isSpeedUpPlant;

        public FloatRange speedUpPercent;

        public bool isOnlyTargetHostile;

        public bool isOnlyTargetFriendly;

        public bool isDoingDamage;

        public int damageAmount;

        public bool isShootingProjectile;

        public bool randomSpawnPosition = false;

        public ThingDef projectile;

        public SoundDef shootingSound;

        public int targetLimit = 5;

        public float radius;

        public int checkInterval;

        public int totalDuration = 5000;

        public int spiritStoneCost = 1;

        public int activeDelay = 300;

        public string uiIcon;
        public CompProperties_Formation() 
        {
            compClass = typeof(CompFormation);
        }
    }
}
