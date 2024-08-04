using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultivatorOfTheRim
{
    [StaticConstructorOnStartup]
	public class WeatherEvent_LightningStrikeTribulation : WeatherEvent_LightningFlash
	{
		private IntVec3 strikeLoc = IntVec3.Invalid;

		private Mesh boltMesh;

		private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

		public int damageAmount = 1;

		public float radius = 3f;

		public WeatherEvent_LightningStrikeTribulation(Map map)
			: base(map)
		{
		}

		public WeatherEvent_LightningStrikeTribulation(Map map, IntVec3 forcedStrikeLoc,int damount,float r)
			: base(map)
		{
			strikeLoc = forcedStrikeLoc;
			damageAmount = damount;
			radius = r;
		}

		public override void FireEvent()
		{
			base.FireEvent();
			if (!strikeLoc.IsValid)
			{
				strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
			}
			boltMesh = LightningBoltMeshPool.RandomBoltMesh;
			if (!strikeLoc.Fogged(map))
			{
				GenExplosion.DoExplosion(strikeLoc, map, radius, CTR_DefOf.CTR_TribulationLightning, null,damageAmount, Rand.Range(0.5f,2f));
				Vector3 loc = strikeLoc.ToVector3Shifted();
				for (int i = 0; i < 4; i++)
				{
					FleckMaker.ThrowSmoke(loc, map, 1.5f);
					FleckMaker.ThrowMicroSparks(loc, map);
					FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
				}
			}
			SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
			SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            if (Rand.Value < CTR_DefOf.CTR_AzureFragment.generateCommonality && !strikeLoc.Fogged(map) && !strikeLoc.Impassable(map))
            {
                Thing thing = ThingMaker.MakeThing(CTR_DefOf.CTR_AzureFragment);
                thing.HitPoints += 10;
                GenSpawn.Spawn(thing, strikeLoc, map);
            }
            else if (Rand.Value <= CultivatorOfTheRimMod.settings.tribRemnantChance && !strikeLoc.Fogged(map) && !strikeLoc.Impassable(map))
            {
                Thing thing = ThingMaker.MakeThing(CTR_DefOf.CTR_TribulationRemnantPill);
                thing.HitPoints += 10;
                GenSpawn.Spawn(thing, strikeLoc, map);
            }
        }

		public override void WeatherEventDraw()
		{
			Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, base.LightningBrightness), 0);
		}
	}
}
