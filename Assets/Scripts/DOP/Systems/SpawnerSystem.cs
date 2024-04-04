using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DOP
{
    public partial struct SpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapConfig>();
            state.RequireForUpdate<WallBucket>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var colony = SystemAPI.GetSingleton<MapConfig>();
            if (Preloader.Instance == null)
            {
                Debug.LogError("Preloader.Instance is null");
                return;
            }

            if (float.TryParse(Preloader.Instance.mapInput.text, out var mapSize))
            {
                colony.mapSize = mapSize;
            }

            Preloader.Instance.mapInput.text = colony.mapSize.ToString();

            SpawnHome(ref state, colony);
            SpawnResource(ref state, colony);
            SpawnPheromones(ref state, colony);
            state.Enabled = false;
        }

        void SpawnHome(ref SystemState state, MapConfig colony)
        {
            var home = state.EntityManager.Instantiate(colony.homePrefab);
            var localTransform = SystemAPI.GetComponentRW<LocalTransform>(home);
            localTransform.ValueRW.Position = new float3(colony.mapSize / 2f, colony.mapSize / 2f, 0f);
            state.EntityManager.AddComponent<Home>(home);
        }

        void SpawnResource(ref SystemState state, MapConfig colony)
        {
            var resource = state.EntityManager.Instantiate(colony.resourcePrefab);
            float mapSize = colony.mapSize;

            float resourceAngle = Random.value * 2f * Mathf.PI;
            var localTransform = SystemAPI.GetComponentRW<LocalTransform>(resource);
            localTransform.ValueRW.Position = new float3(mapSize * 0.5f + Mathf.Cos(resourceAngle) * mapSize * 0.475f, mapSize * 0.5f + Mathf.Sin(resourceAngle) * mapSize * 0.475f, 0);
        }

       

        void SpawnPheromones(ref SystemState state, MapConfig colony)
        {
            var pheromones = state.EntityManager.CreateEntity();
            var pheromonesBuffer = state.EntityManager.AddBuffer<Pheromone>(pheromones);
            pheromonesBuffer.Length = (int)colony.mapSize * (int)colony.mapSize;
            for (var i = 0; i < pheromonesBuffer.Length; i++)
            {
                pheromonesBuffer[i] = new Pheromone { strength = 0f };
            }
        }
    }

}
