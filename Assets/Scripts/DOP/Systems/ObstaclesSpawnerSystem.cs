using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;


namespace DOP
{
    public partial struct ObstaclesSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AntConfig>();
            state.RequireForUpdate<MapConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var colony = SystemAPI.GetSingleton<MapConfig>();
            var obstacleConfig = SystemAPI.GetSingleton<ObstaclesConfig>();


            float mapSize = colony.mapSize;
            int ringCount = obstacleConfig.ringCount;
            float obstacleRadius = obstacleConfig.obstacleSize;
            float maxFillRatio = 0.8f;

            NativeList<float2> obstaclePositions = new NativeList<float2>(Allocator.Temp);

            for (int i = 1; i <= ringCount; ++i)
            {
                float ringRadius = (i / (ringCount + 1f)) * (mapSize * 0.5f);
                float circumference = ringRadius * 2f * Mathf.PI;
                int maxCount = Mathf.CeilToInt(circumference / (2f * obstacleRadius) * 2f);
                int offset = UnityEngine.Random.Range(0, maxCount);
                int holeCount = UnityEngine.Random.Range(1, 3);

                for (int j = 0; j < maxCount; ++j)
                {
                    float fillRatio = (float)j / maxCount;
                    if (((fillRatio * holeCount) % 1f) < maxFillRatio)
                    {
                        float angle = (j + offset) / (float)maxCount * (2f * Mathf.PI);
                        var obstacle = state.EntityManager.Instantiate(obstacleConfig.obstaclePrefab);

                        float2 obstaclePosition = new float2(mapSize * 0.5f + Mathf.Cos(angle) * ringRadius, mapSize * 0.5f + Mathf.Sin(angle) * ringRadius);

                        var localTransform = SystemAPI.GetComponentRW<LocalTransform>(obstacle);
                        localTransform.ValueRW.Position = new float3(obstaclePosition.x, obstaclePosition.y, 0);
                        obstaclePositions.Add(obstaclePosition);
                    }
                }
            }

            int bucketResolution = obstacleConfig.bucketResolution;
            var buckets = SystemAPI.GetSingletonBuffer<WallBucket>();
            buckets.Length = bucketResolution * bucketResolution;
            for (int i = 0; i < buckets.Length; ++i)
            {
                buckets[i] = new WallBucket { obstacles = new UnsafeList<float2>(0, Allocator.Persistent) };
            }
            foreach (var position in obstaclePositions)
            {
                float radius = obstacleConfig.obstacleSize;
                for (int x = Mathf.FloorToInt((position.x - radius) / mapSize * bucketResolution); x <= Mathf.FloorToInt((position.x + radius) / mapSize * bucketResolution); x++)
                {
                    if (x < 0 || x >= bucketResolution)
                    {
                        continue;
                    }
                    for (int y = Mathf.FloorToInt((position.y - radius) / mapSize * bucketResolution); y <= Mathf.FloorToInt((position.y + radius) / mapSize * bucketResolution); y++)
                    {
                        if (y < 0 || y >= bucketResolution)
                        {
                            continue;
                        }
                        int index = x + y * bucketResolution;
                        var list = buckets[index].obstacles;
                        list.Add(position);
                        buckets[index] = new WallBucket { obstacles = list };
                    }
                }
            }


            state.Enabled = false;
        }

    }
}
