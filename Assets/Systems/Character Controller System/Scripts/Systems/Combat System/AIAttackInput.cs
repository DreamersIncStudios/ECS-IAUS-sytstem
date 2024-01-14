using System.Collections;
using System.Collections.Generic;
using DreamersInc.ComboSystem;
using Unity.Entities;
using UnityEngine.UI;

public struct AIAttackInput : IComponentData
{
    public AnimationTrigger TriggerAnimation;

    public AIAttackInput(AnimationTrigger animationTrigger)
    {
        TriggerAnimation = animationTrigger;
    }
}

[UpdateInGroup(typeof(IAUSUpdateBrainGroup))]
public partial class AIAttackSystem : SystemBase
{
    protected override void OnUpdate()
    {
        
        Entities.WithStructuralChanges().WithoutBurst().ForEach((Entity entity, Command handler, ref AIAttackInput input) =>
        {
            handler.InputQueue ??= new Queue<AnimationTrigger>();
            handler.InputQueue.Enqueue(input.TriggerAnimation);
            EntityManager.RemoveComponent<AIAttackInput>(entity);
        }).Run();

    }
}
