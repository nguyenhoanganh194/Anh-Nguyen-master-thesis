using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace DOP
{

    [UpdateAfter(typeof(AntPheromoneDetectionSystem))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct AntObstacleDetectionSystem : ISystem
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
            var obstacles = SystemAPI.GetSingleton<ObstaclesConfig>();        

            var obstacleJob = new ObstacleDetection
            {
                distance = obstacles.wallSteerDistance,
                obstacleSize = obstacles.obstacleSize,
                mapSize = colony.mapSize,
                steeringStrength = obstacles.wallSteerStrength,
                bucketResolution = obstacles.bucketResolution,
                wallBuckets = SystemAPI.GetSingletonBuffer<WallBucket>().AsNativeArray(),
                wallPushbackUnits = obstacles.wallPushbackUnits
            };
            var obstacleJobHandle = obstacleJob.ScheduleParallel(state.Dependency);
            obstacleJobHandle.Complete();
        }
    }



    [BurstCompile]
    [WithAll(typeof(Ant))]
    public partial struct ObstacleDetection : IJobEntity
    {
        public float distance;
        public float mapSize;
        public float obstacleSize;
        public float steeringStrength;
        public int bucketResolution;
        public float wallPushbackUnits;
        [ReadOnly]
        public NativeArray<WallBucket> wallBuckets;

        public static bool DetectPositionInBuckets(float x, float y, in NativeArray<WallBucket> walls, float obstacleSize, float mapSize, int bucketResolution, out float obstacleX, out float obstacleY)
        {
            obstacleX = 0;
            obstacleY = 0;
            // test map boundaries
            if (x < 0 || y < 0 || x >= mapSize || y >= mapSize)
            {
                return true;
            }
            else
            {
                int xIndex = (int)(x / mapSize * bucketResolution);
                int yIndex = (int)(y / mapSize * bucketResolution);
                if (xIndex < 0 || yIndex < 0 || xIndex >= bucketResolution || yIndex >= bucketResolution)
                {
                    return false; // ???
                }
                var obstacles = walls[xIndex + yIndex * bucketResolution];
                foreach (var obstaclePosition in obstacles.obstacles)
                {
                    obstacleX = obstaclePosition.x;
                    obstacleY = obstaclePosition.y;
                    if (math.pow(x - obstacleX, 2) + math.pow(y - obstacleY, 2) <= math.pow(obstacleSize, 2))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Execute(Entity entity, ref Ant ant, ref Position position, in Direction direction)
        {
            int output = 0;

            var directionInRadians = direction.direction / 180f * (float)math.PI;

            // this for loop makes us check the direction * -1 and * 1
            for (int i = -1; i <= 1; i += 2)
            {
                float angle = directionInRadians + i * math.PI * 0.25f;
                float testX = position.position.x + math.cos(angle) * distance;
                float testY = position.position.y + math.sin(angle) * distance;

                float obstacleX, obstacleY;
                if (DetectPositionInBuckets(testX, testY, wallBuckets, obstacleSize, mapSize, bucketResolution, out obstacleX, out obstacleY))
                {
                    output -= i;

                    // Move the ant away from the obstacle 
                    var dx = position.position.x - obstacleX;
                    var dy = position.position.y - obstacleY;
                    var pushbackAngle = math.atan2(dy, dx);

                    position.position.x += math.cos(pushbackAngle) * wallPushbackUnits;
                    position.position.y += math.sin(pushbackAngle) * wallPushbackUnits;

                }
            }

            ant.wallSteering = output * steeringStrength / (float)math.PI * 180f;
        }
    }
}
