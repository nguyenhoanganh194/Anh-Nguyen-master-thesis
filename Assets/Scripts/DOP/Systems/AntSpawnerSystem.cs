using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;


namespace DOP
{
    public partial struct AntSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AntConfig>();
            state.RequireForUpdate<MapConfig>();
            Debug.Log("AntSpawnerSystem OnCreate");
        }

        public void OnUpdate(ref SystemState state)
        {
            var antConfig = SystemAPI.GetSingleton<AntConfig>();
            var mapConfig = SystemAPI.GetSingleton<MapConfig>();
            if (Preloader.Instance == null)
            {
                Debug.LogError("Preloader.Instance is null");
                return;
            }

            if (int.TryParse(Preloader.Instance.numberOfAntInput.text, out var antCount))
            {
                antConfig.antCount = antCount;
            }
            Preloader.Instance.numberOfAntInput.text = antConfig.antCount.ToString();

            state.EntityManager.Instantiate(antConfig.antPrefab, antConfig.antCount, Allocator.Temp);
            var mapSize = mapConfig.mapSize;
            foreach (var (position, direction, localTransform, speed) in SystemAPI.Query<RefRW<Position>, RefRW<Direction>, RefRW<LocalTransform>, RefRW<Speed>>().WithAll<Ant>())
            {
                position.ValueRW.position = new float2(Random.Range(-5f, 5f) + mapSize * 0.5f, Random.Range(-5f, 5f) + mapSize * 0.5f);
                direction.ValueRW.direction = Random.Range(0, 360);
                speed.ValueRW.speed = antConfig.antTargetSpeed;
                localTransform.ValueRW.Scale = antConfig.antScale;
            }


            state.Enabled = false;
        }

    }
}
