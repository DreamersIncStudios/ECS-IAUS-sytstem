using System.Collections;
using System.Collections.Generic;
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
            Debug.Log(World.Player.name);

            Actions.Add(new PatrolArea { Agent=Agent.NavAgent,Waypoints=Agent.Waypoints });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}