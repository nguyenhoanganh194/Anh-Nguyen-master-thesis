using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DOP
{
    public class ObstaclesConfigAuthoring : MonoBehaviour
    {
        public GameObject obstaclePrefab;
        public float obstacleSize;
        public int ringCount;
        public int bucketResolution;

        public float wallSteerStrength;
        public float wallSteerDistance;
        public float wallPushbackUnits;
        class Baker : Baker<ObstaclesConfigAuthoring>
        {
            public override void Bake(ObstaclesConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var obstaclesConfig = new ObstaclesConfig();
                obstaclesConfig.obstacleSize = authoring.obstacleSize;
                obstaclesConfig.ringCount = authoring.ringCount;
                obstaclesConfig.bucketResolution = authoring.bucketResolution;

                obstaclesConfig.wallSteerStrength = authoring.wallSteerStrength;
                obstaclesConfig.wallSteerDistance = authoring.wallSteerDistance;
                obstaclesConfig.wallPushbackUnits = authoring.wallPushbackUnits;


                obstaclesConfig.obstaclePrefab = GetEntity(authoring.obstaclePrefab, TransformUsageFlags.Renderable);
                AddComponent(entity, obstaclesConfig);
            }
        }
    }

   
}
