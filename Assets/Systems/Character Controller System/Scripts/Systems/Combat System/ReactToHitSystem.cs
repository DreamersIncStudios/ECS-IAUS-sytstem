using UnityEngine;
using Unity.Entities;
using DreamersInc.CombatSystem.Animation;
using Stats.Entities;
using DreamersInc.DamageSystem.Interfaces;

namespace DreamersInc.ComboSystem
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class ReactToHitSystem : SystemBase
    {

        EntityCommandBuffer commandBuffer;

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((Entity entity, Animator Anim, Rigidbody RB, ref ReactToContact contact) =>
            {
                Direction dir = contact.HitDirection(out Vector3 dirToTarget);
               
                RB.AddForce(dirToTarget * contact.HitIntensity, ForceMode.Impulse);

                //Todo Add check to see if we can interrupt 
      
                if (contact.HitIntensity < 5 )
                {
                    switch (dir)
                    {
                        case Direction.Left:
                            Anim.Play("HitLeft", 0);
                            break;
                        case Direction.Right:
                            Anim.Play("HitRight", 0);
                            break;
                        case Direction.Front:
                            Anim.Play("HitFront", 0);
                            break;
                        case Direction.Back:
                            Anim.Play("HitBack", 0);
                            break;
                    }
                }
                else
                {
                    switch (dir)
                    {
                        case Direction.Left:
                            Anim.Play("HitLeftStrong", 0);
                            break;
                        case Direction.Right:
                            Anim.Play("HitRightStrong", 0);
                            break;
                        case Direction.Front:
                            Anim.Play("HitFrontStrong", 0);
                            break;
                        case Direction.Back:
                            Anim.Play("HitBackStrong", 0);
                            break;
                    }
                }

                EntityManager.RemoveComponent<ReactToContact>(entity);
            }).WithStructuralChanges().Run();

            Entities.WithoutBurst().WithNone<Animator>().ForEach((Entity entity, Rigidbody RB, ref ReactToContact contact) =>
            {
                //Todo add react movement.
                EntityManager.RemoveComponent<ReactToContact>(entity);

            }).WithStructuralChanges().Run(); ;
            
        }

  
       
    }
}