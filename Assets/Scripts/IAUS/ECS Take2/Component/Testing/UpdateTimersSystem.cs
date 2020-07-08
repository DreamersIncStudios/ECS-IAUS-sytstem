using IAUS.ECS2;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.Core;

namespace Utilities.ReactiveSystem
{
    public abstract class ResetTimerSystem<AISTATE> : SystemBase
        where AISTATE : unmanaged, BaseStateScorer

    {
        private EntityQuery _EntitiesWithState;
       
        protected override void OnCreate()
        {
            base.OnCreate();
            _EntitiesWithState = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AISTATE)) }
            }
                );
        }

        protected EntityCommandBufferSystem GetCommandBufferSystem()
        {
            return World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        [BurstCompile]
        private struct ManageStateResetTimerJob : IJobChunk
        {
            public ArchetypeChunkComponentType<AISTATE> StateChunk;
            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
            [ReadOnly]public float DT;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AISTATE> AIStates = chunk.GetNativeArray(StateChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; ++i)
                {
                    AISTATE state = AIStates[i];
                    if (state.Status == ActionStatus.CoolDown)
                    {

                        if (state.ResetTime > 0.0f)
                            state.ResetTime -= DT;
                        else
                        {
                            state.Status = ActionStatus.Idle;
                            state.ResetTime = 0.0f;
                        }
                    }
                    AIStates[i] = state;
                }

            }
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new ManageStateResetTimerJob()
            {
                StateChunk = GetArchetypeChunkComponentType<AISTATE>(false),
                EntityChunk = GetArchetypeChunkEntityType(),
                DT = Time.DeltaTime

            }.ScheduleParallel(_EntitiesWithState, systemDeps);

            Dependency = systemDeps;

        }

    }
    [UpdateInGroup(typeof(IAUS_UpdateScore))]
    public class patrolTimer : ResetTimerSystem<Patrol> {}
    [UpdateInGroup(typeof(IAUS_UpdateScore))]
    public class WaitTimer : ResetTimerSystem<WaitTime> { }
    [UpdateInGroup(typeof(IAUS_UpdateScore))]
    public class FollowTimer : ResetTimerSystem<FollowCharacter> { }



}