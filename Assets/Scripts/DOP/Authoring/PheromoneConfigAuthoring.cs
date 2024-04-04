using DOP;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DOP
{
    public class PheromoneConfigAuthoring : MonoBehaviour
    {
        public float pheromoneSteerStrength;
        public float pheromoneSteerDistance;

        public float pheromoneGrowthRate;
        public float pheromoneDecayRate;
        class Baker : Baker<PheromoneConfigAuthoring>
        {
            public override void Bake(PheromoneConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var pheromoneConfig = new PheromoneConfig();
                pheromoneConfig.pheromoneGrowthRate = authoring.pheromoneGrowthRate;
                pheromoneConfig.pheromoneDecayRate = authoring.pheromoneDecayRate;

                pheromoneConfig.pheromoneSteerStrength = authoring.pheromoneSteerStrength;
                pheromoneConfig.pheromoneSteerDistance = authoring.pheromoneSteerDistance;
                AddComponent(entity, pheromoneConfig);
            }
        }
    }

    
}
