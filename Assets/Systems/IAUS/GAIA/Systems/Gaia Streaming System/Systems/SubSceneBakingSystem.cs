using System.Collections.Generic;
using DreamersIncStudio.GAIACollective.Streaming.SceneManagement.SectionMetadata;
using Sirenix.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
public partial class SubSceneBakingSystem : SystemBase
{
    private Dictionary<Hash128, List<SectionData>> sectionDataDictionary;

    private int GetSectionID(float3 position, int layer, out float3 center)
    {
        center = float3.zero;
        if (layer == 6) return 0;

        var quadrantCellSize = layer switch
        {
            26 => 300,
            27 => 100,
            28 => 50,
            _ => 600
        };
        var quadrantYMultiplier = layer switch
        {
            28 => 100,
            27 => 200,
            26 => 1000,
            _ => 2000
        };
        var xIndex = Mathf.Floor(position.x / quadrantCellSize);
        var zIndex = Mathf.Floor(position.z / quadrantCellSize);
        var centerX = (xIndex + 0.5f) * quadrantCellSize;
        var centerZ = (zIndex + 0.5f) * quadrantCellSize;
        center = new float3(centerX, 0, centerZ); // Assuming Y is 0 for the center height
        return (int)(Mathf.FloorToInt(position.x / quadrantCellSize) + (quadrantYMultiplier * Mathf.Floor(position.z / quadrantCellSize)));
    }

    protected override void OnCreate()
    {
        sectionDataDictionary = new Dictionary<Hash128, List<SectionData>>();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities.WithoutBurst().ForEach(
            (Entity entity, SceneSection section, in LocalToWorld transform, in RenderFilterSettings settings) =>
            {
                if (sectionDataDictionary.TryGetValue(section.SceneGUID , out var sectionData))
                {
                    if (TryUpdateExistingSection(sectionData, settings, transform, entity, ecb, section.SceneGUID))
                        return;

                    AddNewSection(sectionData, settings, transform, entity, ecb, section.SceneGUID);
                }
                else
                {
                    AddNewSceneSection(settings, transform, entity, ecb, section.SceneGUID);
                }
            }).Run();

     
        foreach (var (area,transform, section, entity) in SystemAPI.Query<GaiaOperationArea,LocalToWorld, SceneSection>().WithEntityAccess())
        {
            if (section.Section != 0) continue;
            if (sectionDataDictionary.TryGetValue(section.SceneGUID , out var sectionData))
            {
                var positionHashKey = GetSectionID(transform.Position, area.Layer, out float3 center);
                foreach (var data in sectionData)
                {
                    if (data.Layer != area.Layer || !data.Center.Equals(center)) continue;
                    ecb.AddSharedComponent(entity, new SceneSection
                    {
                        SceneGUID = section.SceneGUID,
                        Section = data.Section
                    });
                }
            }
            else
            {
                AddNewSection(sectionData, area.Layer, transform, entity, ecb, section.SceneGUID);
            }
       
        }
        ecb.Playback(EntityManager);
        ecb.Dispose();

    }

    private bool TryUpdateExistingSection(List<SectionData> sectionData, RenderFilterSettings settings,
        LocalToWorld transform, Entity entity, EntityCommandBuffer ecb, Hash128 sceneGuid)
    {
        var positionHashKey = GetSectionID(transform.Position, settings.Layer, out float3 center);

        foreach (var data in sectionData)
        {
            if (data.Layer != settings.Layer || !data.Center.Equals(center)) continue;
            ecb.AddSharedComponent(entity, new SceneSection
            {
                SceneGUID = sceneGuid,
                Section = data.Section
            });
            return true;
        }

        return false;
    }

    private void AddNewSection(List<SectionData> sectionData, RenderFilterSettings settings,
        LocalToWorld transform, Entity entity, EntityCommandBuffer ecb, Hash128 sceneGuid)
    {
        var positionHashKey = GetSectionID(transform.Position, settings.Layer, out var center);
        var newSectionCount = sectionData.IsNullOrEmpty() ? 1: sectionData.Count + 1;

        sectionData.Add(new SectionData
        {
            Layer = settings.Layer,
            Center = center,
            Section = newSectionCount
        });

        ecb.AddSharedComponent(entity, new SceneSection
        {
            SceneGUID = sceneGuid,
            Section = newSectionCount
        });
        ecb.AddComponent(entity, new GaiaOperationArea()
        {
            Radius = settings.Layer switch
            {
                6 => 750,
                9 or 10 or 11=>500,
                26 => 250,
                27 => 100,
                28 => 85,
                _ => 2250
            },
            Center = center
        });
    }
    private void AddNewSection(List<SectionData> sectionData, int Layer,
        LocalToWorld transform, Entity entity, EntityCommandBuffer ecb, Hash128 sceneGuid)
    {
        if (sectionData.IsNullOrEmpty())  
            sectionData = new List<SectionData>();
        var positionHashKey = GetSectionID(transform.Position, Layer, out var center);
        var newSectionCount = sectionData.IsNullOrEmpty() ? 1: sectionData.Count + 1;

        sectionData.Add(new SectionData
        {
            Layer = Layer,
            Center = center,
            Section = newSectionCount
        });

        ecb.AddSharedComponent(entity, new SceneSection
        {
            SceneGUID = sceneGuid,
            Section = newSectionCount
        });
        ecb.AddComponent(entity, new GaiaOperationArea()
        {
            Radius = Layer switch
            {
                6 => 750,
                9 or 10 or 11=>500,
                26 => 250,
                27 => 100,
                28 => 85,
                _ => 2250
            },
            Center = center
        });
    }

    private void AddNewSceneSection(RenderFilterSettings settings, LocalToWorld transform, Entity entity,
        EntityCommandBuffer ecb, Hash128 sceneGuid)
    {
        var sectionData = new List<SectionData>();
        sectionDataDictionary.Add(sceneGuid, sectionData);

        AddNewSection(sectionData, settings, transform, entity, ecb, sceneGuid);
    }
    
}
    


public struct SectionData 
{
    public int Layer;
    public float3 Center;
    public int Section;
    
}
