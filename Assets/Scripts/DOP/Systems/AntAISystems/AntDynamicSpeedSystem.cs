
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
    [UpdateAfter(typeof(AntObstacleDetectionSystem))]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct AntDynamicSpeedSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapConfig>();
            state.RequireForUpdate<Ant>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var colonyConfig = SystemAPI.GetSingleton<MapConfig>();
            var antConfig = SystemAPI.GetSingleton<AntConfig>();
            // Dynamics
            var dynamicsJob = new AntDynamicUpdateJob
            {
                mapSize = colonyConfig.mapSize,
                antAcceleration = antConfig.antAccel,
                antTargetSpeed = antConfig.antTargetSpeed
            };
            var dynamicsJobHandle = dynamicsJob.ScheduleParallel(state.Dependency);
            dynamicsJobHandle.Complete();
        }
    }



    [BurstCompile]
    [WithAll(typeof(Ant))]
    public partial struct AntDynamicUpdateJob : IJobEntity
    {
        public float mapSize;
        public float antAcceleration;
        public float antTargetSpeed;

        [BurstCompile]
        public void Execute(
            ref Position position,
            ref Direction direction,
            ref Speed speed,
            ref LocalTransform localTransform,
            in Ant ant)
        {
            // Factor in the steering values
            if (ant.resourceSteering > math.EPSILON)
                direction.direction += ant.resourceSteering;
            else
                direction.direction += ant.wallSteering + ant.pheroSteering + ant.resourceSteering;

            while (direction.direction > 180f)
                direction.direction -= 360f;

            while (direction.direction < -180f)
                direction.direction += 360f;

            var steeringInRad = (ant.wallSteering + ant.pheroSteering + ant.resourceSteering) / 180f * math.PI;
            var oldSpeed = speed.speed;
            var targetSpeed = antTargetSpeed;
            targetSpeed *= 1f - Mathf.Abs(steeringInRad) / 3f;
            speed.speed += (targetSpeed - oldSpeed) * antAcceleration;

            var directionRad = direction.direction / 180f * math.PI;
            localTransform.Rotation = quaternion.Euler(0, 0, directionRad);

            var oldPosition = position.position;
            var speedValue = speed.speed;
            var deltaPos = new float2(
                (float)(speedValue * math.cos(directionRad)),
                (float)(speedValue * math.sin(directionRad)));
            var newPosition = oldPosition + deltaPos;

            if (newPosition.x < 0f || newPosition.x > mapSize || newPosition.y < 0f || newPosition.y > mapSize)
                direction.direction = direction.direction + 180;
            else
            {
                position.position = newPosition;
                localTransform.Position = new float3(newPosition.x, newPosition.y, 0);
            }

        }
    }
}

