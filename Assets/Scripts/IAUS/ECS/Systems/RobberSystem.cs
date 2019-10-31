using Unity.Entities;
using Unity.Jobs;
using IAUS.ECS.Component;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;


namespace IAUS.ECS.System
{
    public struct RobberScore : IJobForEach<RobberC, Lurk, Rob, RunFromCops, ReturnHome>
    {
        public float DT;
        public void Execute(ref RobberC c0, ref Lurk c1, ref Rob c2, ref RunFromCops c3, ref ReturnHome c4)
        {
            if (c0.CashOnHand < 0)
                c0.CashOnHand = 0;

            if (c1.InCooldown())
            {
                c1.Timer -= DT;
            }
            else if (c1.state != States.Running)
            {
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
            float Robbing = c0.Robbing? 1 : 0;
            c1.Score = c1.MoneyOnHand.Output(money) * c1.Robbing.Output(Robbing) * c1.ItemsOnHand.Output(item) * 
                c1.InViewOfCop.Output(c0.DistanceToCop) * c1.CanSeeTarget.Output(c0.DistnaceToTarget);
            c2.Score = c2.MoneyOnHand.Output(money) * c2.Robbing.Output(Robbing) * c2.ItemsOnHand.Output(item) *
    c2.InViewOfCop.Output(c0.DistanceToCop) * c2.CanSeeTarget.Output(c0.DistnaceToTarget);
            c3.Score = c3.MoneyOnHand.Output(money) * c3.Robbing.Output(Robbing) * c3.ItemsOnHand.Output(item) *
    c3.InViewOfCop.Output(c0.DistanceToCop) * c3.CanSeeTarget.Output(c0.DistnaceToTarget);
            c4.Score = c4.MoneyOnHand.Output(money) * c4.Robbing.Output(Robbing) * c4.ItemsOnHand.Output(item) *
    c4.InViewOfCop.Output(c0.DistanceToCop) * c4.CanSeeTarget.Output(c0.DistnaceToTarget);


        }
    }

    public struct RobberSys : IJobForEachWithEntity<RobberC, Movement, LocalToWorld>
    {




        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<ReturnHome> Home;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Lurk> Lurker;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Rob> Steal;
        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<RunFromCops> Evade;


        public void Execute(Entity entity, int index, ref RobberC c0, ref Movement Move, ref LocalToWorld c2)
        {
            float score = new float();
            WhichState whichState = WhichState.Idle;
            if (!Lurker[entity].InCooldown() &&
               Lurker[entity].state != States.Completed &&
               Lurker[entity].state != States.Interrupted &&
               score < Lurker[entity].InfiniteAxisModScore())
            {
                score = Lurker[entity].InfiniteAxisModScore();
                whichState = WhichState.Lurk;
            }
            if (!Steal[entity].InCooldown() &&
                   Steal[entity].state != States.Completed &&
                   Steal[entity].state != States.Interrupted &&
                     score < Steal[entity].InfiniteAxisModScore())
            {
                score = Lurker[entity].InfiniteAxisModScore();
                whichState = WhichState.Rob;
            }
            if (!Evade[entity].InCooldown() &&
               Evade[entity].state != States.Completed &&
               Evade[entity].state != States.Interrupted &&
               score < Evade[entity].InfiniteAxisModScore())
            {
                score = Lurker[entity].InfiniteAxisModScore();
                whichState = WhichState.RunFromCops;
            }
            if (!Home[entity].InCooldown() &&
               Home[entity].state != States.Completed &&
               Home[entity].state != States.Interrupted &&
               score < Home[entity].InfiniteAxisModScore())
            {
                score = Home[entity].InfiniteAxisModScore();
                whichState = WhichState.ReturnHome;
            }

            switch (whichState) {
                case WhichState.Idle:
                    if (Move.CanMove) {
                        Move.CanMove = false;
                    }
                break;
                case WhichState.Lurk:

                    break;
                case WhichState.Rob:

                    break;
                case WhichState.RunFromCops:

                    break;
                case WhichState.ReturnHome:
                   ReturnHome tempHome = Home[entity];
                    if (tempHome.state != States.Running)
                    {
                        Move.TargetLocation = c0.HomePos;
                        Move.CanMove = true;
                        tempHome.state = States.Running;
                    }
                    if (tempHome.state == States.Running && !Move.CanMove)
                    {

                        tempHome.Timer = tempHome.Cooldown;
                        tempHome.state = States.Completed;
                        c0.boughtItem = 0;
                    }

                    Home[entity] = tempHome;
                    break;
            }

        }

        enum WhichState
        {
          Idle,Lurk,Rob,RunFromCops,ReturnHome
        }
    }



    [UpdateAfter(typeof(DetectionSystem))]
    public class RobberSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.deltaTime;
            var RobberScoreJob = new RobberScore() { DT = DT };

            var sys= new RobberSys() {
                Home = GetComponentDataFromEntity<ReturnHome>(false),
                Lurker = GetComponentDataFromEntity<Lurk>(false),
                Steal = GetComponentDataFromEntity<Rob>(false),
                Evade= GetComponentDataFromEntity <RunFromCops>(false)

            };
            JobHandle score = RobberScoreJob.Schedule(this, inputDeps);
            return sys.Schedule(this, score);

        }
    }
}