using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DreamersInc
{
    public class GameMasterAuthoring : MonoBehaviour
    {
        public ControllerScheme controller;


        public class Baking : Baker<GameMasterAuthoring>
        {
            public override void Bake(GameMasterAuthoring authoring)
            {
                var data = new ControllerInfo();
                data.setup(authoring.controller);
                AddComponent(data);
            }
        }
    }
}
