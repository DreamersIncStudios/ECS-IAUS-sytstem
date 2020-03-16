using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace IAUS.ECS2.BackGround.Raycasting
{
    [BurstCompile]
    public struct SetupHumanRayCast : IJob
    {
        public LocalToWorld AgentPos;
        public Detection detect;
        public HumanRayCastPoints humanRaysTargets;
        public NativeList<RaycastCommand> RaysToSetup;
        public void Execute()
        {
            //Chest Ray
            float dist = Vector3.Distance(humanRaysTargets.Chest, AgentPos.Position);
            Vector3 DirTo = ((Vector3)humanRaysTargets.Chest - (Vector3)AgentPos.Position).normalized;
            RaycastCommand temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Head Ray
            dist = Vector3.Distance(humanRaysTargets.Head, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Head - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Right arm Ray
            dist = Vector3.Distance(humanRaysTargets.Right_Arm, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Right_Arm - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Left arm Ray
            dist = Vector3.Distance(humanRaysTargets.Left_Arm, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Left_Arm - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Righ LegRay
            dist = Vector3.Distance(humanRaysTargets.Right_Leg, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Right_Leg - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Left leg Ray
            dist = Vector3.Distance(humanRaysTargets.Left_Leg, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Left_Leg - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
        }

    }

    public struct SetupStuctureRayCast : IJob
    {
            
        public LocalToWorld AgentPos;
        public Detection detect;
        public NativeList<RaycastCommand> RaysToSetup;
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

    public struct SetupCreatureRayCast : IJob
    {
        public LocalToWorld AgentPos;
        public Detection detect;
        public NativeList<RaycastCommand> RaysToSetup;
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}