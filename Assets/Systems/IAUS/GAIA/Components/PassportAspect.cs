using DreamersIncStudio.GAIACollective;
using IAUS.ECS.Component;
using Stats.Entities;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    [WithNone(typeof(PackMember)) ]
    public readonly partial struct PassportAspect : IAspect
    {
        private readonly RefRO<GaiaLife> life;
        private readonly RefRO<IAUSBrain> brain;
        private readonly RefRO<AIStat> stat;
        public uint ID => life.ValueRO.HomeBiomeID;
        public Role Role => brain.ValueRO.Role;
        public int Level => stat.ValueRO.Level;
    }
}