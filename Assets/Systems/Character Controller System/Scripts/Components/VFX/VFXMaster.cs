using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DreamersInc.CharacterControllerSys.VFX
{
    public class VFXMaster : MonoBehaviour
    {
        class baker : Baker<VFXMaster>
        {
            public override void Bake(VFXMaster authoring)
            {
                AddComponentObject(new VFXSpawnMaster());
                AddComponent(new vfxTag());

            }
        }
    }
    public class VFXDatabase : MonoBehaviour {
        public static VFXDatabase Instance;
        public TextAsset VFXList;
        List<VFXInfo> vfxInfos;
        bool VFXLoaded;
        bool PoolLoaded;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this.gameObject);

            loadVFX();
        }
        public void loadVFX()
        {
            VFXLoaded = true;
            VFXList = Resources.Load<TextAsset>("VFX/Combo Damage");
            var lines = VFXList.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            vfxInfos = new List<VFXInfo>();
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');

                VFXInfo temp = new()
                {
                    ID = int.Parse(parts[0]),
                    PoolObject = Resources.Load<GameObject>(parts[1])
                };
                temp.CreatePool(this.gameObject);
                vfxInfos.Add(temp);
            }
        }
        void DestoryVFXPool()
        {
            foreach (Transform item in transform)
            {
                Destroy(item.gameObject);
            }
        }
        public void PlayVFX(int ID, Vector3 Pos, Vector3 Rot, float DelayStart = 0.0f, float lifeTime = 0.0f)
        {
            if (!VFXLoaded)
            {
                DestoryVFXPool();
                loadVFX();
            }
            GetVFX(ID).Play(Pos, Rot, DelayStart, lifeTime);
        }
        public void PlayVFX(int ID, Vector3 Pos, float lifeTime = 0.0f)
        {
            if (!VFXLoaded)
            {
                DestoryVFXPool();
                loadVFX();
            }
            GetVFX(ID).Play(Pos, lifeTime);
        }
        public VFXInfo GetVFX(int id)
        {
            VFXInfo temp = new();
            foreach (var item in vfxInfos)
            {
                if (item.ID == id)
                {
                    temp = item;
                    return temp;
                }
            }
            return null;

        }
    }
    public class VFXSpawnMaster : IComponentData {
        public VFXDatabase VFXspawn;
    }
    public struct vfxTag : IComponentData { };
    public partial class VFXSetup : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

          foreach (var (VFXMast, tag, entity) in SystemAPI.Query<VFXSpawnMaster, vfxTag>().WithEntityAccess())
            {
                var go = new GameObject();
                go.name = "VFX Database";
                VFXMast.VFXspawn = go.AddComponent<VFXDatabase>();
                ecb.RemoveComponent<vfxTag>(entity);
            }
        }
    }

}