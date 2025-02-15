using DreamersIncStudio.FactionSystem;
using DreamersIncStudio.FactionSystem.Authoring;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Entities;

namespace AISenses.VisionSystems
{
    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    [UpdateAfter(typeof(TargetingQuadrantSystem))]
    public partial class CheckTargetThreatSystem : SystemBase
    {
     DynamicBuffer<Factions> factionsBuffer;
  
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
            RequireForUpdate<FactionSingleton>();


        }

        protected override void OnUpdate()
        {
            Entities.ForEach(( DynamicBuffer<ScanPositionBuffer> buffer,ref IAUSBrain brain) =>
            {
                for (int i = 0; i <  buffer.Length; i++)
                {
                    var temp = buffer[i];
                    temp.target.IsFriendly=IsFriendly(brain.FactionID);
                    buffer[i] = temp;
                }
             
            }).WithoutBurst().Run();
        }
        private void RetrieveFactionsData()
        {
            factionsBuffer = SystemAPI.GetSingletonBuffer<Factions>();
 
        }
        private FixedList512Bytes<Relationship> GetRelationship(FactionNames faction)
        {
            foreach (var factions in factionsBuffer)
            {
                if (factions.Faction == faction) return factions.Relationships;
            }
            return default;
        }
        private bool IsFriendly(int factionID)
        {
            var faction = (FactionNames)factionID;
            var relationships = GetRelationship(faction);
            foreach (var relationship in relationships)
            {
                if (relationship.Faction != faction) continue;
                if (relationship.Affinity <= 50) continue;
                return true;
            }
            return false;
        }
    }
}