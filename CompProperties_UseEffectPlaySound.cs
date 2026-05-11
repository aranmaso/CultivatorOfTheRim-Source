using RimWorld;
using Verse;

namespace CultivatorOfTheRim;

public class CompProperties_UseEffectPlaySound : CompProperties_UseEffect
{
    public SoundDef soundDef;

    public CompProperties_UseEffectPlaySound()
    {
        compClass = typeof(CompUseEffect_PlaySound);
    }
}