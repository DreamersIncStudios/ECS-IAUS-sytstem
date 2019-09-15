using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IAUS.Actions;


namespace IAUS
{
    public class UtilitySystem : MonoBehaviour
    {
        public List<ActionBase> Actions;
        public WorldContext World;
        public CharacterContext Agent;
        // Start is called before the first frame update
        void Start()
        {
            World = WorldContext.World;
            Actions = new List<ActionBase>();
            // Response Variables need to be passed through to action to be set in Utility
            Actions.Add(new PatrolArea { NameId="Patrol the Area", Agent=Agent});
            Actions.Add(new WaitAtPoint {NameId = " Wait at spot Look around", Agent=Agent, IntervalOffset = interval });
            Actions.Add(new MoveToPlayer { NameId = "MoveToPlayer", Agent = Agent });
            SetupActions();
        }
        void ScoreAndSort() {
            foreach (ActionBase Action in Actions) {
                Action.Score();
               // Debug.Log(Action.TotalScore + "  " + Action.NameId);
            }
            Actions[2].TotalScore = 0.0f;
            Actions.Sort((a,b)=>a.TotalScore.CompareTo(b.TotalScore));

         //   Debug.Log(Actions[Actions.Count-1].NameId+ Actions[Actions.Count - 1].TotalScore);
            Actions[Actions.Count - 1].Excute();


        }
        void SetupActions() {
            foreach (ActionBase Action in Actions) {
                Action.Setup();
            }
              //  Actions.Sort((a, b) => a.TotalScore.CompareTo(b.TotalScore));

        }

        // Update is called once per frame
        int interval= 60;
        void Update()
        {
            if (Time.frameCount % interval == 1)
            {
                ScoreAndSort();
            }

        }

    }
}