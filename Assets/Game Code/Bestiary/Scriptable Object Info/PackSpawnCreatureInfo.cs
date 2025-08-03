using System.Collections.Generic;
using DreamersIncStudio.GAIACollective;
using Sirenix.OdinInspector;

namespace DreamersInc.BestiarySystem
{
    public class PackSpawnCreatureInfo : CreatureInfo
    {
        public PackType PackType;
        private bool packIsSpecial => PackType == PackType.Special;
        [ShowIf(nameof(packIsSpecial))]
        public List<PackRole> RequiredPackRoles;


    }

    public enum PackType
    {
        Assault, Support, Transport,
        Special
    }

}