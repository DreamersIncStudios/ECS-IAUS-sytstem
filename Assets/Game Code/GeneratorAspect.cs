using Stats.Entities;
using Unity.Entities;

namespace Game_Code
{
    public struct Generator : IComponentData
    {
        public float FuelLevel=> CurFuelLevel / fuelCapacity;
        private readonly float fuelCapacity;
        public float FuelConsumption;
        public float CurFuelLevel;
        public float MaxPowerOutput;
        public bool BeingRepaired;

        public Generator(float fuelCapacity, float curFuelLevel)
        {
            this.fuelCapacity = fuelCapacity;
            CurFuelLevel = curFuelLevel;
            FuelConsumption = 0;
            MaxPowerOutput = 0;
            BeingRepaired = false;
        }
    }

    public struct BatteryBank : IComponentData
    {
        public float PowerLevel=> CurPowerLevel / PowerCapacity;
        public float PowerCapacity;
        public float Consumption;
        public float CurPowerLevel;
        public float MaxPowerOutput;
        public bool BeingRepaired;
    }

    public readonly partial struct GeneratorAspect : IAspect
    {
        private readonly RefRO<Generator> tag;
        private readonly RefRO<AIStat> stats;

        public bool CanOutputPower => tag.ValueRO.FuelLevel>0.0f && stats.ValueRO.HealthRatio>0.0f && !tag.ValueRO.BeingRepaired;
        
        public void OutputPower()
        {
        }

        public void ConsumeFuel()
        {
        }

        public void GetRepaired()
        {
        }
    }

    public partial struct GeneratorUpdateSystem : ISystem
    {

        public void OnUpdate(ref SystemState state)
        {
            
        }
        
        partial struct GeneratorUpdateJob : IJobEntity
        {
            void Execute(Entity entity,  GeneratorAspect aspect)
            {
            }
        }
    }

}