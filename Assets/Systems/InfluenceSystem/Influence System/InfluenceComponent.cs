using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using PixelCrushers.LoveHate;

namespace DreamersInc.InflunceMapSystem
{

    [System.Serializable]
    public struct InfluenceComponent : IComponentData
    {
        public int Threat;
        public int Protection;
        public int2 GetInfluenceValue { get { return new int2(Threat, Protection); } }
        public int2 GetInfluenceValueMod(float mod) {
            return new int2(Mathf.RoundToInt( mod*Threat), Mathf.RoundToInt(mod* Protection));
        }
        public float3 previousPos;
        public int factionID;
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



    //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    //public  partial struct UpdateInfluenceGridSystem : ISystem
    //{
    //    public void OnCreate(ref SystemState state)
    //    {
    //    }

    //    public void OnDestroy(ref SystemState state)
    //    {
    //    }

    //    public void OnUpdate(ref SystemState state)
    //    {
    //        state.Dependency = new UpdateGridJob().Schedule(state.Dependency);
    //    }
        
    //    public partial struct UpdateGridJob : IJobEntity
    //    {

    //        public void Execute(ref InfluenceComponent influence, in LocalTransform toWorlds, in Perceptibility perceptibility)
    //        {
    //            if (influence.GridChanged(toWorlds.Position, out InfluenceGridObject gridpoint) && !influence.NPCOffGrid(toWorlds.Position))
    //            {

    //                InfluenceGridMaster.Instance.grid.GetGridObject(influence.previousPos)?.AddValue(-influence.GetInfluenceValueMod(perceptibility.Score), 10, 25, LoveHate.factionDatabase.GetFaction(influence.factionID));
    //                gridpoint.AddValue(influence.GetInfluenceValueMod(perceptibility.Score), 10, 25, LoveHate.factionDatabase.GetFaction(influence.factionID));
    //                influence.previousPos = toWorlds.Position;

    //            }
    //            else if (influence.NPCOffGrid(toWorlds.Position))
    //            {
    //                InfluenceGridMaster.Instance.grid.GetGridObject(influence.previousPos)?.AddValue(-influence.GetInfluenceValueMod(perceptibility.Score), 10, 25, LoveHate.factionDatabase.GetFaction(influence.factionID));
    //                influence.previousPos = toWorlds.Position;

    //            }
    //        }

            
    //    }
    //}
}