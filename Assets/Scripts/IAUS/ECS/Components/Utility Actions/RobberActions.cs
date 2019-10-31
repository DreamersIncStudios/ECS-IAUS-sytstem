using UnityEngine;
using IAUS.ECS.Utilities;
using System;
using Unity.Mathematics;
namespace IAUS.ECS.Component
{
    public struct Lurk : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.0f;
        }
        public int NumberOfConsiderations { get { return 5; } }

        public float InfiniteAxisModScore()
        {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score + (MakeUp * Score));
        }

        public ConsiderationBased MoneyOnHand;
        public ConsiderationBased Robbing;
        public ConsiderationBased ItemsOnHand;
        public ConsiderationBased InViewOfCop;
        public ConsiderationBased CanSeeTarget;
    }

    public struct Rob : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.0f;
        }
        public int NumberOfConsiderations { get { return 3; } }

        public float InfiniteAxisModScore()
        {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score + (MakeUp * Score));
        }

        public ConsiderationBased MoneyOnHand;
        public ConsiderationBased Robbing;
        public ConsiderationBased ItemsOnHand;
        public ConsiderationBased InViewOfCop;
        public ConsiderationBased CanSeeTarget;
    }

    public struct RunFromCops : BaseUtilityAction
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.0f;
        }
        public int NumberOfConsiderations { get { return 3; } }

        public float InfiniteAxisModScore()
        {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score + (MakeUp * Score));
        }

        public ConsiderationBased MoneyOnHand;
        public ConsiderationBased Robbing;
        public ConsiderationBased ItemsOnHand;
        public ConsiderationBased InViewOfCop;
        public ConsiderationBased CanSeeTarget;
    }

    public struct ReturnHome : BaseUtilityAction
    
    {
        public float Score { get; set; }
        public States state { get; set; }
        public float Cooldown { get; set; }
        public float Timer { get; set; }

        public bool InCooldown()
        {
            return Timer > 0.0f;
        }
        public int NumberOfConsiderations { get { return 3; } }

        public float InfiniteAxisModScore()
        {
            float mod = 1.0f - (1.0f / NumberOfConsiderations);
            float MakeUp = (1.0F - Score) * mod;
            return Mathf.Clamp01(Score + (MakeUp * Score));
        }

        public ConsiderationBased MoneyOnHand;
        public ConsiderationBased Robbing;
        public ConsiderationBased ItemsOnHand;
        public ConsiderationBased InViewOfCop;
        public ConsiderationBased CanSeeTarget;
    }
}