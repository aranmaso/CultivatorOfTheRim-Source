using RimWorld;
using Verse;
using Verse.Sound;

namespace CultivatorOfTheRim;

public class CompUseEffect_PlaySound : CompUseEffect
{
    public CompProperties_UseEffectPlaySound Props => (CompProperties_UseEffectPlaySound)props;
    
    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        Props.soundDef.PlayOneShot(usedBy);
    }
}