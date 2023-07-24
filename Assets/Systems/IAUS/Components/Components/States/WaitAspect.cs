using AISenses.VisionSystems;
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
    readonly VisionAspect VisionAspect;


    float TravelInFiveSec
    {
        get
        {
            return 5 * 10; // TODO change to stat dependent statInfo.ValueRO.Speed * 5;   
        }
    }
    bool targetInRange
    {
        get
        {
            if (VisionAspect.TargetInRange(out _, out float dist))
            {
                return dist < TravelInFiveSec;
            }
            else { return false; }
        }
    }
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
            TotalScore = !targetInRange && wait.ValueRO.Status != ActionStatus.CoolDown ? wait.ValueRW.TotalScore :0.0f; 
            return TotalScore;
        }
    }

    public ActionStatus Status { get => wait.ValueRO.Status; }
}
