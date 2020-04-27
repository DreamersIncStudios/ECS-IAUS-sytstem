using Unity.Entities;
namespace IAUS.ECS2.Charaacter
{
    public class GruntAuthoring :CharacterAuthoring
    {
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);

            dstManager.AddComponent<GruntComponent>(entity);
        }



    }

    public struct GruntComponent : IComponentData {
        public Entity Leader;
    
    }
}