using DreamersInc.InflunceMapSystem;


public interface IInfluence
{
    public InfluenceComponent GetInfluence { get; }
    public Faction GetFaction { get; }
}
