using IAUS.ECS.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using IAUS.ECS.Component;
using UnityEngine;
namespace IAUS.Sample.Archtypes
{
    public class Citizen : BaseAI
    {
        public int TravelRange;
        public int MaxCashOnHand;
        public int CarryLimit;
        [SerializeField]public ConsiderationBased Test;

        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            //Add Additional Components Here;
            var Cit = new CitizenC() { HomePos = transform.position, TravelRange = TravelRange, MaxCashOnHand = MaxCashOnHand, CarryLimit = CarryLimit, BeingRobbed= false, CashOnHand= 0 };
            var Money = new GetMoney()
            {
                Cooldown = 20,
                state = States.Idle,
                MoneyOnHand = new ConsiderationBased() {
                    responseType= ResponseTypeECS.Logistic,
                    M=75,
                    K=1,
                    B=.025f,
                    C=.3f
                },
                Robbing = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M =50,
                    K =1,
                    B =0,
                    C =.5f,
                },
                ItemsOnHand = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M =50,
                    K =.9f,
                    B =.025f,
                    C =.6f,
                }
            };
            var Buy = new GoBuyStuff()
            {
                Cooldown = 0f,
                state = States.Idle,
                MoneyOnHand = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = -1,
                    B = 1f,
                    C = .375f
                },
                Robbing = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = 1,
                    B = 0,
                    C = .5f,
                },
                ItemsOnHand = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = .9f,
                    B = .025f,
                    C = .6f,
                }
            };
            var Run = new Evade()
            {
                Cooldown = 180,
                state = States.Idle,
                MoneyOnHand = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 75,
                    K = -1,
                    B = 1f,
                    C = .5f
                },
                Robbing = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = -1,
                    B = 1,
                    C = .5f,
                },
                ItemsOnHand = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 75,
                    K = -1f,
                    B = 1f,
                    C = .6f,
                }
            };
            var Home= new TakeStuffHome()
            {
                Cooldown = 180,
                state = States.Idle,
                MoneyOnHand = new ConsiderationBased() // consider removing money
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = 1,
                    B = .025f,
                    C = .3f
                },
                Robbing = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 50,
                    K = 1,
                    B = 0,
                    C = .5f,
                },
                ItemsOnHand = new ConsiderationBased()
                {
                    responseType = ResponseTypeECS.Logistic,
                    M = 40,
                    K = -.9f,
                    B = 1f,
                    C = .45f,
                }
            };
            dstManager.AddComponentData(entity, Cit);
            dstManager.AddComponentData(entity, Money);
            dstManager.AddComponentData(entity, Run);
            dstManager.AddComponentData(entity, Buy);
            dstManager.AddComponentData(entity, Home);

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
        public bool BeingRobbed;

    }
}