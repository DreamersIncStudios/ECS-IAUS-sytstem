using InfluenceSystem.Component;
using Dreamers.SquadSystem;
namespace IAUS.NPCSO.Interfaces
{
    public interface IEnemyNPC
    {
         bool IsPartOfTeam { get; }
        NPCLevel GetNPCLevel { get; }

    }
 
}