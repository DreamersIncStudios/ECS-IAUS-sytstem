using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS2;

namespace SpawnerSystem.ScriptableObjects
{
    public class LeaderComponent : MonoBehaviour, IConvertGameObjectToEntity
    {
        public GameObject BackupLeader;
        public List<SquadEntityAdder> Squad;
        public bool Updated;
        public Entity Self { get { return selfRef; } }
        Entity selfRef;
        public LeaderComponent() { }
        public LeaderComponent(GameObject Back, List<SquadEntityAdder> squad)
        {
            BackupLeader = Back;
            Squad = squad;
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var leader = new LeaderTag() { };

            dstManager.AddComponentData(entity, leader);
            DynamicBuffer<SquadMemberBuffer> buffer = dstManager.AddBuffer<SquadMemberBuffer>(entity);

            selfRef = entity;
        }


    }

    public class SquadMember : MonoBehaviour, IConvertGameObjectToEntity
    {
        public Entity Self { get { return selfRef; } }
        Entity selfRef;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            selfRef = entity;

        }
    }
    public struct LeaderTag : IComponentData
    {
        public Entity BackupLeader;

    }

    // all npc need a reference to self entity for squad grouping


    public struct SquadMemberBuffer : IBufferElementData
    {
        public Entity SquadMember;

    }
    [System.Serializable]
    public struct SquadEntityAdder {
        public GameObject GO;
        public bool Added;
    }
    public struct AddSquadTag : IComponentData { };
   public class SquadUP : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((  ref LeaderTag Tag ,LeaderComponent Leader ) => {

                if(Leader.BackupLeader !=null)
                Tag.BackupLeader = Leader.BackupLeader.GetComponent<SquadMember>().Self;
            
            });
            ComponentDataFromEntity<FollowCharacter> follow = GetComponentDataFromEntity<FollowCharacter>(false);
                // add leader ref to follow character command
            Entities.ForEach((DynamicBuffer<SquadMemberBuffer> Buffer, LeaderComponent Leader) => {

                if (!Leader.Updated) {
                    for (int i = 0; i < Leader.Squad.Count; i++)
                    {
                        SquadEntityAdder Adder = Leader.Squad[i];
                        if (!Adder.Added)
                        {
                            SquadMemberBuffer temp = new SquadMemberBuffer()
                            {
                                SquadMember = Adder.GO.GetComponent<SquadMember>().Self
                            };
                            Buffer.Add(temp);
                            Adder.Added = true;
                            Leader.Squad[i] = Adder;
                            FollowCharacter tempFollow = follow[Adder.GO.GetComponent<SquadMember>().Self];
                            tempFollow.Target = Leader.Self;
                            follow[Adder.GO.GetComponent<SquadMember>().Self] = tempFollow;
                        }
                    }
                    Leader.Updated = true;
                }
             
            });
        }
    }
}

