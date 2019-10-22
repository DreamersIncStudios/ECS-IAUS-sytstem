using Unity.Entities;
using Unity.Jobs;
using IAUS.ECS.Component;
using UnityEngine;
namespace IAUS.ECS.System
{

    public struct CheckCashOnHand : IJobForEach<CitizenC, GetMoney>
    {
        public void Execute(ref CitizenC c0, ref GetMoney c1)
        {
            c1.Score = Mathf.Clamp01((float)c0.CashOnHand / c0.MaxCashOnHand);
        }
    }
    public struct CheckItemsOnHand : IJobForEach<CitizenC, GetMoney>
    {
        public void Execute(ref CitizenC c0, ref GetMoney c1)
        {
            c1.Score = Mathf.Clamp01((float)c0.boughtItem / c0.CarryLimit);
        }
    }


}