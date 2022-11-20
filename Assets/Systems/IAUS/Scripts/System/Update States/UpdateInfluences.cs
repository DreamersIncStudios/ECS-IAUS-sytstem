using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Component;
using DreamersInc.InflunceMapSystem;
using PixelCrushers.LoveHate;
using Unity.Transforms;

namespace IAUS.ECS.Systems
{
    public class UpdateInfluences : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref RetreatCitizen citizen, ref LocalToWorld toWorld) => { 
                citizen.GridValueAtPos =  InfluenceGridMaster.Instance.grid.GetGridObject(toWorld.Position).GetValue(LoveHate.factionDatabase.GetFaction(citizen.FactionMemberID));
                InfluenceGridMaster.Instance.grid.GetGridObject(toWorld.Position).GetHighestThreatCell(LoveHate.factionDatabase.GetFaction(citizen.FactionMemberID), true, out int x, out int y);
                citizen.LocationOfHighestThreat = InfluenceGridMaster.Instance.grid.GetWorldPosition(x, y);

                InfluenceGridMaster.Instance.grid.GetGridObject(toWorld.Position).GetLowestThreatCell(LoveHate.factionDatabase.GetFaction(citizen.FactionMemberID), true, out x, out y);
                citizen.LocationOfLowestThreat = InfluenceGridMaster.Instance.grid.GetWorldPosition(x, y);

            });
            Entities.ForEach((ref TerrorizeAreaState terrorize) => { });

        }


    }
}

