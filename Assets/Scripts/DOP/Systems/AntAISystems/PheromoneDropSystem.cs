
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DOP
{
    [UpdateAfter(typeof(AntDynamicSpeedSystem))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct AntPheromoneRenderedSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Ant>();
            state.RequireForUpdate<MapConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var colony = SystemAPI.GetSingleton<MapConfig>();
            var pehro = SystemAPI.GetSingleton<PheromoneConfig>();
            var ant = SystemAPI.GetSingleton<AntConfig>();


            var pheromones = SystemAPI.GetSingletonBuffer<Pheromone>();
            var nativePheromones = pheromones.AsNativeArray();
            var pheromoneDropJob = new PheromoneDropJob
            {
                deltaTime = SystemAPI.Time.fixedDeltaTime,
                mapSize = (int)colony.mapSize,
                antTargetSpeed = ant.antTargetSpeed,
                pheromoneGrowthRate = pehro.pheromoneGrowthRate,
                pheromones = nativePheromones
            };
            var pheromoneDropJobHandle = pheromoneDropJob.ScheduleParallel(state.Dependency);
            pheromoneDropJobHandle.Complete();

            // Decay Pheromones
            var pheromoneDecayJob = new PheromoneDecayJob
            {
                pheromoneDecayRate = pehro.pheromoneDecayRate,
                pheromones = pheromones
            };
            state.Dependency = pheromoneDecayJob.Schedule(pheromones.Length, 100, pheromoneDropJobHandle);
        }
    }

    [BurstCompile]
    [WithAll(typeof(Ant))]
    public partial struct PheromoneDropJob : IJobEntity
    {
        public float deltaTime;
        public int mapSize;
        public float antTargetSpeed;
        public float pheromoneGrowthRate;
        [NativeDisableParallelForRestriction]
        public NativeArray<Pheromone> pheromones;

        public void Execute(in Ant ant, in Position position, in Speed speed)
        {
            var strength = ant.hasResource ? 1.0f : 0.3f;
            strength *= speed.speed / antTargetSpeed;

            var gridPosition = math.int2(math.floor(position.position));
            if (gridPosition.x < 0 || gridPosition.y < 0 || gridPosition.x >= mapSize || gridPosition.y >= mapSize)
            {
                return;
            }

            var index = gridPosition.x + gridPosition.y * mapSize;
            var pheromone = pheromones[index];
            pheromone.strength += pheromoneGrowthRate * strength * (1f - pheromone.strength) * deltaTime;
            pheromones[index] = pheromone;
        }
    }

    [BurstCompile]
    public struct PheromoneDecayJob : IJobParallelFor
    {
        public float pheromoneDecayRate;
        [NativeDisableParallelForRestriction]
        public DynamicBuffer<Pheromone> pheromones;

        public void Execute(int index)
        {
            var pheromone = pheromones[index];
            pheromone.strength *= pheromoneDecayRate;
            pheromones[index] = pheromone;
        }
    }

}

