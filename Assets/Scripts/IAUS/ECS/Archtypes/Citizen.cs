
using Unity.Entities;
using Unity.Mathematics;
using IAUS.ECS.Component;

namespace IAUS.Sample.Archtypes
{
    public class Citizen : BaseAI
    {
        public int TravelRange;
        public int MaxCashOnHand;
        public int CarryLimit;
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            //Add Additional Components Here;
            var Cit = new CitizenC() { HomePos = transform.position, TravelRange = TravelRange, MaxCashOnHand = MaxCashOnHand, CarryLimit = CarryLimit };
            dstManager.AddComponentData(entity, Cit);
        }

    }
}
namespace IAUS.ECS.Component { 
   public struct CitizenC :IComponentData{
        public float3 HomePos;
        public int TravelRange;
        public int CashOnHand;
        public int MaxCashOnHand;
        public int boughtItem;
        public int CarryLimit;

    }
}