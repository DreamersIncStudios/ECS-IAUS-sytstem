using Unity.Entities;
using Unity.Jobs;
using IAUS.ECS.Component;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;


namespace IAUS.ECS.System
{
    // rewrite this into one group?? Can the robber us any of this jobs ? no 
    
    public struct CitizenScorer : IJobForEach<CitizenC, GetMoney,GoBuyStuff,TakeStuffHome,Evade>
    {
        public float DT;

        public void Execute(ref CitizenC c0, ref GetMoney c1, ref GoBuyStuff c2, ref TakeStuffHome c3, ref Evade c4)
        {
            if (c0.CashOnHand < 0)
                c0.CashOnHand = 0;

            if (c1.InCooldown())
            {
                c1.Timer -= DT;
            }
            else if(c1.state!=States.Running){
                c1.state = States.Idle;
            }
            if (c2.InCooldown())
            {
                c2.Timer -= DT;
            }
            else if (c2.state != States.Running)
            {
                c2.state = States.Idle;
            }

            if (c3.InCooldown())
            {
                c3.Timer -= DT;
            }
            else if (c3.state != States.Running)
            {
                c3.state = States.Idle;
            }

            float money = (float)c0.CashOnHand / c0.MaxCashOnHand;
            float item = ((float)c0.boughtItem / c0.CarryLimit);
            float Robbing = c0.BeingRobbed ? 1 : 0;

            c1.Score = c1.MoneyOnHand.Output(money) * c1.ItemsOnHand.Output(item) * c1.Robbing.Output(Robbing);
            c2.Score = c2.MoneyOnHand.Output(money) * c2.ItemsOnHand.Output(item) * c2.Robbing.Output(Robbing);
            c3.Score = c3.MoneyOnHand.Output(money) * c3.ItemsOnHand.Output(item) * c3.Robbing.Output(Robbing);
            c4.Score = c4.MoneyOnHand.Output(money) * c4.ItemsOnHand.Output(item) * c4.Robbing.Output(Robbing);
           // Debug.Log(c1.InfiniteAxisModScore());
           // Debug.Log(c2.ItemsOnHand.Output(item));
           // Debug.Log(c3.InfiniteAxisModScore());
           // Debug.Log(c4.InfiniteAxisModScore());

        }
    }
    public struct AICitizenJob : IJobForEachWithEntity<CitizenC, Movement,LocalToWorld>
    {
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<GetMoney> Money;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<GoBuyStuff> Buy;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Evade> Evade;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<TakeStuffHome> Home;
        [DeallocateOnJobCompletion] [ReadOnly]public NativeArray<LocalToWorld> ATMPositions;
        [DeallocateOnJobCompletion] [ReadOnly]public NativeArray<LocalToWorld> StorePositions;

