using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using SpawnerSystem;
using IAUS.SpawnerSystem.Database;

namespace IAUS.SpawnerSystem
{

    public class IAUSSquadSO : SquadSO
    {
        public override void Spawn(Vector3 Position) 
        {
            GameObject leaderGO = IAUSEnemyDatabase.GetEnemy(LeaderID).Spawn(Position, new List<GameObject>());
            GameObject BackupGO = IAUSEnemyDatabase.GetEnemy(BackupLeaderID).Spawn(Position);
            BackupGO.AddComponent<SquadMember>();
            LeaderComponent test = leaderGO.AddComponent<LeaderComponent>();
            test.BackupLeader = BackupGO;
            test.Squad = new List<SquadEntityAdder>();
            foreach (SquadMemberID squadMember in SquadMemberIDs)
            {
                for (int i = 0; i < squadMember.NumberOfSpawns; i++)
                {
                    GameObject memberGO = IAUSEnemyDatabase.GetEnemy(squadMember.ID).Spawn(Position);
                    memberGO.AddComponent<SquadMember>();
                    test.Squad.Add(new SquadEntityAdder()
                    {
                        GO = memberGO.gameObject,
                        Added = false 
                    });
                }
            }

           
        }
    }


}