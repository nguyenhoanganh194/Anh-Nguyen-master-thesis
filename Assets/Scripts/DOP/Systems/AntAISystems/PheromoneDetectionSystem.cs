using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace DOP
{
    [UpdateAfter(typeof(AntRandomSteerSystem))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct AntPheromoneDetectionSystem : ISystem
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
            var pheroConfig = SystemAPI.GetSingleton<PheromoneConfig>();
            var pheromones = SystemAPI.GetSingletonBuffer<Pheromone>();

            var pheromoneDetectionJob = new PheromoneDetectionJob
            {
                mapSize = (int)colony.mapSize,
                steeringStrength = pheroConfig.pheromoneSteerStrength,
                distance = pheroConfig.pheromoneSteerDistance,
                pheromones = pheromones
            };
            var pheromoneDetectionJobHandle = pheromoneDetectionJob.ScheduleParallel(state.Dependency);
            pheromoneDetectionJobHandle.Complete();
        }
    }


    [BurstCompile]
    [WithAll(typeof(Ant))]
    public partial struct PheromoneDetectionJob : IJobEntity
    {
        public int mapSize;
        public float steeringStrength;
        public float distance;
        [ReadOnly]
        public DynamicBuffer<Pheromone> pheromones;

        public void Execute(ref Ant ant, in Position position, in Direction direction)
        {
            var output = 0f;
            var directionRadians = direction.direction / 180f * math.PI;

            for (var i = -1; i <= 1; i += 2)
            {
                var angle = directionRadians + i * math.PI * 0.25f;
                var testX = position.position.x + math.cos(angle) * distance;
                var testY = position.position.y + math.sin(angle) * distance;

                if (testX >= 0 && testY >= 0 && testX < mapSize && testY < mapSize)
                {
                    var index = (int)testX + (int)testY * mapSize;
                    var value = pheromones[index].strength;
                    output += value * i;
                }
            }

            ant.pheroSteering = math.sign(output) * steeringStrength;
        }
    }

}

