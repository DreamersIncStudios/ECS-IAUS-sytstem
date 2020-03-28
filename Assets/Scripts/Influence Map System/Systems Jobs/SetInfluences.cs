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
        public EntityQueryDesc influencersPlayers = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(LocalToWorld) },
            Any = new ComponentType[] { typeof(PlayerCharacter) }

        }; public EntityQueryDesc influencersEnemy = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(LocalToWorld) },
            Any = new ComponentType[] { typeof(EnemyCharacter) }

        };
        public EntityQueryDesc StaticInfluencer = new EntityQueryDesc()
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

                EntityQuery PlayerQ = GetEntityQuery(influencersPlayers);
                EntityQuery EnemyQ = GetEntityQuery(influencersEnemy);
                NativeArray<LocalToWorld> InfPosPlayer = PlayerQ.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
                NativeArray<Influencer> influencersPlayer = PlayerQ.ToComponentDataArray<Influencer>(Allocator.TempJob);

                NativeArray<LocalToWorld> InfPosEnemies = EnemyQ.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
                   NativeArray<Influencer> influencersEnemies = EnemyQ.ToComponentDataArray<Influencer>(Allocator.TempJob);

                NativeArray<Entity> Static = GetEntityQuery(StaticInfluencer).ToEntityArray(Allocator.TempJob);
                ComponentDataFromEntity<Influencer> Influence = GetComponentDataFromEntity<Influencer>(true);
                ComponentDataFromEntity<LocalToWorld> Position = GetComponentDataFromEntity<LocalToWorld>(true);
              
                JobHandle CoverSet = Entities
                    .WithDeallocateOnJobCompletion(Static)
                    .WithNativeDisableParallelForRestriction(Influence)
                    .WithNativeDisableParallelForRestriction(Position)
                    .ForEach((DynamicBuffer<Gridpoint> gridpoints) =>
                    {
                        for (int cnt = 0; cnt < gridpoints.Length; cnt++)
                        {
                            Gridpoint gridSquare = gridpoints[cnt];

                            for (int j = 0; j < Static.Length; j++)
                            {
                                float dist = Vector3.Distance(gridSquare.Position, Position[Static[j]].Position);

                                if (Influence[Static[j]].influence.Proximity.y > dist)
                                {
                                    gridSquare.Global.Proximity.x = Influence[Static[j]].influence.Proximity.x / dist;
                                }
                                if (Influence[Static[j]].influence.Threat.y > dist)
                                {
                                    gridSquare.Global.Threat.x = Influence[Static[j]].influence.Threat.x / dist;
                                }
                            }
                            gridpoints[cnt] = gridSquare;
                        }

                    })
                    .WithReadOnly(Influence)
                    .WithReadOnly(Position)
                    .Schedule(inputDeps);
                    

                JobHandle setinf = Entities
                    .WithDeallocateOnJobCompletion(InfPosEnemies)
                    .WithDeallocateOnJobCompletion(InfPosPlayer)
                    .WithDeallocateOnJobCompletion(influencersEnemies)
                    .WithDeallocateOnJobCompletion(influencersPlayer)

                    .ForEach((ref DynamicBuffer < Gridpoint > gridpoints) =>
                    
                    {
                        for (int cnt = 0; cnt < gridpoints.Length; cnt++)
                        {
                            Gridpoint temp = gridpoints[cnt];
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



                    }).Schedule( CoverSet);


                return setinf;
            }
            else

                return inputDeps;
            
        }
    }

}