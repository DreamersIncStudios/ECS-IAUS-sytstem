using System;
using System.Collections.Generic;
using AISenses;
using AISenses.VisionSystems;
using AISenses.VisionSystems.Combat;
using Components.MovementSystem;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Base;
using DreamersInc.ComboSystem;
using DreamersInc.InfluenceMapSystem;
using DreamersIncStudio.FactionSystem;
using DreamersIncStudio.GAIACollective;
using Global.Component;
using IAUS.ECS;
using IAUS.ECS.Component;
using IAUS.ECS.Component.Attacking;
using MotionSystem;
using MotionSystem.Components;
using ProjectDawn.Navigation;
using Stats;
using Stats.Entities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using LocalTransform = Unity.Transforms.LocalTransform;
using Object = UnityEngine.Object;

namespace DreamersInc.BestiarySystem
{


    public class CharacterBuilder
    {
        private GameObject model;
        private readonly Entity entity;
        private Entity aiEntity;
        private Entity visionEntity;
        private BaseCharacterComponent character;
        private FactionNames factionID;
        private uint classLevel;
        private string tag;
        private ComboSO combo;
        private EntityManager manager;

        public CharacterBuilder WithModel(GameObject go, Vector3 position, string tagging)
        {
            return WithModel(go, position, tagging, out _);
        }

        public CharacterBuilder WithModel(GameObject go, Vector3 position, string tagging, out GameObject spawned)
        {
            spawned = model = Object.Instantiate(go);
            go.transform.position = position;
            tag = go.tag = tagging;
            if (entity == Entity.Null) return this;
            manager.SetComponentData(entity, new LocalTransform()
            {
                Position = position,
                Rotation = go.transform.rotation,
                Scale = 1
            });
            return this;
        }

        public CharacterBuilder WithAnimation()
        {
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            var anim = model.GetComponent<Animator>();
           // manager.AddComponentObject(entity, anim);
            TransformGO transformLink = new()
            {
                transform = model.transform
            };
            manager.AddComponentData(entity, transformLink);
            return this;
        }

        public CharacterBuilder WithAIControl()
        {
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            manager.AddComponentData(entity, Agent.Default);
            manager.AddComponentData(entity, AgentBody.Default);
            manager.AddComponentData(entity, AgentLocomotion.Default);
            manager.AddComponentData(entity, new AgentShape()
            {
                Radius = 2,
                Height = 4,
                Type = ShapeType.Cylinder
            });
            manager.AddComponentData(entity, AgentCollider.Default);
            manager.AddComponentData(entity, AgentSonarAvoid.Default);
            manager.AddComponentData(entity, AgentSeparation.Default);
            manager.AddComponentData(entity, new GiveUpStopTimer());

            var move = new Movement()
            {
                //Acceleration = agent.acceleration,
                //StoppingDistance = agent.stoppingDistance,
                //Offset = agent.baseOffset,
            };
            move.SetMovementSpeed(character.GetPrimaryAttribute((int)AttributeName.Speed).AdjustBaseValue);
            manager.AddComponentData(entity, move);
            manager.AddComponentData(entity, new AI_Control());
            manager.AddComponentData(entity, new AIStat() { Speed = 10 });
            return this;
        }


        public CharacterBuilder WithNPCAttack(NPCAttackSequence sequence)
        {
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            manager.AddComponentObject(entity, new NPCAttack()
            {
                AttackSequence = sequence
            });


            return this;
        }
        public CharacterBuilder WithPlayerControl()
        {
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            manager.AddComponent<Player_Control>(entity);
            manager.AddComponent<AttackTarget>(entity);
            manager.AddComponentObject(entity, new Command());

            return this;
        }

        public CharacterBuilder WithVFX()
        {
            return this;
        }

