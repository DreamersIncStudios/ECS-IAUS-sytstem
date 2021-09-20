using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;


namespace DreamersInc.InflunceMapSystem
{

    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct InfluenceComponent : IComponentData
    {
        public int Threat;
        public int Protection;
        public int2 GetInfluenceValue { get { return new int2(Threat, Protection); }  }
        public float3 previousPos;
        public Faction faction;
        public bool GridChanged(float3 position, out InfluenceGridObject gridpoint)
        {
            InfluenceGridObject currentInflunceGridObject = InfluenceGridMaster.Instance.grid.GetGridObject(previousPos);
            gridpoint = InfluenceGridMaster.Instance.grid.GetGridObject(position);
            if (gridpoint == null)
            {
                return false;
            }
            return currentInflunceGridObject != gridpoint;
        }
        public bool NPCOffGrid(float3 position)
        {
            return null == InfluenceGridMaster.Instance.grid.GetGridObject(position) && InfluenceGridMaster.Instance.grid.GetGridObject(previousPos) != null;
        }

    }


    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public sealed class UpdateInfluenceGridSystem : SystemBase
    {
        EntityQuery Influencers;
        protected override void OnCreate()
        {
            base.OnCreate();
            Influencers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadWrite(typeof(InfluenceComponent))
            }
            });
            Influencers.SetChangedVersionFilter(new ComponentType[] {
            ComponentType.ReadWrite(typeof(LocalToWorld))
        });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new UpdateGridJob()
            {
                TestChunk = GetComponentTypeHandle<InfluenceComponent>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleSingle(Influencers, systemDeps);
            Dependency = systemDeps;
        }
        public struct UpdateGridJob : IJobChunk
        {
            public ComponentTypeHandle<InfluenceComponent> TestChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {

                NativeArray<InfluenceComponent> testInfluences = chunk.GetNativeArray(TestChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    InfluenceComponent influence = testInfluences[i];
                    if (influence.GridChanged(toWorlds[i].Position, out InfluenceGridObject gridpoint) && !influence.NPCOffGrid(toWorlds[i].Position))
                    {
                        InfluenceGridMaster.Instance.grid.GetGridObject(influence.previousPos)?.AddValue( -influence.GetInfluenceValue, 10, 25, influence.faction);
                        gridpoint.AddValue( influence.GetInfluenceValue, 10, 25, influence.faction);
                        influence.previousPos = toWorlds[i].Position;

                    }
                    else if (influence.NPCOffGrid(toWorlds[i].Position))
                    {
                        InfluenceGridMaster.Instance.grid.GetGridObject(influence.previousPos)?.AddValue( -influence.GetInfluenceValue, 10, 25, influence.faction);
                        influence.previousPos = toWorlds[i].Position;

                    }
                    testInfluences[i] = influence;
                }

            }
        }
    }
}