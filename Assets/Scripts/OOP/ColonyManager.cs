using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OOP
{
    public class ColonyManager : MonoBehaviour
    {
        public GameObject homePrefab;
        public GameObject resourcePrefab;
        public int antCount;
        



        public Vector2 resourcePosition;
        public Vector2 colonyPosition;

        private int mapSize = 128;
        public void SpawnHomeAndResource(int mapSize)
        {
            this.mapSize = mapSize;
            colonyPosition = Vector2.one * mapSize * .5f;
            Instantiate(homePrefab, new Vector3(colonyPosition.x, colonyPosition.y, 0f) * 0.0078f, Quaternion.identity, transform);

            float resourceAngle = Random.value * 2f * Mathf.PI;
            resourcePosition = Vector2.one * mapSize * .5f + new Vector2(Mathf.Cos(resourceAngle) * mapSize * .475f, Mathf.Sin(resourceAngle) * mapSize * .475f);
            Instantiate(resourcePrefab, new Vector3(resourcePosition.x, resourcePosition.y, 0f) * 0.0078f, Quaternion.identity, transform);

        }
    }

}