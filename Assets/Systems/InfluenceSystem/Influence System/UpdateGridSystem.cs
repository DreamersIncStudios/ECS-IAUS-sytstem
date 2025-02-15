using DreamersInc.InflunceMapSystem;
using DreamersIncStudio.FactionSystem;
using DreamersIncStudio.FactionSystem.Authoring;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DreamersIncStudio.InfluenceMapSystem
{
    public partial class UpdateGridSystem : SystemBase
    {

        public int2 SectorScore(float3 position , FactionNames faction)
        {
            var query = SystemAPI.QueryBuilder().WithAll<InfluenceComponent, LocalToWorld>().Build();
            var positions = query.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            var influences = query.ToComponentDataArray<InfluenceComponent>(Allocator.TempJob);
            var relationships = GetRelationship(faction);
            var sectorMask = int2.zero;
            for (var i = 0; i < positions.Length; i++)
            {
                bool isFriendly = false;
                foreach (var relationship in relationships)
                {
                    if (relationship.Faction != influences[i].FactionID) continue;
                    if (relationship.Affinity <= 50) continue;
                    isFriendly = true;
                    break;
                }

                var dist = Vector3.Distance(positions[i].Position, position);
                var direction = ((Vector3)position - (Vector3)positions[i].Position).normalized;
                if(dist> influences[i].DetectionRadius) continue;
                //Todo add some raycast filter
                int sector = GetSectorForDirection(direction);
                int rangeValue = GetRangeValue(dist, influences[i].DetectionRadius);
                int sectorShift = sector * 4;
                int currentSectorValue = isFriendly? (sectorMask.x >> sectorShift) & 0b1111:(sectorMask.y >> sectorShift) & 0b1111;
                int newSectorValue = Mathf.Min(15, currentSectorValue + rangeValue);
                if (isFriendly)
                {
                    sectorMask.x &= ~(0b1111 << sectorShift);
                    sectorMask.x |= newSectorValue << sectorShift;
                }else
                {
                    sectorMask.y &= ~(0b1111 << sectorShift);
                    sectorMask.y |= newSectorValue << sectorShift;
                }
            }
            return sectorMask;
        }

        private FixedList512Bytes<Relationship> GetRelationship(FactionNames faction)
        {
            foreach (var factions in factionsBuffer)
            {
                if (factions.Faction == faction) return factions.Relationships;
            }
            return default;
        }

        int GetSectorForDirection(Vector3 direction) {
            return Mathf.FloorToInt((Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg + 360) % 360 / 45f);
        }
        int GetRangeValue(float distance, float detectionRadius) {
            if (distance < detectionRadius * 0.5f) return 3;
            if (distance < detectionRadius * 0.75f) return 2;
            return distance <= detectionRadius ? 1 : 0;
        }
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<FactionSingleton>();
            //Todo add running Tag require for update      

        }
        [ReadOnly] DynamicBuffer<Factions> factionsBuffer;
        Entity factionSingleton;

        protected override void OnUpdate()
        {
            RetrieveFactionsData();
        }

        private void RetrieveFactionsData()
        {
            factionsBuffer = SystemAPI.GetSingletonBuffer<Factions>();
        }
    }
       
}