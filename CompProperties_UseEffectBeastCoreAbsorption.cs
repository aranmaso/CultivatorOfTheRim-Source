using RimWorld;
using Verse;

namespace CultivatorOfTheRim;

public class CompProperties_UseEffectBeastCoreAbsorption : CompProperties_UseEffect
{
    public HediffDef HediffDef;

    public FloatRange severity = new FloatRange(1,10);

    public CompProperties_UseEffectBeastCoreAbsorption()
    {
        compClass = typeof(CompUseEffect_BeastCoreAbsorption);
    }
}