        public CharacterBuilder WithEntityPhysics(PhysicsInfo physicsInfo)
        {
            if (entity == Entity.Null)
            {
                Debug.Log("not entity");
                return this;
            }

            if (model == null)
            {
                Debug.Log("no model");
                return this;
            }

            var shape = new PhysicsShape();
            if (model.TryGetComponent<UnityEngine.CapsuleCollider>(out var capsule))
            {
                shape = PhysicsShape.Capsule;
            }

            if (model.TryGetComponent<UnityEngine.BoxCollider>(out var box))
            {
                shape = PhysicsShape.Box;
            }

            var spCollider = new BlobAssetReference<Unity.Physics.Collider>();
            switch (shape)
            {
                case PhysicsShape.Capsule:
                    spCollider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry()
                    {
                        Radius = capsule.radius,
                        Vertex0 = capsule.center + new Vector3(0, capsule.height, 0),
                        Vertex1 = new float3(0, 0, 0)

                    }, new CollisionFilter()
                    {
                        BelongsTo = physicsInfo.BelongsTo.Value,
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });


                    break;
                case PhysicsShape.Box:
                    if (box != null)
                        spCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry()
                        {
                            Center = box.center,
                            Size = box.size,
                            Orientation = quaternion.identity,
                        }, new CollisionFilter()
                        {
                            BelongsTo = physicsInfo.BelongsTo.Value,
                            CollidesWith = physicsInfo.CollidesWith.Value,
                            GroupIndex = 0
                        });
                    manager.AddComponentData(entity, new PhysicsCollider()
                        { Value = spCollider });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            manager.AddSharedComponent(entity, new PhysicsWorldIndex());
            manager.AddComponentData(entity, new PhysicsCollider()
                { Value = spCollider });
            manager.AddComponentData(entity, new PhysicsInfo
            {
                BelongsTo = physicsInfo.BelongsTo,
                CollidesWith = physicsInfo.CollidesWith
            });
            if (model.TryGetComponent<Rigidbody>(out var rb))
                manager.AddComponentObject(entity, rb);
            return this;
        }

        public CharacterBuilder WithCombat(ComboSO combo)
        {
            this.combo = combo;
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            //manager.AddComponent<StoreWeapon>(entity);

            var comboInfo = Object.Instantiate(combo);
            manager.AddComponentObject(entity, new PlayerComboComponent { Combo = comboInfo });
            return this;
        }

        public CharacterBuilder WithStats(ICharacterData stats, uint playerLevel, string name)
        {
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            BaseCharacterComponent data = new()
            {
                GORepresentative = model // todo change to instance 
            };
            data.SetupDataEntity(stats, name);
            manager.AddComponentObject(entity, data);

            this.character = data;
            
            var baseEntityArch = manager.CreateArchetype(
                typeof(LocalTransform),
                typeof(LocalToWorld),
                typeof(MeleeAttackPosition),
                typeof(RangeAttackPosition)
            );
            var baseDataEntity = manager.CreateEntity(baseEntityArch);
            manager.SetName(baseDataEntity, "Attack Location Entity");
            manager.SetComponentData(baseDataEntity, new LocalTransform() { Scale = 1 });
            manager.AddComponentData(baseDataEntity, new Parent()
            {
                Value = entity
            });
            manager.AddBuffer<ReserveLocationTag>(baseDataEntity);
            var meleeAttackPositions = manager.GetBuffer<MeleeAttackPosition>(baseDataEntity);
            meleeAttackPositions.Length = 4;
            var rangeAttackPositions = manager.GetBuffer<RangeAttackPosition>(baseDataEntity);
            rangeAttackPositions.Length = 6;
            return this;
        }

        public CharacterBuilder WithCharacterDetection()
        {
            if (entity == Entity.Null) return this;
            if (!model) return this;
            var baseEntityArch = manager.CreateArchetype(
                typeof(LocalTransform),
                typeof(LocalToWorld)
            );
            visionEntity = manager.CreateEntity(baseEntityArch);
            manager.SetName(visionEntity, "Vision Entity");
            manager.SetComponentData(visionEntity, new LocalTransform() { Scale = 1 });
            manager.AddComponentData(visionEntity, new Parent()
            {
                Value = entity
            });

            var vision = new Vision();
            vision.InitializeSense(character);
            //Todo Move to entity by self
            manager.AddBuffer<Enemies>(visionEntity);
            manager.AddBuffer<Allies>(visionEntity);
            manager.AddBuffer<AISenses.Resources>(visionEntity);
            manager.AddBuffer<PlacesOfInterest>(visionEntity);
            manager.AddBuffer<GlobalTargets>(visionEntity);
            manager.AddComponentData(visionEntity, vision);
            return this;
        }

        public CharacterBuilder WithInventorySystem(EquipmentSave Equipment)
        {
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            CharacterInventory inventory = new();
            inventory.Setup(Equipment, character);
            manager.AddComponentData(entity, inventory);

            return this;
        }

        public CharacterBuilder WithFactionInfluence(FactionNames factionID, int influenceValue, uint classLevel,
           float3 centerOffset = default, bool isPlayer = false)
        {
            if (entity == Entity.Null) return this;
            if (!model) return this;


            this.factionID = factionID;
            this.classLevel = classLevel;
            if (aiEntity != Entity.Null)
            {
                manager.AddComponentData(aiEntity,
                    new InfluenceComponent(this.factionID, influenceValue,
                        15));
            } //Todo add range of Influence to CharacterInfo Scriptable Object
            else
            {
                manager.AddComponentData(entity,
                    new InfluenceComponent(this.factionID, influenceValue, 15));
            }

            manager.AddComponentData(entity, new AITarget()
            {
                FactionID = (FactionNames)factionID,
                NumOfEntityTargetingMe = 3,
                CanBeTargetByPlayer = isPlayer,
                Type = TargetType.Character,
                level = classLevel,
                CenterOffset = centerOffset
            });

            manager.AddComponentData(entity, new Perceptibility
            {
                movement = MovementStates.Standing_Still,
                noiseState = NoiseState.Normal,
                visibilityStates = VisibilityStates.Visible
            });
            return this;
        }

