using PixelCrushers.LoveHate;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DreamersInc.InflunceMapSystem
{
    public readonly partial struct InfluenceAspect : IAspect
    {
        readonly RefRO<LocalTransform> Position;
        readonly RefRO<InfluenceComponent> influence;

        public float DistanceToHighProtection { get { return 10; } }


        Faction faction => LoveHate.GetFaction(influence.ValueRO.factionID);
        public float ThreatAtLocation
        {
            get
            {
                return 1;
              //  return InfluenceGridMaster.Instance.grid.GetGridObject(Transform.WorldPosition).GetValue(faction).y;
            }
        }
        public float ProtectionAtLocation
        {
            get {
                return 10;
                //return InfluenceGridMaster.Instance.grid.GetGridObject(Transform.WorldPosition).GetValue(faction).x;
                }
        }

        public float ThreatRatio
        {
            get
            {
                return ThreatAtLocation / ProtectionAtLocation;
            }

        }
    }
}