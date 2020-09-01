using UnityEngine;
using Unity.Entities;
using IAUS.ECS2.BackGround.Raycasting;
using InfluenceMap.Factions;

namespace IAUS.ECS2.BackGround
{
    public class UpdateRacastTargetPoints : ComponentSystem
    {
        protected override void OnUpdate()
        {

            Entities.ForEach(( Animator Anim, ref Attackable attackable, ref HumanRayCastPoints humanRayCast) => {
                humanRayCast.Head = Anim.GetBoneTransform(HumanBodyBones.Head).position;
                humanRayCast.Chest = Anim.GetBoneTransform(HumanBodyBones.Chest).position;
                humanRayCast.Left_Arm = Anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
                humanRayCast.Right_Arm = Anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
                humanRayCast.Left_Leg = Anim.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position;
                humanRayCast.Right_Leg = Anim.GetBoneTransform(HumanBodyBones.RightLowerLeg).position;


            });
        }
    }
}