        public CharacterBuilder WithMovement(MovementData Move)
        {
            if (entity == Entity.Null) return this;
            if (model == null) return this;
            manager.AddComponentData(entity, Agent.Default);
            manager.AddComponentData(entity, AgentBody.Default);
            manager.AddComponentData(entity, AgentLocomotion.Default);
            manager.AddComponentData(entity, new AgentShape()
            {
                Radius = 2,
                Height = 4,
                Type = ShapeType.Cylinder
            });
            manager.AddComponentData(entity, AgentCollider.Default);
            manager.AddComponentData(entity, AgentSonarAvoid.Default);
            manager.AddComponentData(entity, new AgentSeparation()
            {
                Radius = 2,
                Weight = 1,
                Layers = NavigationLayers.Everything
            });
            manager.AddComponentData(entity, AgentSmartStop.Default);
            var move = new Movement()
            {

            };
            manager.AddComponentData(entity, new GiveUpStopTimer());
            move.SetMovementSpeed(character.GetPrimaryAttribute((int)AttributeName.Speed).AdjustBaseValue);
            manager.AddComponentData(entity, move);
            var controllerData = new CharControllerE();
            controllerData.Setup(Move, model.GetComponent<UnityEngine.CapsuleCollider>());
            manager.AddComponentData(entity, controllerData);

            return this;
        }

        public CharacterBuilder WithAI(NPCLevel getNpcLevel, List<AIStates> aiStatesToAdd, bool capableOfMelee = false,
            bool capableOfMagic = false, bool capableOfRange = false, Role role = default)
        { if (!model || entity == Entity.Null) return this;

        var baseEntityArch = manager.CreateArchetype(
            typeof(LocalTransform),
            typeof(LocalToWorld)
        );
        aiEntity = manager.CreateEntity(baseEntityArch);
        manager.SetName(aiEntity, "AI Entity");
        manager.SetComponentData(aiEntity, new LocalTransform() { Scale = 1 });
        manager.AddComponentData(aiEntity, new Parent()
        {
            Value = entity
        });

        manager.AddComponentData(aiEntity, new AIStat());

       manager.AddComponentData(aiEntity,new IAUSBrain()
        {
            NPCLevel = getNpcLevel,
            FactionID = factionID,
            Difficulty = Difficulty.Normal,
            Role = role,
            CurrentState = AIStates.None
        });
        manager.AddComponentData(aiEntity, new VisionIAUSLink(visionEntity));
        model.layer = LayerMask.NameToLayer("NPC");
        var statesToCheck = manager.AddBuffer<StateData>(aiEntity);
        foreach (var state in aiStatesToAdd)
        {
            statesToCheck.Add(new StateData(state));
        }

        manager.AddComponent<SetupBrainTag>(aiEntity);

            return this;
        }
        


        public CharacterBuilder(string entityName, out Entity spawnedEntity)
        {
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var baseEntityArch = manager.CreateArchetype(
                typeof(LocalTransform),
                typeof(LocalToWorld)
            );
            var baseDataEntity = manager.CreateEntity(baseEntityArch);
            manager.SetName(baseDataEntity, entityName != string.Empty ? entityName : "NPC Data");
            manager.SetComponentData(baseDataEntity, new LocalTransform() { Scale = 1 });
            spawnedEntity = entity = baseDataEntity;

        }

        public CharacterBuilder(string entityName)
        {
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var baseEntityArch = manager.CreateArchetype(
                typeof(LocalTransform),
                typeof(LocalToWorld)
            );
            var baseDataEntity = manager.CreateEntity(baseEntityArch);
            manager.SetName(baseDataEntity, entityName != string.Empty ? entityName : "NPC Data");
            manager.SetComponentData(baseDataEntity, new LocalTransform() { Scale = 1 });
            entity = baseDataEntity;

        }
        public CharacterBuilder WithActiveHour(TimesOfDay activeHour, uint HomeBiomeID)
        {
            manager.AddComponentData(entity, new GaiaLife(activeHour, HomeBiomeID));
            return this;
        }
        
        public CharacterBuilder WithParent(Entity requestParentToLink)
        {
            if(requestParentToLink == Entity.Null) 
                return this;
            manager.AddComponentData(entity, new Parent()
            {
                Value = requestParentToLink
            });
            return this;
        }
        
        public Entity Build()
        {

            return entity;
        }
    }
}