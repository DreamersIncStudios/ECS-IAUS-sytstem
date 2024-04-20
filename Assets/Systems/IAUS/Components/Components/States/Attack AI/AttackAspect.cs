using System;
using AISenses.VisionSystems;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public readonly partial struct AttackAspect : IAspect
    {
        private readonly VisionAspect visionAspect;
        private readonly RefRO<MapVision> mapVision;
        private readonly RefRW<AttackState> state;
        private readonly RefRO<LocalTransform> transform;

        public void DeterminePlan()
        {
            if (state.ValueRO.Plan != AttackPlan.None)
            {
                // Is Plan still valid?
                switch (state.ValueRO.Plan)
                {
                    case AttackPlan.Rest:
                        break;
                    case AttackPlan.MoveToLocation:
                        break;
                    case AttackPlan.Attack:
                        break;
                    case AttackPlan.Evade:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                // select an attack Plan
            }
        }
        

        public int MeleeScore
        {
            get
            {
                if (state.ValueRO.InCooldown) return 0;
                if (!state.ValueRO.CapableOfMelee) return 0;
                var temp = 2;
                if (!InAttackRange(3)) return temp;
                temp++;
                return temp;
            }
        }
        public int MagicScore
        {
            get
            {
                if (state.ValueRO.InCooldown) return 0;
                if (!state.ValueRO.CapableOfMagic) return 0;
                var temp = 2;
                if (!InAttackRange(10)) return temp;
                temp++;
                return temp;
            }
        }
        
        public int RangeScore
        {
            get
            {
                if (state.ValueRO.InCooldown) return 0;
                if (!state.ValueRO.CapableOfProjectile) return 0;
                var temp = 2;
                if (!InAttackRange(30)) return temp;
                temp++;
                return temp;
            }
        }
        
        bool InAttackRange(float range)
        {
            return visionAspect.TargetEnemyTargetInRange(out _, out float dist) && dist <= range;
        }

    }

    public enum AttackPlan
    {
        None,
        Rest,
        MoveToLocation,
        Attack,
        Evade
    }
}
