
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;
using Color = UnityEngine.Color;

namespace OOP
{


    public class OOPGameManager : MonoBehaviour
    {
        public AntManager antConfig;
        public PheroManager pheroConfig;
        public ColonyManager colonyConfig;
        public ObstacleManager obstacleConfig;

        public int mapSize = 128;

        private void OnValidate()
        {
            antConfig = GetComponent<AntManager>();
            pheroConfig = GetComponent<PheroManager>();
            colonyConfig = GetComponent<ColonyManager>();
            obstacleConfig = GetComponent<ObstacleManager>();
        }

        public static OOPGameManager Instance;
        void Awake()
        {
            Instance = this;
        }

        IEnumerator Start()
        {
            float antCount = 100;
            mapSize = 128;
            while (Preloader.Instance == null)
            {
                yield return null;
            }
            if (int.TryParse(Preloader.Instance.numberOfAntInput.text, out var number))
            {
                antCount = number;
            }
            else
            {
                Preloader.Instance.numberOfAntInput.text = antCount.ToString();
            }

            if (int.TryParse(Preloader.Instance.mapInput.text, out number))
            {
                mapSize = number;
            }
            else
            {
                Preloader.Instance.mapInput.text = mapSize.ToString();
            }

            pheroConfig.SetUpPheromones(mapSize);
            obstacleConfig.SetUp(mapSize);
            colonyConfig.SpawnHomeAndResource(mapSize);
            antConfig.SpawnAnts((int)antCount, mapSize);
        }

        void FixedUpdate()
        {
            antConfig.UpdateAnts();
            pheroConfig.UpdatePheromones();
        }
       
    }
}
