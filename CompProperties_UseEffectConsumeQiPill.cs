using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CultivatorOfTheRim;

public class CompProperties_UseEffectConsumeQiPill : CompProperties_UseEffect
{
    public FloatRange severity = new FloatRange(0.01f,0.1f);

    public List<string> minimumCultivationLevel;

    public HediffDef hediffDef;
    
    public HediffDef backlashHediffDef;
    
    public bool notForMortal = false;
    
    public bool lethalForMortal = false;
    
    public bool allowBodyCultivator = false;
    
    public bool onlyBodyCultivator = false;
    
    public float Hediffchance = 1f;
    
    public CompProperties_UseEffectConsumeQiPill()
    {
        compClass = typeof(CompUseEffect_ConsumeQiPill);
    }
}