using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace DOP
{
    [CreateAfter(typeof(SpawnerSystem))]
    public partial struct AntRenderingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MapConfig>();
            state.RequireForUpdate<Ant>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Ant color
            var antRenderingJob = new AntRenderingJob();
            state.Dependency = antRenderingJob.Schedule(state.Dependency);
            state.Dependency.Complete();
        }
    }
    [BurstCompile]
    [WithAll(typeof(Ant))]
    public partial struct AntRenderingJob : IJobEntity
    {
        public float mapSize;

        [BurstCompile]
        public void Execute(in Ant ant, ref URPMaterialPropertyBaseColor color)
        {
            if (ant.hasResource)
                color.Value = new Vector4(1, 1, 0, 1);
            else
                color.Value = new Vector4(0, 0, 1, 1);
        }
    }
}


