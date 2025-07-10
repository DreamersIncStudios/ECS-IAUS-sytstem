using System.Collections.Generic;
using DreamersIncStudio.GAIACollective;

namespace DreamersInc.BestiarySystem
{
    public class PackSpawnCreatureInfo : CreatureInfo
    {
        public List<PackRole> RequiredPackRoles;


    }

    public enum PackType
    {
        Assault, Support, Transport
    }

}