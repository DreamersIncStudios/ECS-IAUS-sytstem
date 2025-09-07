using DreamersInc;
using DreamersIncStudio.FactionSystem;
using DreamersIncStudio.FactionSystem.Authoring;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AISenses.VisionSystems
{
    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    [UpdateAfter(typeof(TargetingQuadrantSystem))]
    public partial class CheckTargetThreatSystem : SystemBase
    {
    
        Entity factionSingleton;
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<FactionSingleton>();
      

        }
        protected override void OnUpdate()
        {
           var  factionsBuffer = SystemAPI.GetSingletonBuffer<Factions>();
          Dependency = Entities.ForEach((DynamicBuffer<ScanPositionBuffer> buffer, ref IAUSBrain brain) =>
           {
               for (int i = 0; i < buffer.Length; i++)
               {
                   var temp = buffer[i];
                   var relationships = default(FixedList512Bytes<Relationship>);
                   foreach (var factions in factionsBuffer)
                   {
                       if (factions.Faction != brain.FactionID) continue;
                       relationships = factions.Relationships;
                       break;
                   }

                   foreach (var relationship in relationships)
                   {
                       if (relationship.Faction != temp.target.TargetInfo.FactionID) continue;
                       temp.target.Affinity = relationship.Affinity switch
                       {
                           < -75 => Affinity.Hate,
                           > -75 and < -35 => Affinity.Negative,
                           > -35 and < 35 => Affinity.Neutral,
                           > 35 and < 74 => Affinity.Positive,
                           > 75 => Affinity.Love,
                           _ => Affinity.Neutral
                       };
                   }

                   buffer[i] = temp;
               }

           }).Schedule(Dependency);

           Entities.WithAll<Player_Control>().ForEach((DynamicBuffer<ScanPositionBuffer> buffer) =>
           {
               for (int i = 0; i < buffer.Length; i++)
               {
                   var temp = buffer[i];
                   var relationships = default(FixedList512Bytes<Relationship>);
                   foreach (var factions in factionsBuffer)
                   {
                       if (factions.Faction != FactionNames.Player) continue;
                       relationships = factions.Relationships;
                       break;
                   }

                   foreach (var relationship in relationships)
                   {
                       if (relationship.Faction != temp.target.TargetInfo.FactionID) continue;
                       temp.target.Affinity = relationship.Affinity switch
                       {
                           < -75 => Affinity.Hate,
                           > -75 and < -35 => Affinity.Negative,
                           > -35 and < 35 => Affinity.Neutral,
                           > 35 and < 74 => Affinity.Positive,
                           > 75 => Affinity.Love,
                           _ => Affinity.Neutral
                       };
                   }

                   buffer[i] = temp;
               }

           }).Schedule();
        }
        

    }
}