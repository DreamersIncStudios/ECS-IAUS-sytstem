using InfluenceMap.Factions;

namespace IAUS.SpawnerSystem.interfaces { 
    public interface iFaction {
        Attackable FactionData { get; }
        bool AbleToAttack { get; }  // if false, NPC can not attack threats but will runaway or signal others who can 

    
    }

}
