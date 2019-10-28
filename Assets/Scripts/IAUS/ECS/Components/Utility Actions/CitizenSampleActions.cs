using UnityEngine;
using IAUS.ECS.Utilities;
using System;
using Unity.Mathematics;
namespace IAUS.ECS.Component {
    [Serializable]
    public struct GetMoney : BaseUtilityAction
    {
        public float Score { get; set; }
        [SerializeField] public States state { get; set; }
        public float Cooldown { get; set; }
       [SerializeField] public float Timer { get; set; }

        public bool InCooldown()
        {
                return Timer > 0.0f;
        }
        public int NumberOfConsiderations { get { return 3; } }

       [SerializeField] public ConsiderationBased MoneyOnHand;
        [SerializeField] public ConsiderationBased Robbing;
        [SerializeField] public ConsiderationBased ItemsOnHand;

        public float InfiniteAxisModScore() {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score +(MakeUp*Score));
        }

    }
    public struct GoBuyStuff: BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }
        public float Delay;
        public bool Delayed() {
            return Delay > .01f;
        }
        public bool InCooldown()
        {
            return Timer > 0.1f;
        }

        public int NumberOfConsiderations { get { return 3; } }
        public ConsiderationBased MoneyOnHand;
        public ConsiderationBased ItemsOnHand;
        public ConsiderationBased Robbing;


        public float InfiniteAxisModScore()
        {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score + (MakeUp * Score));
        }

    }
    public struct TakeStuffHome : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 1.0f;
        }
        public int NumberOfConsiderations { get { return 3; } }

        public ConsiderationBased MoneyOnHand;
        public ConsiderationBased ItemsOnHand;
        public ConsiderationBased Robbing;

        public float InfiniteAxisModScore()
        {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score + (MakeUp * Score));
        }

    }

    public struct Evade : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.2f;
        }
        public int NumberOfConsiderations { get { return 3; } }

        public ConsiderationBased MoneyOnHand;
        public ConsiderationBased ItemsOnHand;
        public ConsiderationBased Robbing;
        public float InfiniteAxisModScore()
        {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score + (MakeUp * Score));
        }

    }
}
