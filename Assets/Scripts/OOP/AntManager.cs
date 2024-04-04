using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

namespace OOP
{ 
    public class AntManager : MonoBehaviour
    {
        public Ant antPrefab;
        public Vector3 antSize;
        public float outwardStrength;
        public float inwardStrength;



        public Ant[] ants;

        public void SpawnAnts(int antCount, float mapSize = 128)
        {
            Ant.rotationMatrixLookup = new Matrix4x4[Ant.rotationResolution];
            for (int i = 0; i < Ant.rotationResolution; i++)
            {
                float angle = (float)i / Ant.rotationResolution;
                angle *= 360f;
                Ant.rotationMatrixLookup[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), antSize);
            }


            ants = new Ant[antCount];


            //int batch = 40;
            //int start = 0;

            //var id = new List<int>();
            //while (start < batch)
            //{
            //    for (int i = start; i < antCount; i += batch)
            //    {
            //        var ant = Instantiate(antPrefab);
            //        ant.SetUp(new Vector3(Random.Range(-5f, 5f) + mapSize * .5f, Random.Range(-5f, 5f) + mapSize * .5f, 0f));

            //        ants[i] = ant;
            //        id.Add(i);
            //    }
            //    start++;
            //}
            //Debug.LogError($"Estimated size of ant: {Marshal.SizeOf(ants[0])} bytes");
            //Debug.LogError($"Index: {string.Join(" ", id)}");
            //Debug.LogError($"Count: {id.Count}");

            for (int i = 0; i < antCount; i++)
            {
                var ant = Instantiate(antPrefab);
                ant.SetUp(new Vector3(Random.Range(-5f, 5f) + mapSize * .5f, Random.Range(-5f, 5f) + mapSize * .5f, 0f));
                ants[i] = ant;
            }

        }

        public void UpdateAnts()
        {
            for (int i = 0; i < ants.Length; i++)
            {
                ants[i].AntFixedUpdate();
            }
        }
    }
}