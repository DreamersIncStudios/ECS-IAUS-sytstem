
namespace IAUS.ECS
{
    public enum AIStates
    {

        None, 
        Patrol, 
        Heal_Self_Item, // combine this with heal Magic to make Heal
        Heal_Magic, 
        Attack, 
        Retreat, 
        FindCover, 
        Talk, 
        Guard, 
        GroupUp, 
        Wait, 
        GotoLeader,
        InvestigateArea, 
        SearchArea, 
        RetreatToLocation, //Todo remove
        RetreatToQuadrant, //Todo remove
        FollowTarget, 
        ChaseMoveToTarget,
        Traverse, 
        GatherResources,
        CallBackUp,
        Terrorize,
        WanderQuadrant, 
        PerformMaintenance,


    }
}