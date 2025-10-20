using System.Linq;
using IAUS.ECS.Component;
using Stats.Entities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace IAUS.ECS.Systems.Reactive
{


    partial struct DetermineAction : IJobEntity
    {
        public float deltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref AttackActionTag state, in AIStat stat, in LocalToWorld transform)
        {

            if (state.AttackPlans.Length != 0) return;
            state.AttackType = DetermineHowToAttack(state, stat, transform);
            // select an attack Plan
            int[] scores = new[]
            {
                -1,
                RestScore(state, stat),
                Wander,
                GetTargetLocation(state),
                GetAttackLocation(state),
                TravelToTargetMeleeLocation(state, stat, transform),
                TravelToTargetMagicLocation(state, stat, transform),
                TravelToTargetRangeLocation(state, stat, transform),
                MeleeScore(state, stat, transform),
                MagicScore(state, stat, transform),
                RangeScore(state, stat, transform),
                EvadeTarget(state, stat)
            };
            var sortedScores = scores.ToList().OrderByDescending(x => x);
            foreach (var score in sortedScores)
            {
                if (score <= 0) continue;
                if (state.AttackPlans.Length >= 8) return;
                var index = scores.ToList().IndexOf(score);
                state.AttackPlans.Add((AttackPlan)(index));
            }

           // aspect.ExecutePlan(entity, chunkIndex, deltaTime, ECB);
        }

        public HowToAttack DetermineHowToAttack(AttackActionTag state, AIStat stat, LocalToWorld transform)
        {
            int[] scores = new[]
            {
                -1,
                MeleeScore(state, stat, transform),
                MagicScore(state, stat, transform),
                RangeScore(state, stat, transform),
            };
            var sortedScores = scores.ToList().OrderByDescending(x => x);

            var index = scores.ToList().IndexOf(0);
            return (HowToAttack)(index);
        }

        private int MeleeScore(AttackActionTag state, AIStat stats, LocalToWorld transform)
        {
            if (state.InAttackCooldown) return 0;
            if (!state.CapableOfMelee) return 0;
            //Todo add Map influence check

            var temp = 2;
            if (InSafeHpRange(stats))
                temp++;
            if (!CoverInRange() && (state.CapableOfMagic || state.CapableOfProjectile))
            {
                if (!CoverInRange())
                    temp++;
            }

            if (MultiAttackStates(state))
            {
                if (!state.CapableOfMagic && ManaLevelLow())
                    temp++;

                if (!state.CapableOfProjectile && AmmoLevelLow())
                    temp++;
            }

            if (!InAttackRange(state, 3, transform)) return temp;
            temp++;
            return temp;

        }

        private int RangeScore(AttackActionTag state, AIStat stats, LocalToWorld transform)
        {

            if (state.InAttackCooldown) return 0;
            if (!state.CapableOfProjectile) return 0;
            //Todo add Map influence check
            //Check for cover
            //check for Ammo
            //check for HP
            var temp = 2;
            if (!CoverInRange())
                temp++;
            if (MultiAttackStates(state))
            {
                if (!state.CapableOfMagic && ManaLevelLow())
                    temp++;

                if (!state.CapableOfMelee && !AmmoLevelLow())
                    temp++;
            }

            if (!InAttackRange(state, 3, transform)) return temp;
            temp++;
            return temp;

        }
        private int MagicScore(AttackActionTag state, AIStat stats, LocalToWorld transform)
        {

            if (state.InAttackCooldown) return 0;
            if (!state.CapableOfMagic) return 0;
            //Todo add Map influence check

            var temp = 2;
            if (!CoverInRange())
                temp++;
            if (MultiAttackStates(state))
            {
                if (!state.CapableOfMelee && !ManaLevelLow())
                    temp++;

                if (!state.CapableOfProjectile && AmmoLevelLow())
                    temp++;
            }

            if (!InAttackRange(state, 3, transform)) return temp;

            temp++;
            return temp;

        }

        private bool InSafeHpRange(AIStat stats) => stats.HealthRatio > .425f;
        
        bool CoverInRange()
        {
            return false;
        }

        bool ManaLevelLow()
        {
            return false;
        }

        bool AmmoLevelLow()
        {
            return false;
        }
        bool InAttackRange(AttackActionTag state, float range, LocalToWorld transform)
        {
            if (state.AttackPosition.Equals(float3.zero)) return false;
            var distance = Vector3.Distance(transform.Position, state.AttackPosition);
            return distance <= range;
        }
        private bool MultiAttackStates(AttackActionTag state) => (state.CapableOfMagic && state.CapableOfMelee) ||
                                                                 (state.CapableOfMagic && state.CapableOfProjectile) ||
                                                                 (state.CapableOfMelee && state.CapableOfProjectile);


        private int TravelToTargetMeleeLocation(AttackActionTag state, AIStat stats, LocalToWorld transform)
        {

            if (!state.CapableOfMelee) return 0;
            if (InAttackRange(state, 6, transform)) return 0;
            var temp = 1;
            if (stats.HealthRatio < .35f) return temp;
            temp++;
            if (stats.HealthRatio > .65f)
                temp++;
            return temp;

        }
        private int TravelToTargetMagicLocation(AttackActionTag state, AIStat stats, LocalToWorld transform)
        {

            if (!state.CapableOfMagic || InAttackRange(state, 10, transform)) return 0;
            var temp = 3;
            if (stats.HealthRatio > .35f)
            {
                if (state.CapableOfMelee)
                    temp++;
                else
                {
                    return temp;
                }
            }

            if (stats.HealthRatio > .65f)
                temp++;
            return temp;

        }

        private int TravelToTargetRangeLocation(AttackActionTag state, AIStat stats, LocalToWorld transform)
        {

            if (!state.CapableOfProjectile|| InAttackRange(state, 10, transform)) return 0;
            var temp = 2;
            if (!(stats.HealthRatio < .35f))
                return temp;
            if (state.CapableOfMelee)
                temp++;


            return temp;

        }

        private int Wander => 0;

        private int EvadeTarget(AttackActionTag state, AIStat stats)
        {
           
                if (stats.HealthRatio > .35f) return 0;
                var temp = 2;
                return temp;
            
        }

        private int RestScore(AttackActionTag state, AIStat stats)
        {
          
                var temp = 0;
                if (stats.HealthRatio < .35f) return temp;
                if (state.InAttackCooldown)
                    temp = 3;
                return temp;
            
        }

        private int GetAttackLocation(AttackActionTag state) => state.AttackPosition.Equals(float3.zero) ? 10 : 0;
        private int GetTargetLocation(AttackActionTag state) => state.TargetPosition.Equals(float3.zero) ? 10 : 0;
    }
}