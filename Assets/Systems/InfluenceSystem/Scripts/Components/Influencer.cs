using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Transforms;
using CharacterAlignmentSystem;


/// <summary> Work to Do 
/// Rewrite this as A single job 
/// </summary>
namespace InfluenceMap
{
    [GenerateAuthoringComponent]
    public struct Influencer : IComponentData
    {
        public Influence influence;
        public float RingWidth;
        public FallOff fallOff;



        public float M { get { return 3.0f; } }
        public float K { get { return 1.0f; } } // Value of K is to be between -1 and 1 for Logistic Responses
        public float B { get { return -2.0f; } }
        public float C { get { return 0.0f; } }

        public float Output(float input)
        {
            float temp = new float();
            switch (fallOff)
            {
                case FallOff.LinearQuadInverse:
                    temp = M * Mathf.Pow((input - C), K) + B;
                    break;
                case FallOff.Logistic:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C))) + B;
                    break;
            }
            return temp;
        }
    }
        public struct InfluenceValues : IComponentData
        {
            public Gridpoint InfluenceAtSelf;
            public Gridpoint InfluenceAtTarget;
            public float3 TargetLocation;
        }

    public class GetInfluenceSystem : JobComponentSystem
    {

        public EntityQueryDesc StaticObjectQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer) },

            Any = new ComponentType[] { typeof(Cover) }
        };

        public EntityQueryDesc DynamicAttackaleObjectQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(CharacterAlignment) }
        };


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<Entity> StaticObjects = GetEntityQuery(StaticObjectQuery).ToEntityArray(Allocator.TempJob);

            NativeArray<Entity> DynamicObjects = GetEntityQuery(DynamicAttackaleObjectQuery).ToEntityArray(Allocator.TempJob);

            ComponentDataFromEntity<LocalToWorld> positions = GetComponentDataFromEntity<LocalToWorld>(true);
            ComponentDataFromEntity<Influencer> Influence = GetComponentDataFromEntity<Influencer>(true);
            ComponentDataFromEntity<CharacterAlignment> AttackableInfo = GetComponentDataFromEntity<CharacterAlignment>(true);

            JobHandle CheckInfluenceAtPoint = Entities
            .WithReadOnly(StaticObjects)
            .WithNativeDisableParallelForRestriction(StaticObjects)
            .WithReadOnly(DynamicObjects)
            .WithNativeDisableParallelForRestriction(positions)
            .WithReadOnly(positions)
            .WithNativeDisableParallelForRestriction(Influence)
            .WithReadOnly(Influence)
            .WithNativeDisableParallelForRestriction(AttackableInfo)
            .WithReadOnly(AttackableInfo)
            .ForEach((Entity entity, ref InfluenceValues influenceValues) =>
            {
                Gridpoint InfluenceAtPoint = new Gridpoint();
                for (int index = 0; index < StaticObjects.Length; index++)
                {
                    float dist = Vector3.Distance(influenceValues.TargetLocation, positions[StaticObjects[index]].Position);
                    Influencer Temp = Influence[StaticObjects[index]];
                    if (dist < Temp.influence.Proximity.y)
                    {
                        InfluenceAtPoint.Global.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                    }
                    if (dist < Temp.influence.Threat.y)
                    {
                        InfluenceAtPoint.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                    }
                }
                CharacterAlignment self = AttackableInfo[entity];


                for (int index = 0; index < DynamicObjects.Length; index++)
                {
                    float dist = Vector3.Distance(influenceValues.TargetLocation, positions[DynamicObjects[index]].Position);
                    Influencer Temp = Influence[DynamicObjects[index]];


                    switch (self.Faction)
                    {
                        case Alignment.Angel:

                            switch (AttackableInfo[DynamicObjects[index]].Faction)
                            {
                                case Alignment.Angel:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                                case Alignment.Human:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                                case Alignment.Daemon:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                            }

                            break;
                        case Alignment.Human:
                            switch (AttackableInfo[DynamicObjects[index]].Faction)
                            {
                                case Alignment.Angel:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                                case Alignment.Human:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                                case Alignment.Daemon:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                            }
                            break;
                        case Alignment.Daemon:
                            switch (AttackableInfo[DynamicObjects[index]].Faction)
                            {
                                case Alignment.Angel:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                                case Alignment.Human:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                                case Alignment.Daemon:
                                    if (dist < Temp.influence.Proximity.y)
                                    {
                                        InfluenceAtPoint.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                    }
                                    if (dist < Temp.influence.Threat.y)
                                    {
                                        InfluenceAtPoint.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                    }
                                    break;
                            }
                            break;
                    }

                }

                influenceValues.InfluenceAtTarget = InfluenceAtPoint;
            })
            .Schedule(inputDeps);

            JobHandle CheckInfluenceAtSelf = Entities
                .WithDeallocateOnJobCompletion(StaticObjects)
                .WithReadOnly(StaticObjects)
            .WithNativeDisableParallelForRestriction(StaticObjects)

                .WithDeallocateOnJobCompletion(DynamicObjects)
                .WithReadOnly(DynamicObjects)
                .WithNativeDisableParallelForRestriction(positions)
                .WithReadOnly(positions)
                .WithNativeDisableParallelForRestriction(Influence)
                .WithReadOnly(Influence)
                .WithNativeDisableParallelForRestriction(AttackableInfo)
                .WithReadOnly(AttackableInfo)
                .ForEach((Entity entity, ref InfluenceValues influenceValues, in LocalToWorld transform) =>
                {
                    Gridpoint InfluenceAtSelf = new Gridpoint();
                    for (int index = 0; index < StaticObjects.Length; index++)
                    {
                        float dist = Vector3.Distance(transform.Position, positions[StaticObjects[index]].Position);
                        Influencer Temp = Influence[StaticObjects[index]];
                        if (dist < Temp.influence.Proximity.y)
                        {
                            InfluenceAtSelf.Global.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                        }
                        if (dist < Temp.influence.Threat.y)
                        {
                            InfluenceAtSelf.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                        }
                    }
                    CharacterAlignment self = AttackableInfo[entity];


                    for (int index = 0; index < DynamicObjects.Length; index++)
                    {
                        float dist = Vector3.Distance(transform.Position, positions[DynamicObjects[index]].Position);
                        Influencer Temp = Influence[DynamicObjects[index]];


                        switch (self.Faction)
                        {
                            case Alignment.Angel:

                                switch (AttackableInfo[DynamicObjects[index]].Faction)
                                {
                                    case Alignment.Angel:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                    case Alignment.Human:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                    case Alignment.Daemon:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                }

                                break;
                            case Alignment.Human:
                                switch (AttackableInfo[DynamicObjects[index]].Faction)
                                {
                                    case Alignment.Angel:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                    case Alignment.Human:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                    case Alignment.Daemon:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                }
                                break;
                            case Alignment.Daemon:
                                switch (AttackableInfo[DynamicObjects[index]].Faction)
                                {
                                    case Alignment.Angel:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                    case Alignment.Human:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                    case Alignment.Daemon:
                                        if (dist < Temp.influence.Proximity.y)
                                        {
                                            InfluenceAtSelf.Ally.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                                        }
                                        if (dist < Temp.influence.Threat.y)
                                        {
                                            InfluenceAtSelf.Ally.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                                        }
                                        break;
                                }
                                break;
                        }

                    }


                    influenceValues.InfluenceAtSelf = InfluenceAtSelf;
                })
                .Schedule(CheckInfluenceAtPoint);

            return CheckInfluenceAtSelf;
        }




    }
    [System.Serializable]
    public struct Influence {
      
        public float2 Proximity;   //In physical attack range  Value and range
        public float2 Threat; //In range of attack or notice   Value and range
    }

    public enum FallOff
    {
        LinearQuadInverse,
        Ring,
        Barrier,
        Logistic


    }



}