using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace DOP
{
    

    public class MapConfigAuthoring : MonoBehaviour
    {
        public GameObject homePrefab;
        public GameObject resourcePrefab;
        public float mapSize;


        class Baker : Baker<MapConfigAuthoring>
        {
            public override void Bake(MapConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var colony = new MapConfig();
                colony.mapSize = authoring.mapSize;
                colony.homePrefab = GetEntity(authoring.homePrefab, TransformUsageFlags.Renderable);
                colony.resourcePrefab = GetEntity(authoring.resourcePrefab, TransformUsageFlags.Renderable);

                AddComponent(entity, colony);
                AddBuffer<WallBucket>(entity);
            }
        }
    }
    [ChunkSerializable]
    public struct WallBucket : IBufferElementData
    {
        public UnsafeList<float2> obstacles;
    }

}

