using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IAUS.Actions;

using Unity.Entities;


namespace IAUS
{
    public class UtilitySystem : MonoBehaviour
    {
        public List<ActionBase> Actions;
        public WorldContext World;
        public CharacterContext Agent;
        int CurActionIndex;
        List<Transform> Waypoints { get { return Agent.Waypoints; } }
        int WaypointIndex = 0;
        // Start is called before the first frame update
        void Start()
        {
            World = WorldContext.World;
            Agent.Target = Waypoints[0];
            Actions = new List<ActionBase>();
            // Response Variables need to be passed through to action to be set in Utility
            Actions.Add(new MoveToTargetLocation { NameId="Patrol the Area", Agent=Agent});
            Actions.Add(new WaitAtPoint {NameId = " Wait at spot Look around", Agent=Agent, IntervalOffset = interval });
            Actions.Add(new MoveToPlayer { NameId = "MoveToPlayer", Agent = Agent });
            SetupActions();
        }
        public void ScoreAndSort() {

            if (!Agent.Moving && !Agent.Waiting)
            {
                if (WaypointIndex < Waypoints.Count - 1)
                    WaypointIndex++;
                else WaypointIndex = 0;
                Agent.Target = Waypoints[WaypointIndex];
            }

            foreach (ActionBase Action in Actions) {
                Action.Score();
                if (Action.actionStatus != ActionStatus.Running) {
                    Action.CooldownTimer -= Time.deltaTime * interval;
                }
               // Debug.Log(Action.TotalScore + "  " + Action.NameId);
            }
            Actions[2].TotalScore = 0.0f;

            int index = Actions.FindIndex(a => a.TotalScore== Actions.Max(b=>b.TotalScore));
            // Actions.Sort((a,b)=>a.TotalScore.CompareTo(b.TotalScore));

            //   Debug.Log(Actions[Actions.Count-1].NameId+ Actions[Actions.Count - 1].TotalScore);
            if (index != CurActionIndex) {
                Actions[CurActionIndex].actionStatus = ActionStatus.Idle;
                Actions[index].OnStart();
                if (Actions[CurActionIndex].actionStatus != ActionStatus.Interrupted)
                { Actions[CurActionIndex].OnExit(); }
                else
                { Actions[CurActionIndex].actionStatus=ActionStatus.Idle; }

                CurActionIndex = index;
            }


            Actions[index].Execute();


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
            //if (Time.frameCount % interval == 1)
            //{
            //    ScoreAndSort();
            //}

        }

    }


    public class ECSUtilitySystem : ComponentSystem
    {
        int interval = 60;
        protected override void OnUpdate()
        {

            Entities.ForEach((UtilitySystem US) =>
            {
                if (UnityEngine.Time.frameCount % interval == 1)
                {
                    US.ScoreAndSort();
                }


            });
        }


    }


}