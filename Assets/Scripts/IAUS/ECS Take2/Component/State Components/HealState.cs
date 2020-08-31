using Unity.Entities;
using UnityEngine;
using IAUS.Core;
using Unity.Jobs;
using Stats;
using System.Linq;
using Dreamers.InventorySystem;
using Unity.Collections;
using UnityEngine.SocialPlatforms;
using Unity.Burst;

namespace IAUS.ECS2 {
    public interface HealBaseState : BaseStateScorer
    {
         ConsiderationData Health { get; set; }
      //  public ConsiderationData ThreatInArea;
        ConsiderationData TimeBetweenHeals { get; set; }

        float TimeBetweenHealsTimer { get; set; }
    }


    public struct HealSelfViaItem : HealBaseState {


        [SerializeField] private ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] private float _resetTime;
        [SerializeField] private float _totalScore;
        public ConsiderationData RecoveryItemsInInventory;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
         public int FullInventoryofItem;
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

       [SerializeField] public ConsiderationData Health { get; set; }
        [SerializeField] public ConsiderationData TimeBetweenHeals { get; set; }
        
      
        public float TimeBetweenHealsTimer { get; set; }
    }


    public struct HealSelfActionTag :IComponentData{
        public bool test;
    }
    [UpdateInGroup(typeof(IAUS_UpdateScore))]
    public class HealSelfStateScore : SystemBase
    {
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = Entities.ForEach((ref HealSelfViaItem state, in PlayerStatComponent stats, in HealTimerConsideration timer 
                , in DynamicBuffer<InventoryConsiderationBuffer> InventoryBuffer
                ) =>
            {
                //find the right buffer
                float ItemRatio = 0.01f;
                for (int i = 0; i < InventoryBuffer.Length; i++)
                {
                    InventoryConsiderationBuffer buffer = InventoryBuffer[i];
                    if (buffer.Consider.ItemTypeToConsider == Dreamers.InventorySystem.TypeOfGeneralItem.Recovery)
                        ItemRatio = buffer.Consider.Ratio;
                }
             
            float TotalScore = Mathf.Clamp01(state.Health.Output(stats.HealthRatio)*
                    state.TimeBetweenHeals.Output(timer.Ratio) * state.RecoveryItemsInInventory.Output(ItemRatio)
                     );
          
               state.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * state.mod) * TotalScore);
             

            }
            ).ScheduleParallel(systemDeps);

            Dependency = systemDeps;
        }
    }
    [UpdateInGroup(typeof(IAUS_UpdateState))]
    public class Healaction : SystemBase
    {
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
           Entities
                  .WithoutBurst()
                  .ForEach((Entity entity, CharacterInventory characterInventory, ref HealSelfViaItem heal, ref HealTimerConsideration Timer, in HealSelfActionTag tag, in BaseAI AI) =>
                  {
                      if (heal.Status != ActionStatus.Success)
                      {
                          Dreamers.InventorySystem.Base.ItemSlot IS;
                          if (characterInventory.Inventory.ReturnHealtItem(out IS))
                          {
                              IS.Item.Use(characterInventory.Inventory, characterInventory.Inventory.ItemsInInventory.IndexOf(IS), characterInventory.Inventory.AttachedCharacter);
                              Timer.Timer = heal.TimeBetweenHealsTimer;
                              Debug.Log("Heal self now");
                              heal.Status = ActionStatus.Success;
                              EntityManager.RemoveComponent<HealSelfActionTag>(entity);
                          }
                      }
                      else
                      { 
                              EntityManager.RemoveComponent<HealSelfActionTag>(entity);
                      }
                  }
            ).Run();

            Dependency = systemDeps;
        }

        public class HealTimer : SystemBase
        {
            EntityQuery Heal;

            protected override void OnCreate()
            {
                base.OnCreate();
                Heal = GetEntityQuery(new EntityQueryDesc() { 
                    All = new ComponentType[] { ComponentType.ReadWrite(typeof(HealTimerConsideration)), ComponentType.ReadOnly(typeof(HealSelfViaItem))}
                });
            }

            protected override void OnUpdate()
            {
                float DT = Time.DeltaTime;

                JobHandle systemDeps = Dependency;
                systemDeps = new ReduceTimer<HealSelfViaItem>()
                {
                    DT = DT,
                    HealChunk = GetArchetypeChunkComponentType<HealSelfViaItem>(),
                    TimerChunk = GetArchetypeChunkComponentType<HealTimerConsideration>()
                }.ScheduleParallel(Heal, systemDeps);

                Dependency = systemDeps;

                }
            [BurstCompile]
            struct ReduceTimer<HEALSTATES>: IJobChunk
            where HEALSTATES : unmanaged, HealBaseState
            {
                public ArchetypeChunkComponentType<HealTimerConsideration> TimerChunk;
                public ArchetypeChunkComponentType<HEALSTATES> HealChunk;

                public float DT;
                public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
                {
                    NativeArray<HealTimerConsideration> TimersArray = chunk.GetNativeArray(TimerChunk);
                    NativeArray<HEALSTATES> HealBases = chunk.GetNativeArray(HealChunk);

                    for (int i = 0; i < chunk.Count; i++)
                    {
                        HealTimerConsideration timer = TimersArray[i];
                        HEALSTATES healBase = HealBases[i];
                        if (timer.Timer > 0.0)
                            timer.Timer -= DT;
                        else
                            timer.Timer = 0.0f;

                        timer.Ratio = timer.Timer / (float)healBase.TimeBetweenHealsTimer;
                        TimersArray[i] = timer;
                    }
                }
            }
        }

    }


}
