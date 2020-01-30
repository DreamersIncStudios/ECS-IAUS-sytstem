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
            Any = new ComponentType[] { typeof(PlayerCharaacter) }

        }; public EntityQueryDesc influencersEnemy = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(LocalToWorld) },
            Any = new ComponentType[] { typeof(EnemyCharacter) }

        };
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

        
            EntityQuery GlobalQ = GetEntityQuery(influencersGlobal);
            EntityQuery PlayerQ = GetEntityQuery(influencersPlayer);
            EntityQuery EnemyQ = GetEntityQuery(influencersEnemy);


            var setinf = new Set()
            {
                InfPosGlobal = GlobalQ.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                influencersGlobal = GlobalQ.ToComponentDataArray<Influencer>(Allocator.TempJob),

                InfPosPlayer = PlayerQ.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                influencersPlayer = PlayerQ.ToComponentDataArray<Influencer>(Allocator.TempJob),

                InfPosEnemies =PlayerQ.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
                influencersEnemies = PlayerQ.ToComponentDataArray<Influencer>(Allocator.TempJob)
            };

            JobHandle handle = setinf.Schedule(this, inputDeps);
            return handle;
        }
    }


    [BurstCompile]
    public struct Set : IJobForEach_B<Gridpoint>
    {
        [ReadOnly] public NativeArray<LocalToWorld> InfPosGlobal;
        [ReadOnly] public NativeArray<Influencer> influencersGlobal;

        [ReadOnly] public NativeArray<Influencer> influencersPlayer;
        [ReadOnly] public NativeArray<LocalToWorld> InfPosPlayer;

        [ReadOnly] public NativeArray<Influencer> influencersEnemies;
        [ReadOnly] public NativeArray<LocalToWorld> InfPosEnemies;


        public void Execute(DynamicBuffer<Gridpoint> gridpoints)
        {
            for(int cnt = 0; cnt< gridpoints.Length;cnt++) {
                Gridpoint temp= gridpoints[cnt];
                temp.Global = new Influence();
                temp.Player = new Influence();
                temp.Enemy = new Influence();


                for (int index = 0; index < InfPosGlobal.Length; index++) {

                    if(temp.dist==0.0f)
                        temp.dist = Vector3.Distance(temp.Position, InfPosGlobal[index].Position);

                    if (influencersGlobal[index].influence.Proximity.y > temp.dist)  
                        temp.Global.Proximity.x += influencersGlobal[index].influence.Proximity.x;

                }
                for (int index = 0; index < InfPosPlayer.Length; index++)
                {

                        float dist = Vector3.Distance(temp.Position, InfPosPlayer[index].Position);

                    if (influencersPlayer[index].influence.Proximity.y > dist)
                    {
                        temp.Player.Proximity.x += influencersPlayer[index].influence.Proximity.x;
                        if(temp.Global.Proximity.x>0)
                            temp.Global.Proximity.x -= influencersPlayer[index].influence.Proximity.x;
                    }
                }
                for (int index = 0; index < InfPosEnemies.Length; index++)
                {

                    float dist = Vector3.Distance(temp.Position, InfPosEnemies[index].Position);

                    if (influencersEnemies[index].influence.Proximity.y > dist)
                    {
                        temp.Enemy.Proximity.x += influencersEnemies[index].influence.Proximity.x;
                        if (temp.Global.Proximity.x > 0)
                            temp.Global.Proximity.x -= influencersEnemies[index].influence.Proximity.x;
                    }
                }
                gridpoints[cnt] = temp;
            }

        }
    }

}