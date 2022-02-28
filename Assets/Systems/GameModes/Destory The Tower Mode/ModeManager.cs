using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using DreamersIncStudios.MoonShot;
using GameModes.DestroyTheTower.TowerSystem;
using System.Timers;
using GameModes.DestroyTheTower.TimerSystems;
using TMPro;
using UnityEngine.Events;
namespace GameModes.DestroyTheTower
{
    public class ModeManager : MonoBehaviour
    {
        public UnityEvent OnRoundStart, OnRoundEnd, OnWin, OnLose;


        public List<RoundSettings> Rounds;
        public TowerManager TM;
        public int CurrentRound{get; private set;}
        public float TimeLeftInRound { get; private set; }
        public bool InPlay;
        public PlayerCharacter StartingCharacter { get; private set; }
        public PlayerCharacter CharacterReference;

        // Start is called before the first frame update
        void Start()
        {
            CharacterReference = GameObject.FindObjectOfType<PlayerCharacter>();
            TM.Parent = this.transform;
            CurrentRound = 0; // TODO pull from Save File later
            SetupRound(CurrentRound);
        }

        // Update is called once per frame
        void Update()
        {
            TimerDisplay.text = RoundTimer.DisplayTimeLeft;

            TimerDisplay.ForceMeshUpdate(true);
        }
        TimerCountDown RoundTimer;
        public GameObject TimerUI;
        [SerializeField] public TextMeshProUGUI TimerDisplay;//=> TimerUI.GetComponent<TextMeshProUGUI>();
        public void SetupRound(int index) {
            InPlay = false;
            CurrentRound = index;
           RoundTimer = new TimerCountDown(Rounds[index].TimeLimit,TimerDisplay);
            TimeLeftInRound = Rounds[index].TimeLimit;
             CopyPlayerAtStartOfRound();
            TM.SpawnTower(Rounds[index].Level, Rounds[index].NumberOfTowers);
            GameMaster.Instance.State = GameStates.WaitingToStartLevel;

        }
        public void StartRound() {
            if (!InPlay)
            {
                InPlay = true;
                GameMaster.Instance.State = GameStates.Playing;
                RoundTimer.StartCountdown();
                OnRoundStart.Invoke();
            }
        }

        public void ResetAndStartRound()
        {
            foreach (GameObject tower in TM.TowersInScene) {
                // TODO Tower class to destory all NPC is created
                Object.Destroy(tower);
            }
            ResetPlayer();
            RoundTimer.Stop();
            SetupRound(CurrentRound);
        }
        public void RoundWon() {
            CurrentRound++;
        }
        public void GameOver() { 
            //TODO Fade to black 
            GameMaster.Instance.State = GameStates.Game_Over;
        }
        SaveDataDTT data;
        void CopyPlayerAtStartOfRound() {
            data = new SaveDataDTT(CharacterReference);
        }
        void ResetPlayer() {
            CharacterReference = StartingCharacter;
            //TODO Add inventory cloning
            CharacterReference.StatUpdate();
        }



        private void OnValidate()
        {
            if (Rounds.Count != 0)
            {
                for (int i = 0; i < Rounds.Count; i++)
                {
                    Rounds[i].Level = i + 1;
                }
            }
        }

    }
    [System.Serializable]
    public class SaveDataDTT {
        List<Attributes> PlayerAttributes;
        List<Vital> PlayerVitals;
        int PlayerLevel;

        //TODO add Inventory and Equipment 
        public SaveDataDTT(PlayerCharacter player) {
            PlayerLevel = player.Level;
            PlayerAttributes = player.GetAttributes();
            PlayerVitals = player.GetVitals();
        }
    }

}