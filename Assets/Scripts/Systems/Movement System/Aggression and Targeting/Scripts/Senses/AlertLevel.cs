using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace AISenses
{
    public struct AlertLevel : IComponentData
    {
        // rewrite so that Alert and Caution can't exceed 100
        public int Alert => AudioAlertLevel + VisualAlertLevel;
       [SerializeField] public int Caution => AudioCautionLevel + VisualCautionLevel;

        public ReactionType ReactToWhat;
        
        public int AudioCautionLevel; // used for Retreat/Run away from 
        public int AudioAlertLevel; // used for Retreat/Run away from 
        public int VisualCautionLevel; // used for Retreat/Run away from 
        public int VisualAlertLevel; // used for Retreat/Run away from 
        public bool AudioOrVisual => (AudioCautionLevel + AudioAlertLevel) >= (VisualCautionLevel + VisualAlertLevel);
        public bool NeedForAlarm => Alert > NoticeThreshold || Caution > NoticeThreshold;
        public int NoticeThreshold => 50; // based off awareness stats later

        public float3 LocationOfThreat;


    }

    public enum ReactionType { 
        none, Audio, Visual, Impact
    }
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(HearingSystem))]
    public class UpdateAlert : SystemBase
    {
        private EntityQuery AlertQuery;
        protected override void OnCreate()
        {
            AlertQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AlertLevel)), ComponentType.ReadOnly(typeof(Vision)), ComponentType.ReadOnly(typeof(Hearing)) }
            });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new RaiseTheAlarmFor() {
                AlertChunk = GetComponentTypeHandle<AlertLevel>(false),
                hearingChunk = GetComponentTypeHandle<Hearing>(true),
                VisionChunk = GetComponentTypeHandle<Vision>(true)
            }.ScheduleParallel(AlertQuery, systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        public struct RaiseTheAlarmFor : IJobChunk
        {
            public ComponentTypeHandle<AlertLevel> AlertChunk;
            [ReadOnly] public ComponentTypeHandle<Hearing> hearingChunk;
            [ReadOnly] public ComponentTypeHandle<Vision> VisionChunk;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AlertLevel> Alerts = chunk.GetNativeArray(AlertChunk);
                NativeArray<Hearing> Hearings = chunk.GetNativeArray(hearingChunk);
                NativeArray<Vision> Visions = chunk.GetNativeArray(VisionChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AlertLevel alert = Alerts[i];

                    if (alert.NeedForAlarm)
                    {
                        if (alert.AudioOrVisual)
                        {
                            alert.ReactToWhat = ReactionType.Audio;
                            alert.LocationOfThreat = Hearings[i].LocationOfSound;
                         
                        }
                        else
                        {
                            alert.ReactToWhat = ReactionType.Visual;
                            alert.LocationOfThreat = Visions[i].ThreatPosition;
                        }
                    }
                    else
                    {
                        alert.LocationOfThreat = float3.zero;
                        alert.ReactToWhat = ReactionType.none;
                    }


                        Alerts[i] = alert;
                }

            }
        }



    }

}
