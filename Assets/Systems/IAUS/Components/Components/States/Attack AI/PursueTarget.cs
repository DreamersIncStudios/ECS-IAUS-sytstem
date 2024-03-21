using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct PursueTarget : IBaseStateScorer
    {
        public PursueTarget(float coolDownTime)
        {
            this.coolDownTime = coolDownTime;
            status = ActionStatus.Idle;
            Index = 0;
            resetTime = 0;
            totalScore = 0;
            TargetArea = new float3();
            TargetEntity = Entity.Null;
        }

        public AIStates Name => AIStates.ChaseMoveToTarget;
        public Entity TargetEntity;
        public float3 TargetArea;
        public float TotalScore { get => totalScore;
            set => totalScore = value;
        }
        public ActionStatus Status { get => status;
            set => status = value;
        }
        public float CoolDownTime => coolDownTime;
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get => resetTime;
            set => resetTime = value;
        }
        public float mod => 1.0f - (1.0f / 3.0f);
        public int Index { get; private set; }
        
        float coolDownTime;
        float resetTime { get; set; }
        float totalScore { get; set; }
        ActionStatus status;
        public void SetIndex(int index)
        {
            Index = index;
        }
    }

    public struct ChaseTargetTag : IComponentData
    {
    }
}