using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;

namespace InfluenceMap
{
    public class SetInfluences : JobComponentSystem
    {
        public EntityQueryDesc influencersGlobal = new EntityQueryDesc()
        {
            All = new ComponentType[]{ typeof(Influencer),typeof (LocalToWorld)},
            Any = new ComponentType[] {  typeof(Cover) }

        };
        public EntityQueryDesc influencersPlayer = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(LocalToWorld) },
            Any = new ComponentType[] { typeof(PlayerCharacter) }

        }; public EntityQueryDesc influencersEnemy = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(LocalToWorld) },
            Any = new ComponentType[] { typeof(EnemyCharacter) }

        };
        public EntityQueryDesc CoverInfluencer = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(LocalToWorld) , typeof(StaticInfluencer) }

        };
        int interval = 360;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();


        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (UnityEngine.Time.frameCount % interval == 2)
            {

                EntityQuery PlayerQ = GetEntityQuery(influencersPlayer);
                EntityQuery EnemyQ = GetEntityQuery(influencersEnemy);
                EntityQuery Cover = GetEntityQuery(CoverInfluencer);
                JobHandle CoverSet = new SetStaticGlobal()
                {
                    StaticEntity = Cover.ToEntityArray(Allocator.TempJob),
                    Influence = GetComponentDataFromEntity<Influencer>(true),
                    Position = GetComponentDataFromEntity<LocalToWorld>(true)
                }.Schedule(this,inputDeps);
                var setinf = new Set()
                {
                    InfPosPlayer = PlayerQ.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                    influencersPlayer = PlayerQ.ToComponentDataArray<Influencer>(Allocator.TempJob),

                    InfPosEnemies = EnemyQ.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                    influencersEnemies = EnemyQ.ToComponentDataArray<Influencer>(Allocator.TempJob)
                };

                JobHandle handle = setinf.Schedule(this, CoverSet);
                return handle;
            }
            else

                return inputDeps;
            
        }
    }


    [BurstCompile]
    public struct Set : IJobForEach_B<Gridpoint>
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Influencer> influencersPlayer;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> InfPosPlayer;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Influencer> influencersEnemies;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> InfPosEnemies;


        public void Execute(DynamicBuffer<Gridpoint> gridpoints)
        {
            for(int cnt = 0; cnt< gridpoints.Length;cnt++) {
                Gridpoint temp= gridpoints[cnt];
                temp.Player = new Influence();
                temp.Enemy = new Influence();

                for (int index = 0; index < InfPosPlayer.Length; index++)
                {

                        float dist = Vector3.Distance(temp.Position, InfPosPlayer[index].Position);

                    if (influencersPlayer[index].influence.Proximity.y > dist)
                    {
                        temp.Player.Proximity.x += influencersPlayer[index].influence.Proximity.x;
                    }
                    if (influencersPlayer[index].influence.Threat.y > dist)
                    {
                        temp.Player.Threat.x += influencersPlayer[index].influence.Threat.x;
                    }
                }

                for (int index = 0; index < InfPosEnemies.Length; index++)
                {

                    float dist = Vector3.Distance(temp.Position, InfPosEnemies[index].Position);

                    if (influencersEnemies[index].influence.Proximity.y > dist)
                    {
                        temp.Enemy.Proximity.x += influencersEnemies[index].influence.Proximity.x;
                    }
                    if (influencersEnemies[index].influence.Threat.y > dist)
                    {
                        temp.Enemy.Threat.x += influencersEnemies[index].influence.Threat.x;
                    }
                }
                gridpoints[cnt] = temp;
            }

        }
    }

}