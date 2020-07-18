using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpawnerSystem.ScriptableObjects
{
    public  class SquadSO : ScriptableObject,ISpawnable, iSquad
    {

        [SerializeField] int _spawnID;
        [SerializeField] int leaderID;
        [SerializeField] int backupLeaderID;
        [SerializeField] List<SquadMemberID> squadMemberID;
        public int SpawnID { get { return _spawnID; } set { _spawnID = value; } }

        public Vector3 SpawnOffset { get { return Vector3.zero; } }

        public int LeaderID { get { return leaderID; }  }

        public int BackupLeaderID { get { return backupLeaderID; } }

        public List<SquadMemberID> SquadMemberIDs { get { return squadMemberID; }}

        public void Spawn(Vector3 Position) {
            GameObject leaderGO = EnemyDatabase.GetEnemy(LeaderID).Spawn( Position);
            GameObject BackupGO = EnemyDatabase.GetEnemy(BackupLeaderID).Spawn( Position);
            BackupGO.AddComponent<SquadMember>();
            LeaderComponent test = leaderGO.AddComponent<LeaderComponent>();
            test.BackupLeader = BackupGO;
            test.Squad = new List<SquadEntityAdder>();
            foreach (SquadMemberID squadMember in SquadMemberIDs)
            {
                for (int i = 0; i < squadMember.NumberOfSpawns; i++)
                {
                  GameObject  memberGO = EnemyDatabase.GetEnemy(squadMember.ID).Spawn(Position);//= Instantiate(EnemyDatabase.GetEnemy(squadMember.ID).GO, Position, Quaternion.identity);
                    memberGO.AddComponent<SquadMember>();
                    test.Squad.Add(new SquadEntityAdder()
                    {
                        GO = memberGO.gameObject,
                        Added = false
                    } );
                }
            }
        }
    }

    public interface iSquad
    {
        int LeaderID { get;}
        int BackupLeaderID { get;}
        List<SquadMemberID> SquadMemberIDs { get; }
    }
    [System.Serializable]

    // pass a list of squad gameobjects. Write a componentsystem or Ijob or method to get entities 
    public struct SquadMemberID{
        public int ID;
        public int NumberOfSpawns;
    }

}