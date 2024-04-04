using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOP
{
    public struct PheromoneConfig : IComponentData
    {
        public float pheromoneSteerStrength;
        public float pheromoneSteerDistance;


        public float pheromoneGrowthRate;
        public float pheromoneDecayRate;
    }
    

    [Serializable]
    public struct MapConfig : IComponentData
    {
        public float mapSize;
        public Entity homePrefab;
        public Entity resourcePrefab;
    }

    public struct AntConfig : IComponentData
    {
        public float antTargetSpeed;
        public float antAccel;
        public int antCount;
        public float antScale;


        public float randomSteerStrength;
        public float resourceSteerStrength;

        public Entity antPrefab;
    }
    public struct ObstaclesConfig : IComponentData
    {
        public float obstacleSize;
        public int ringCount;
        public int bucketResolution;

        public float wallSteerStrength;
        public float wallSteerDistance;
        public float wallPushbackUnits;

        public Entity obstaclePrefab;
    }
}
