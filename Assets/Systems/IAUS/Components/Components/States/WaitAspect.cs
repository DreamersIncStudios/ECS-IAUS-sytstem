using IAUS.ECS.Component;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public readonly partial struct WaitAspect : IAspect
{
    readonly RefRW<Wait> wait;
    readonly RefRO<AIStat> stat;

    public float Score
    {
        get
        {
            float TotalScore = wait.ValueRO.TimeLeft.Output(wait.ValueRO.TimePercent) * wait.ValueRO.HealthRatio.Output(stat.ValueRO.HealthRatio);
            wait.ValueRW.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * wait.ValueRO.mod) * TotalScore);
            TotalScore = wait.ValueRW.TotalScore; 
            return TotalScore;
        }
    }

    public ActionStatus Status { get => wait.ValueRO.Status; }
}
