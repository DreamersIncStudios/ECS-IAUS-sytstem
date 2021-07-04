using InfluenceSystem.Component;
using IAUS.ECS2.Component;
using System.Collections.Generic;
namespace IAUS.NPCSO.Interfaces
{
    public interface IEnemyNPC
    {
        bool IsPartOfTeam { get; }
        NPCLevel GetNPCLevel { get; }
        List<AttackTypeInfo> GetAttackType {get;}
        AttackTargetState GetAttackTargetState { get; }

    }
 
}