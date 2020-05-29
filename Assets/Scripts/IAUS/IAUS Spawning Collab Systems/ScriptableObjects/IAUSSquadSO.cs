using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using SpawnerSystem;
namespace IAUS.SpawnerSystem
{

    public class IAUSSquadSO : SquadSO
    {
        public override void Spawn(Vector3 Position) 
        {
            GameObject leaderGO = EnemyDatabase.GetEnemy(LeaderID).Spawn(Position);
            GameObject BackupGO = EnemyDatabase.GetEnemy(BackupLeaderID).Spawn(Position);
            BackupGO.AddComponent<SquadMember>();
            LeaderComponent test = leaderGO.AddComponent<LeaderComponent>();
            test.BackupLeader = BackupGO;
            test.Squad = new List<SquadEntityAdder>();
            foreach (SquadMemberID squadMember in SquadMemberIDs)
            {
                for (int i = 0; i < squadMember.NumberOfSpawns; i++)
                {
                    GameObject memberGO = EnemyDatabase.GetEnemy(squadMember.ID).Spawn(Position);//= Instantiate(EnemyDatabase.GetEnemy(squadMember.ID).GO, Position, Quaternion.identity);
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

    static public class IAUSSquadDatabase
    {
        static public List<IAUSSquadSO> Squads;
        static public bool IsLoaded { get; private set; } = false;

        static private void ValidateDatebase()
        {
            if (Squads == null) Squads = new List<IAUSSquadSO>();
        }

        static public void LoadDatabase()
        {
            if (IsLoaded)
                return;
            LoadDatabaseForce();
        }
        static public void LoadDatabaseForce()
        {
            ValidateDatebase();
            IsLoaded = true;
            IAUSSquadSO[] resources = Resources.LoadAll<IAUSSquadSO>(@"Squad-IAUS");
            foreach (IAUSSquadSO squad in resources)
            {
                if (!Squads.Contains(squad))
                    Squads.Add(squad);
            }
        }

        static public void ClearDatabase()
        {
            IsLoaded = false;
            Squads.Clear();
        }

        static public IAUSSquadSO GetEnemy(int SpawnID)
        {
            ValidateDatebase();
            LoadDatabase();
            foreach (IAUSSquadSO squad in Squads)
            {
                if (squad.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(squad) as IAUSSquadSO;
            }
            return null;
        }
    }
}