        public float DT;
        public void Execute(Entity entity, int index, ref CitizenC c0, ref Movement Move, ref LocalToWorld Pos)
        {
            float score = new float();
            WhichState whichState = new WhichState();
            if (!Money[entity].InCooldown() &&
                Money[entity].state != States.Completed &&
                Money[entity].state != States.Interrupted &&
                score < Money[entity].InfiniteAxisModScore())
            {
              score =  Money[entity].InfiniteAxisModScore();
                whichState = WhichState.GetMoney;
            }

            if (!Buy[entity].InCooldown() && 
                Buy[entity].state != States.Completed &&
                Buy[entity].state != States.Interrupted &&
                score < Buy[entity].InfiniteAxisModScore())
            {
                score = Buy[entity].InfiniteAxisModScore();
                whichState = WhichState.GetItems;
            }

            if (!Evade[entity].InCooldown() &&
                score < Evade[entity].InfiniteAxisModScore())
            {
                score = Evade[entity].InfiniteAxisModScore();
                whichState = WhichState.Evade;
            }

            if (!Home[entity].InCooldown() &&
                score < Home[entity].InfiniteAxisModScore())
            {
                score = Home[entity].InfiniteAxisModScore();
                whichState = WhichState.GoHome;
            }

           // Debug.Log(whichState);
            switch (whichState) {
                case WhichState.GetMoney:
                    GetMoney tempMoney = new GetMoney();
                    if (Money[entity].state != States.Running )
                    {
                        float3 closestATM = new float3();
                        closestATM = ATMPositions[0].Position;
                        for (int cnt = 0; cnt < ATMPositions.Length-1; cnt++)
                        {
                            if (Vector3.Distance(Pos.Position, ATMPositions[cnt].Position) < Vector3.Distance(Pos.Position, closestATM))
                            {
                                closestATM = ATMPositions[cnt].Position;
                            }
                        }
                         tempMoney = Money[entity];
                        tempMoney.state = States.Running;
                        Money[entity] = tempMoney;
                        Move.MovementSpeed = 8;
                        Move.TargetLocation = closestATM;
                        Move.CanMove = true;
                    }
                    // need to add a delay
                    if (Money[entity].state == States.Running && !Move.CanMove) {
                        c0.CashOnHand = c0.MaxCashOnHand;// update late to intervals
                        tempMoney = Money[entity];
                        Money[entity] = tempMoney;
                        tempMoney.state = States.Idle;
                        tempMoney.Timer = tempMoney.Cooldown;
                        Money[entity] = tempMoney;
                    }

                    break;
                case WhichState.GetItems:
                 GoBuyStuff tempBuy = new GoBuyStuff();
                    tempBuy = Buy[entity];

                    if (tempBuy.Delayed()) { tempBuy.Delay -= DT; }
                    else
                    {
                        if (tempBuy.state != States.Running)
                        {
                            Move.MovementSpeed = 10;
                            float3 ClosestStore = new float3();
                            int StoreNumber = 0;
                            for (int cnt = 0; cnt < StorePositions.Length - 1; cnt++)
                            {
                                if (Vector3.Distance(Pos.Position, StorePositions[cnt].Position) < Vector3.Distance(Pos.Position, ClosestStore))
                                {
                                    if (!StorePositions[cnt].Position.Equals(Move.TargetLocation))
                                    {
                                        ClosestStore = StorePositions[cnt].Position;
                                        StoreNumber = cnt;
                                    }
                                }
                            }
                            Move.TargetLocation = StorePositions[StoreNumber].Position;
                            Move.CanMove = true;
                            tempBuy.state = States.Running;

                        }
                        if (tempBuy.state == States.Running && !Move.CanMove)
                        {

                            tempBuy.Timer = tempBuy.Cooldown;
                            tempBuy.state = States.Completed;
                            tempBuy.Delay = 10;
                            //Buy[entity] = tempBuy;
                            c0.CashOnHand -= 2000;
                            c0.boughtItem++;
                        }
                    }
            
                    Buy[entity] = tempBuy;

                        break;
                case WhichState.Evade:
                    break;
                case WhichState.GoHome:
                    TakeStuffHome tempHome = Home[entity];
                    if (tempHome.state != States.Running) {
                        Move.TargetLocation = c0.HomePos;
                        Move.CanMove = true;
                        tempHome.state = States.Running;
                    }
                    if (tempHome.state == States.Running && !Move.CanMove) {

                        tempHome.Timer = tempHome.Cooldown;
                        tempHome.state = States.Completed;
                        c0.boughtItem = 0;
                    }

                    Home[entity] = tempHome;
                    break;
            }
        }


        enum WhichState {
            GetMoney,GetItems,GoHome,Evade
        }
    }



    
    public class ScoreSystem : JobComponentSystem
    {
        [DeallocateOnJobCompletion] NativeArray<LocalToWorld> ATMPost;
        [DeallocateOnJobCompletion] NativeArray<LocalToWorld> StorePost;


    
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityQueryDesc ATMS = new EntityQueryDesc {
                All = new ComponentType[] { typeof(ATMC), typeof(LocalToWorld) }

            };
            EntityQueryDesc Stores = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(StoreC), typeof(LocalToWorld) }

            };
            ATMPost = GetEntityQuery(ATMS).ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            StorePost = GetEntityQuery(Stores).ToComponentDataArray<LocalToWorld>(Allocator.TempJob);

            var Citizen = new CitizenScorer() { DT=Time.DeltaTime};

            var ai = new AICitizenJob()
            {
                Money = GetComponentDataFromEntity<GetMoney>(false),
                ATMPositions = ATMPost,
                Buy = GetComponentDataFromEntity<GoBuyStuff>(false),
                Evade = GetComponentDataFromEntity<Evade>(false),
                Home = GetComponentDataFromEntity<TakeStuffHome>(false),
                StorePositions = StorePost,
                DT=Time.DeltaTime
              
            };

           JobHandle job1 =  Citizen.Schedule(this, inputDeps);
         return   ai.Schedule(this, job1);
        }
    }

}