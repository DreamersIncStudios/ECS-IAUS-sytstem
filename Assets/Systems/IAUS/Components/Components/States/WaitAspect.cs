using IAUS.ECS.Component;
using Stats.Entities;
using System;
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
            if(wait.ValueRO.Index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(wait), $"Please check Creature list and Consideration Data to make sure {wait.ValueRO.name} state is implements");

            }
            float TotalScore = wait.ValueRO.TimeLeft.Output(wait.ValueRO.TimePercent) * wait.ValueRO.HealthRatio.Output(stat.ValueRO.HealthRatio);
            wait.ValueRW.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * wait.ValueRO.mod) * TotalScore);
            TotalScore = wait.ValueRW.TotalScore; 
            return TotalScore;
        }
    }

    public ActionStatus Status { get => wait.ValueRO.Status; }
}
