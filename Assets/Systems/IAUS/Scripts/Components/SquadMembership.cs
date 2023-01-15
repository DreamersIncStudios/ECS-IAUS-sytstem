using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.NPCScriptableObj;
namespace IAUS.ECS.Component
{
    [GenerateAuthoringComponent]
    public struct SquadMembership : IComponentData
    {
        public int SquadID { get; private set; }

        public void SetSquadID(int IDNo) {
            SquadID = IDNo;
        }
    }

    public struct Squad {
        public EnemyNPCSO Leader;
        public List<GruntSpawner> Grunts;

        public void SpawnSquad(Vector3 pos, int SquadID)
        {
            Leader.Spawn(pos, SquadID);
            foreach (var grunt in Grunts)
            {
                for (int i = 0; i < grunt.NoOfGrunts; i++)
                {
                    grunt.Grunt.Spawn(pos, SquadID);
                }
            }
        }
    }

    public struct GruntSpawner {
        public int NoOfGrunts;
        public EnemyNPCSO Grunt;
    }
}