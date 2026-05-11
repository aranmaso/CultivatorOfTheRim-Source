using RimWorld;
using Verse;

namespace CultivatorOfTheRim;

public class CompProperties_UseEffectConsumeGenericPill : CompProperties_UseEffect
{
    public HediffDef hediffDef;

    public HediffDef hediffDefToRemove;

    public float baseDuration;
    
    public HediffDef specialHediffDef;

    public Gender genderRequirement;

    public BodyPartDef part;

    public FloatRange severity = new FloatRange(0.1f,0.1f);

    public CompProperties_UseEffectConsumeGenericPill()
    {
        compClass = typeof(CompUseEffect_ConsumeGenericPill);
    }
}