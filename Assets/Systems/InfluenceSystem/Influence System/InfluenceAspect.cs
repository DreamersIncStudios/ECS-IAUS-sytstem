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
        readonly TransformAspect Transform;
        readonly RefRO<InfluenceComponent> influence;



        Faction faction => LoveHate.GetFaction(influence.ValueRO.factionID);
        public float ThreatAtLocation
        {
            get
            {
                return InfluenceGridMaster.Instance.grid.GetGridObject(Transform.WorldPosition).GetValue(faction).y;
            }
        }
        public float ProtectionAtLocation
        {
            get { return InfluenceGridMaster.Instance.grid.GetGridObject(Transform.WorldPosition).GetValue(faction).x; }
        }
    }
}