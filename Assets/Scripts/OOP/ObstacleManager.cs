using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OOP
{
    public class ObstacleManager : MonoBehaviour
    {
        public Obstacle obstaclePrefab;
        public int bucketResolution;
        public int obstacleRingCount;
        [Range(0f, 1f)]
        public float obstaclesPerRing;
        public float obstacleRadius;

        public Obstacle[] obstacles;
        public Obstacle[,][] obstacleBuckets;
        public Obstacle[] emptyBucket = new Obstacle[0];

        private int mapSize = 128;

        public void SetUp(int mapSize = 128)
        {
            this.mapSize = mapSize;
            GenerateObstacles();
            UpdateObjstacleBucket();
        }
        

        private void GenerateObstacles()
        {
            List<Obstacle> output = new List<Obstacle>();
            for (int i = 1; i <= obstacleRingCount; i++)
            {
                float ringRadius = (i / (obstacleRingCount + 1f)) * (mapSize * .5f);
                float circumference = ringRadius * 2f * Mathf.PI;
                int maxCount = Mathf.CeilToInt(circumference / (2f * obstacleRadius) * 2f);
                int offset = Random.Range(0, maxCount);
                int holeCount = Random.Range(1, 3);
                for (int j = 0; j < maxCount; j++)
                {
                    float t = (float)j / maxCount;
                    if ((t * holeCount) % 1f < obstaclesPerRing)
                    {
                        float angle = (j + offset) / (float)maxCount * (2f * Mathf.PI);

                        var position = new Vector2(mapSize * .5f + Mathf.Cos(angle) * ringRadius, mapSize * .5f + Mathf.Sin(angle) * ringRadius);
                        var obstacle = Instantiate(obstaclePrefab, position * 0.0078f, Quaternion.identity, transform);
                        obstacle.position = position;
                        obstacle.radius = obstacleRadius;
                        output.Add(obstacle);
                    }
                }
            }

            obstacles = output.ToArray();
        }
        public Obstacle[] GetObstacleBucket(Vector2 pos)
        {
            return GetObstacleBucket(pos.x, pos.y);
        }
        public Obstacle[] GetObstacleBucket(float posX, float posY)
        {
            int x = (int)(posX / mapSize * bucketResolution);
            int y = (int)(posY / mapSize * bucketResolution);
            if (x < 0 || y < 0 || x >= bucketResolution || y >= bucketResolution)
            {
                return emptyBucket;
            }
            else
            {
                return obstacleBuckets[x, y];
            }
        }



        private void UpdateObjstacleBucket()
        {
            List<Obstacle>[,] tempObstacleBuckets = new List<Obstacle>[bucketResolution, bucketResolution];

            for (int x = 0; x < bucketResolution; x++)
            {
                for (int y = 0; y < bucketResolution; y++)
                {
                    tempObstacleBuckets[x, y] = new List<Obstacle>();
                }
            }

            for (int i = 0; i < obstacles.Length; i++)
            {
                Vector2 pos = obstacles[i].position;
                float radius = obstacles[i].radius;
                for (int x = Mathf.FloorToInt((pos.x - radius) / mapSize * bucketResolution); x <= Mathf.FloorToInt((pos.x + radius) / mapSize * bucketResolution); x++)
                {
                    if (x < 0 || x >= bucketResolution)
                    {
                        continue;
                    }
                    for (int y = Mathf.FloorToInt((pos.y - radius) / mapSize * bucketResolution); y <= Mathf.FloorToInt((pos.y + radius) / mapSize * bucketResolution); y++)
                    {
                        if (y < 0 || y >= bucketResolution)
                        {
                            continue;
                        }
                        tempObstacleBuckets[x, y].Add(obstacles[i]);
                    }
                }
            }

            obstacleBuckets = new Obstacle[bucketResolution, bucketResolution][];
            for (int x = 0; x < bucketResolution; x++)
            {
                for (int y = 0; y < bucketResolution; y++)
                {
                    obstacleBuckets[x, y] = tempObstacleBuckets[x, y].ToArray();
                }
            }
        }
    }

}