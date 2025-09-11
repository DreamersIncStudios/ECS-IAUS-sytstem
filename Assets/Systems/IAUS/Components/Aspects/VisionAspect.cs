using Global.Component;
using System.Linq;
using DreamersIncStudio.FactionSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AISenses.VisionSystems
{
    public readonly partial struct VisionAspect : IAspect
    {
        private readonly DynamicBuffer<Enemies> enemies;
        private readonly DynamicBuffer<Allies> allies;
        private readonly DynamicBuffer<Resources> Resources;
        private readonly DynamicBuffer<PlacesOfInterest> placesOfInterests;
        private readonly RefRW<Vision> vision;

        public float3 TargetEnemyPosition => vision.ValueRO.TargetEnemyPosition;
        public Entity TargetEnemy => vision.ValueRO.TargetEnemyEntity;
        public float3 TargetFriendPosition => vision.ValueRO.TargetFriendlyPosition;
        public Entity TargetFriend => vision.ValueRO.TargetFriendlyEntity;
        

        public bool TargetInReactRange
        {
            get
            {
                foreach (var item in enemies)
                    if (item is { Dist: < 25, target: { Affinity: Affinity.Hate or Affinity.Negative } } )
                    {
                        return true;
                    }

                return false;
            }

        }


        public bool TargetEnemyTargetInRange(out float dist)
        {
            return TargetEnemyTargetInRange(out _, out _, out dist);
        }



        private bool TargetEnemyTargetInRange(out float3 Position, out AITarget target, out float dist)
        {
            target = new AITarget();
            dist = 0f;
            Position = float3.zero;
            if (enemies.IsEmpty)
            {
                vision.ValueRW.TargetEnemyEntity = Entity.Null;
                return false;
            }

            foreach (var enemy in enemies)
            {
                if (enemy.target.Affinity is Affinity.Love or Affinity.Positive or Affinity.Neutral) continue;
                target = enemy.target.TargetInfo;
                dist = enemy.target.DistanceTo;
                vision.ValueRW.TargetEnemyEntity = enemy.target.Entity;
                Position = vision.ValueRW.TargetEnemyPosition =
                    vision.ValueRW.LastKnownPositionEnemy = enemy.target.LastKnownPosition;
                return true;
            }

            return false;
        }


    }
}