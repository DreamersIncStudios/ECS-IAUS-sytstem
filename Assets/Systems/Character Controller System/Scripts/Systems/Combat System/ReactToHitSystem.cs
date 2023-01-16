using UnityEngine;
using Unity.Entities;
using DreamersInc.CombatSystem.Animation;
using Stats.Entities;

namespace DreamersInc.ComboSystem
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class ReactToHitSystem : SystemBase
    {

        EntityCommandBuffer commandBuffer;

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((Entity entity, AnimatorComponent Animcomp,ref ReactToContact contact ) => {
                //Todo Add check to see if we can interrupt 
                Direction dir = contact.HitDirection(out Vector3 dirToTarget);
                var rb = Animcomp.RB;
                var anim = Animcomp.anim;

                rb.AddForce(dirToTarget * contact.HitIntensity, ForceMode.Impulse);
                if (contact.HitIntensity < 5)
                {
                    switch (dir)
                    {
                        case Direction.Left:
                            anim.Play("HitLeft", 0);
                            break;
                        case Direction.Right:
                            anim.Play("HitRight", 0);
                            break;
                        case Direction.Front:
                            anim.Play("HitFront", 0);
                            break;
                        case Direction.Back:
                            anim.Play("HitBack", 0);
                            break;
                    }
                }
                else
                {
                    switch (dir)
                    {
                        case Direction.Left:
                            anim.Play("HitLeftStrong", 0);
                            break;
                        case Direction.Right:
                            anim.Play("HitRightStrong", 0);
                            break;
                        case Direction.Front:
                            anim.Play("HitFrontStrong", 0);
                            break;
                        case Direction.Back:
                            anim.Play("HitBackStrong", 0);
                            break;
                    }
                }

                EntityManager.RemoveComponent<ReactToContact>(entity);
            }).WithStructuralChanges().Run();
        }

  
       
    }
}