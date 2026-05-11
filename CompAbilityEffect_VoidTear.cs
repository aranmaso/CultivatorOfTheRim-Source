using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CultivatorOfTheRim;

public class CompAbilityEffect_VoidTear : CompAbilityEffect
{
    public new CompProperties_AbilityEffectVoidTear Props => (CompProperties_AbilityEffectVoidTear)props;
    
    public static string SkipUsedSignalTag = "CompAbilityEffect.SkipUsed";
    
    public static void SendSkipUsedSignal(LocalTargetInfo target, Thing initiator)
    {
        Find.SignalManager.SendSignal(new Signal(SkipUsedSignalTag, target.Named("POSITION"), initiator.Named("SUBJECT")));
    }
    public override IEnumerable<PreCastAction> GetPreCastActions()
    {
        yield return new PreCastAction
        {
            action = delegate(LocalTargetInfo t, LocalTargetInfo d)
            {
                if (!parent.def.HasAreaOfEffect)
                {
                    Pawn pawn = t.Pawn;
                    if (pawn != null)
                    {
                        FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(pawn, FleckDefOf.PsycastSkipFlashEntry, new Vector3(-0.5f, 0f, -0.5f));
                        dataAttachedOverlay.link.detachAfterTicks = 5;
                        pawn.Map.flecks.CreateFleck(dataAttachedOverlay);
                    }
                    else
                    {
                        FleckMaker.Static(t.CenterVector3, parent.pawn.Map, FleckDefOf.PsycastSkipFlashEntry);
                    }
                    FleckMaker.Static(d.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipInnerExit);
                }
                FleckMaker.Static(t.Cell, parent.pawn.Map, FleckDefOf.PsycastSkipOuterRingExit);
                if (!parent.def.HasAreaOfEffect)
                {
                    SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(t.Cell, parent.pawn.Map));
                    SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(d.Cell, parent.pawn.Map));
                }
            },
            ticksAwayFromCast = 5
        };
    }
    
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        LocalTargetInfo destination = target;
        if (!destination.IsValid)
        {
            return;
        }
        Pawn pawn = parent.pawn;
        if (!parent.def.HasAreaOfEffect)
        {
            parent.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(target.Thing, pawn.Map), target.Thing.Position, 60);
        }
        else
        {
            parent.AddEffecterToMaintain(EffecterDefOf.Skip_EntryNoDelay.Spawn(target.Thing, pawn.Map), target.Thing.Position, 60);
        }
        parent.AddEffecterToMaintain(EffecterDefOf.Skip_Exit.Spawn(destination.Cell, pawn.Map), destination.Cell, 60);
        target.Thing.TryGetComp<CompCanBeDormant>()?.WakeUp();
        target.Thing.Position = destination.Cell;
        if (target.Thing is Pawn pawn2)
        {
            if ((pawn2.Faction == Faction.OfPlayer || pawn2.IsPlayerControlled) && pawn2.Position.Fogged(pawn2.Map))
            {
                FloodFillerFog.FloodUnfog(pawn2.Position, pawn2.Map);
            }
            pawn2.Notify_Teleported();
            SendSkipUsedSignal(pawn2.Position, pawn2);
        }
    }
    public bool CanPlaceSelectedTargetAt(LocalTargetInfo target)
    {
        Pawn pawn = parent.pawn;
        if (pawn != null)
        {
            Building_Door door = target.Cell.GetDoor(pawn.MapHeld);
            if (door != null && !door.CanPhysicallyPass(pawn))
            {
                return false;
            }
            if (pawn.Spawned && !target.Cell.Impassable(parent.pawn.Map))
            {
                return target.Cell.WalkableBy(parent.pawn.Map, pawn);
            }
            return false;
        }
        return CanTeleportThingTo(target, parent.pawn.Map);
    }
    public static bool CanTeleportThingTo(LocalTargetInfo target, Map map)
    {
        Building edifice = target.Cell.GetEdifice(map);
        if (edifice != null && edifice.def.surfaceType != SurfaceType.Item && edifice.def.surfaceType != SurfaceType.Eat && !(edifice is Building_Door { Open: not false }))
        {
            return false;
        }
        List<Thing> thingList = target.Cell.GetThingList(map);
        for (int i = 0; i < thingList.Count; i++)
        {
            if (thingList[i].def.category == ThingCategory.Item)
            {
                return false;
            }
        }
        return true;
    }
}