using UnityEngine;
using Verse;

namespace CultivatorOfTheRim
{
    public class MoteMovingToPoint : Mote
    {
        public MoteAttachLink link2 = MoteAttachLink.Invalid;

        public float LifetimeFraction => base.AgeSecs / def.mote.Lifespan;

        public void Attach(TargetInfo a, TargetInfo b)
        {
            link1 = new MoteAttachLink(a, Vector3.zero);
            link2 = new MoteAttachLink(b, Vector3.zero);
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
            UpdatePositionAndRotation();
        }

        protected void UpdatePositionAndRotation()
        {
            if (link1.Linked && link2.Linked)
            {
                if (!link1.Target.ThingDestroyed)
                {
                    link1.UpdateDrawPos();
                }
                if (!link2.Target.ThingDestroyed)
                {
                    link2.UpdateDrawPos();
                }
                Vector3 lastDrawPos = link1.LastDrawPos;
                Vector3 lastDrawPos2 = link2.LastDrawPos;
                exactPosition = lastDrawPos + (lastDrawPos2 - lastDrawPos) * LifetimeFraction;
                if (def.mote.rotateTowardsTarget)
                {
                    exactRotation = lastDrawPos.AngleToFlat(lastDrawPos2) + 90f;
                }
            }
            exactPosition.y = def.altitudeLayer.AltitudeFor();
        }
    }
}
