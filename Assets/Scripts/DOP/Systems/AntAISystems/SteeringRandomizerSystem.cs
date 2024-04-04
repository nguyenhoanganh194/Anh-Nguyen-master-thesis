using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace DOP
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct AntRandomSteerSystem : ISystem
    {
        private NativeArray<Random> rngs;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Ant>();

            rngs = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < JobsUtility.MaxJobThreadCount; i++)
            {
                rngs[i] = new Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var antConfig = SystemAPI.GetSingleton<AntConfig>();

            // SteeringRandomizer
            var steeringJob = new SteeringRandomizerJob
            {
                rngs = rngs,
                steeringStrength = antConfig.randomSteerStrength
            };
            var steeringJobHandle = steeringJob.ScheduleParallel(state.Dependency);

            steeringJobHandle.Complete();
        }
    }

    [BurstCompile]
    [WithAll(typeof(Ant))]
    public partial struct SteeringRandomizerJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Random> rngs;
        public float steeringStrength;

        [NativeSetThreadIndex] private int threadId;

        [BurstCompile]
        public void Execute(ref Direction direction)
        {
            var rng = rngs[threadId];
            direction.direction += rng.NextFloat(-steeringStrength, steeringStrength);
            rngs[threadId] = rng;
        }
    }

}

