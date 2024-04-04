
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace OOP
{
    public class Ant : MonoBehaviour
    {

        public OOPGameManager AntManager => OOPGameManager.Instance;

        public static Matrix4x4[] rotationMatrixLookup;
        public static int rotationResolution = 360;

        public Vector2 savedPostion; //8bytes
        public MeshRenderer myMesh;

        [SerializeField]
        private float mapScale = 0.078f;

        [SerializeField]
        private float trailAddSpeed, antAccel, antSpeed;
        [SerializeField]
        private float randomSteering, pheromoneSteerStrength, wallSteerStrength, goalSteerStrength;

        private float facingAngle , speed;

        private bool holdingResource;
        public Color redColor, yellowColor;
        public void SetUp(Vector2 pos)
        {
            savedPostion = pos;
            facingAngle = Random.value * Mathf.PI * 2f;
            speed = 0f;
            holdingResource = false;
            transform.position = savedPostion * mapScale;
        }

        float PheromoneSteering(float distance)
        {
            float output = 0;

            for (int i = -1; i <= 1; i += 2)
            {
                float angle = facingAngle + i * Mathf.PI * .25f;
                float testX = savedPostion.x + Mathf.Cos(angle) * distance;
                float testY = savedPostion.y + Mathf.Sin(angle) * distance;

                if (testX < 0 || testY < 0 || testX >= AntManager.mapSize || testY >= AntManager.mapSize)
                {

                }
                else
                {
                    int index = PheromoneIndex((int)testX, (int)testY);
                    float value = AntManager.pheroConfig.pheromones[index];
                    output += value * i;
                }
            }
            return Mathf.Sign(output);
        }
        int PheromoneIndex(int x, int y)
        {
            return x + y * AntManager.mapSize;
        }
        int WallSteering(float distance)
        {
            int output = 0;

            for (int i = -1; i <= 1; i += 2)
            {
                float angle = facingAngle + i * Mathf.PI * .25f;
                float testX = savedPostion.x + Mathf.Cos(angle) * distance;
                float testY = savedPostion.y + Mathf.Sin(angle) * distance;

                if (testX < 0 || testY < 0 || testX >= AntManager.mapSize || testY >= AntManager.mapSize)
                {

                }
                else
                {
                    int value = AntManager.obstacleConfig.GetObstacleBucket(testX, testY).Length;
                    if (value > 0)
                    {
                        output -= i;
                    }
                }
            }
            return output;
        }

        bool Linecast(Vector2 point1, Vector2 point2)
        {
            float dx = point2.x - point1.x;
            float dy = point2.y - point1.y;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            int stepCount = Mathf.CeilToInt(dist * .5f);
            for (int i = 0; i < stepCount; i++)
            {
                float t = (float)i / stepCount;
                if (AntManager.obstacleConfig.GetObstacleBucket(point1.x + dx * t, point1.y + dy * t).Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        void DropPheromones(float targetSpeed)
        {
            float excitement = .3f;
            if (holdingResource)
            {
                excitement = 1f;
            }
            excitement *= speed / targetSpeed;
            int x = Mathf.FloorToInt(savedPostion.x);
            int y = Mathf.FloorToInt(savedPostion.y);
            if (x < 0 || y < 0 || x >= AntManager.mapSize || y >= AntManager.mapSize)
            {
                return;
            }

            int index = PheromoneIndex(x, y);
            AntManager.pheroConfig.pheromones[index] += trailAddSpeed * excitement * (1f - AntManager.pheroConfig.pheromones[index]) * Time.fixedDeltaTime;
        }

        float RandomSteering()
        {
             return Random.Range(-randomSteering, randomSteering);
        }


        Vector2 CheckState()
        {
            Vector2 targetPos;
            if (holdingResource == false)
            {
                targetPos = AntManager.colonyConfig.resourcePosition;
            }
            else
            {
                targetPos = AntManager.colonyConfig.colonyPosition;
            }

           

            if (holdingResource)
            {
                myMesh.material.color = yellowColor;
            }
            else
            {
                myMesh.material.color = redColor;
            }

            if ((savedPostion - targetPos).sqrMagnitude < 4f * 4f)
            {
                holdingResource = !holdingResource;
                facingAngle += Mathf.PI;
            }
            return targetPos;
        }

        void CheckIfTargetInLineOfSight(Vector2 target)
        {
            if (Linecast(savedPostion, target) == false)
            {
                float targetAngle = Mathf.Atan2(target.y - savedPostion.y, target.x - savedPostion.x);
                if (targetAngle - facingAngle > Mathf.PI)
                {
                    facingAngle += Mathf.PI * 2f;
                }
                else if (targetAngle - facingAngle < -Mathf.PI)
                {
                    facingAngle -= Mathf.PI * 2f;
                }
                else
                {
                    if (Mathf.Abs(targetAngle - facingAngle) < Mathf.PI * .5f)
                    {
                        facingAngle += (targetAngle - facingAngle) * goalSteerStrength;
                    }
                }
            }
        }

        Vector2 GetCurrentVelocity()
        {
            Vector2 velocity = new Vector2(Mathf.Cos(facingAngle), Mathf.Sin(facingAngle)) * speed;
            if (savedPostion.x + velocity.x < 0f || savedPostion.x + velocity.x > AntManager.mapSize)
            {
                velocity.x = -velocity.x;
            }
            else
            {
                savedPostion.x += velocity.x;
            }
            if (savedPostion.y + velocity.y < 0f || savedPostion.y + velocity.y > AntManager.mapSize)
            {
                velocity.y = -velocity.y;
            }
            else
            {
                savedPostion.y += velocity.y;
            }
            return velocity;
        }


        void BlockAntFromPassingObstacle(ref Vector2 velocity)
        {

            Vector2 distance = Vector2.zero;
            float dist;

            Obstacle[] nearbyObstacles = AntManager.obstacleConfig.GetObstacleBucket(savedPostion);

            for (int j = 0; j < nearbyObstacles.Length; j++)
            {
                Obstacle obstacle = nearbyObstacles[j];
                distance.x = savedPostion.x - obstacle.position.x;
                distance.y = savedPostion.y - obstacle.position.y;
                dist = distance.magnitude;
                if (dist < AntManager.obstacleConfig.obstacleRadius)
                {
                    distance.x /= dist;
                    distance.y /= dist;
                    savedPostion.x = obstacle.position.x + distance.x * AntManager.obstacleConfig.obstacleRadius;
                    savedPostion.y = obstacle.position.y + distance.y * AntManager.obstacleConfig.obstacleRadius;

                    velocity.x -= distance.x * (distance.x * velocity.x + distance.y * velocity.y) * 1.5f;
                    velocity.y -= distance.y * (distance.x * velocity.x + distance.y * velocity.y) * 1.5f;
                }
            }

            float inwardOrOutward = -AntManager.antConfig.outwardStrength;
            float pushRadius = AntManager.mapSize * .4f;
            if (holdingResource)
            {
                inwardOrOutward = AntManager.antConfig.inwardStrength;
                pushRadius = AntManager.mapSize;
            }
            distance.x = AntManager.colonyConfig.colonyPosition.x - savedPostion.x;
            distance.y = AntManager.colonyConfig.colonyPosition.y - savedPostion.y;

            dist = distance.magnitude;
            inwardOrOutward *= 1f - Mathf.Clamp01(dist / pushRadius);
            velocity.x += distance.x / dist * inwardOrOutward;
            velocity.y += distance.y / dist * inwardOrOutward;
        }

        void MoveAlongObstacleBucket()
        {
            var velocity = GetCurrentVelocity();
            var oldVelocity = velocity;
            BlockAntFromPassingObstacle(ref velocity);
            if (oldVelocity.x != velocity.x || oldVelocity.y != velocity.y)
            {
                facingAngle = Mathf.Atan2(velocity.y, velocity.x);
            }

        }


        public void AntFixedUpdate()
        {
            float targetSpeed = antSpeed;
            float pheroSteering = PheromoneSteering(3f);
            int wallSteering = WallSteering(1.5f);

            facingAngle += RandomSteering();
            Vector2 targetPosition = CheckState();


            facingAngle += pheroSteering * pheromoneSteerStrength;
            CheckIfTargetInLineOfSight(targetPosition);
            facingAngle += wallSteering * wallSteerStrength;
            MoveAlongObstacleBucket();
          

            targetSpeed *= 1f - (Mathf.Abs(pheroSteering) + Mathf.Abs(wallSteering)) / 3f;
            speed += (targetSpeed - speed) * antAccel;

            DropPheromones(targetSpeed);

            transform.position = new Vector3(savedPostion.x, savedPostion.y, 0) * mapScale;
            transform.rotation = Quaternion.Euler(0, 0, facingAngle * Mathf.Rad2Deg);
        }
    }
}

