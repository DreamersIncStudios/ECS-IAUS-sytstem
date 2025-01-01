using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using AISenses;
using AISenses.VisionSystems;
using Components.MovementSystem;
using ProjectDawn.Navigation;
using Stats.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace IAUS.ECS.Component
{
    public struct TerrorizeAreaState : IBaseStateScorer
    {
        public float3 TargetPosition;
        public Entity TargetEntity;
        public int TargetPositionID;
        public TerrorizeAreaState(float coolDownTime, bool melee = false, bool magic = false, bool range = false)
        {
            this.coolDownTime = coolDownTime;
            _status = ActionStatus.Idle;
            Index = 0;
            _resetTime = 0;
            _totalScore = 0;
            CapableOfMelee = melee;
            CapableOfMagic = magic;
            CapableOfProjectile = range;
            AttackResetTimer = 0.0f;
            TargetPosition = float3.zero;
            TargetEntity = Entity.Null;
            AttackPlans = new FixedList64Bytes<AttackPlan>();
            TargetPositionID = -1;
        }

        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public AIStates Name { get { return AIStates.Terrorize; } }
 
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
    
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
        
        public  FixedList32Bytes<AttackPlan> AttackPlans;
        public float AttackResetTimer;
        [SerializeField]  public bool InAttackCooldown => AttackResetTimer != 0.0f;
        public bool CapableOfMelee, CapableOfMagic,CapableOfProjectile;
    }


    public struct TerrorizeAreaTag : IComponentData {

    }

    public readonly partial struct TerrorizeAreaAspect : IAspect
    {
        private readonly VisionAspect visionAspect;
        private readonly RefRW<TerrorizeAreaState> state;
        private readonly RefRO<LocalTransform> transform;
        private readonly RefRO<AIStat> stats;
        private readonly RefRW<Movement> move;
        private readonly RefRO<AgentBody> agent; 
        
        //Todo Move to AI state to allow for Variability 
        private bool IsHealthy => stats.ValueRO.HealthRatio > .725f;
        private bool IsInDanger => stats.ValueRO.HealthRatio < .35f;
       
        bool InAttackRange(float range)
        {
            if(state.ValueRO.TargetPosition.Equals(float3.zero)) return false;
            var distance = Vector3.Distance(transform.ValueRO.Position, state.ValueRO.TargetPosition);
            return distance <= range;
        }
        
        public void DeterminePlan()
        {
            if(state.ValueRO.AttackPlans.Length!=0)return;
            // select an attack Plan
            int[] scores= new[]
            {
                -1,
                RestScore,
                GetAttackLocation,
                TravelToTargetMeleeLocation,
                TravelToTargetMagicLocation,
                TravelToTargetRangeLocation,
                MeleeScore, MagicScore, RangeScore,
                -1

            };
            var sortedScores = scores.ToList().OrderByDescending(x => x);
            foreach (var score in sortedScores)
            {
                if (score <= 0) continue;
                if(state.ValueRW.AttackPlans.Length>=8)return;
                var index = scores.ToList().IndexOf(score);
                state.ValueRW.AttackPlans.Add((AttackPlan)(index));
            }

        }

        public void ExecutePlan(Entity entity, int chunkIndex, float deltaTime, EntityCommandBuffer.ParallelWriter ECB)
        {
            if(state.ValueRO.AttackPlans.IsEmpty)return;
            switch (state.ValueRO.AttackPlans[0])
            {
                case AttackPlan.None:
                    Debug.LogError("Npc was able to enter Execute Plan with Plan being established");
                    DeterminePlan();
                    break;
                case AttackPlan.Rest:
                    state.ValueRW.AttackResetTimer -= deltaTime;
                    if (state.ValueRO.AttackResetTimer <= 0.0f)
                    {
                        state.ValueRW.AttackResetTimer = 0.0f;
                        state.ValueRW.AttackPlans.RemoveAt(0);
                    }
                    break;
                case AttackPlan.GetAttackLocation:
                    break;
                case AttackPlan.MoveToLocationMelee:
                case AttackPlan.MoveToLocationMagic:
                case AttackPlan.MoveToLocationRange:
                    if(!move.ValueRO.TargetLocation.Equals(state.ValueRO.TargetPosition) && !state.ValueRO.TargetPosition.Equals(float3.zero))
                        move.ValueRW.SetLocation(state.ValueRO.TargetPosition);
                    if(agent.ValueRO.RemainingDistance<5)
                        state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMelee:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 5; //Todo make a variable based off attack and difficulty 
                    ECB.AddComponent<SelectAndAttack>(chunkIndex, entity);
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMagic:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 8;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackRange:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 8;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
            }
        }

        private int RestScore {
            get
            {
                var temp = 0;
                if (IsInDanger) return temp;
                if(state.ValueRO.InAttackCooldown)
                    temp = 3;
                return temp; 
            }
        }
        private int GetAttackLocation => state.ValueRO.TargetPosition.Equals(float3.zero) ? 10 : 0;
        private int TravelToTargetMeleeLocation {
            get
            {
                if (!state.ValueRO.CapableOfMelee) return 0;
                if (InAttackRange(6)) return 0;
                var temp = 1;
                if (IsInDanger) return temp;
                temp++;
                if (IsHealthy)
                    temp++;
                return temp;

            }
        }

        private int TravelToTargetMagicLocation {
            get
            {
                if (!state.ValueRO.CapableOfMagic||InAttackRange(10)) return 0;
                var temp = 3;
                if (IsInDanger)
                {
                    if (state.ValueRO.CapableOfMelee)
                        temp++;
                    else
                    {
                        return temp;
                    }
                }

                if (IsHealthy)
                    temp++;
                return temp;
            }
        }

        private int TravelToTargetRangeLocation {
            get
            {
                if (!state.ValueRO.CapableOfProjectile) return 0;
                var temp = 2;
                if (IsInDanger)
                {
                    if (state.ValueRO.CapableOfMelee)
                        temp++;
                    else
                    {
                        return temp;
                    }
                }
                if (IsHealthy)
                    temp++;
                return temp;
            }
        }
        private int MeleeScore
        {
            get
            {
                if (state.ValueRO.InAttackCooldown) return 0;
                if (!state.ValueRO.CapableOfMelee) return 0;
                var temp = 2;
                if (!InAttackRange(3)) return temp;
                temp++;
                return temp;
            }
        }

        private int MagicScore
        {
            get
            {
                if (state.ValueRO.InAttackCooldown) return 0;
                if (!state.ValueRO.CapableOfMagic) return 0;
                var temp = 2;
                if (!InAttackRange(10)) return temp;
                temp++;
                return temp;
            }
        }

        private int RangeScore
        {
            get
            {
                if (state.ValueRO.InAttackCooldown) return 0;
                if (!state.ValueRO.CapableOfProjectile) return 0;
                var temp = 2;
                if (!InAttackRange(30)) return temp;
                temp++;
                return temp;
            }
        }
  
    }
}