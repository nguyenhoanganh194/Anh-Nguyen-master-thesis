
using Unity.Entities;
using UnityEngine;

namespace DOP
{
    [CreateAfter(typeof(SpawnerSystem))]
    public partial struct TrailRenderingSystem : ISystem
    {
        private const string quadObject = "Quad";
        private int mapSize;
        private bool initialized;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Pheromone>();
            state.RequireForUpdate<MapConfig>();
            initialized = false;
        }

        void Initialize()
        {
            if (initialized)
                return;

            var colony = SystemAPI.GetSingleton<MapConfig>();
            mapSize = (int)colony.mapSize;

            var gameObject = GameObject.Find(quadObject);
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            var material = meshRenderer.material;
            var texture2D = new Texture2D(mapSize, mapSize, TextureFormat.RFloat, false);
            material.mainTexture = texture2D;

            var transform = gameObject.GetComponent<Transform>();
            transform.localScale = new Vector3(mapSize, mapSize, 1);
            transform.localPosition = new Vector3(mapSize / 2, mapSize / 2, 0);

            initialized = true;
        }

        public void OnUpdate(ref SystemState state)
        {
            Initialize();
            var gameObject = GameObject.Find(quadObject);
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            var material = meshRenderer.material;
            var texture2D = material.mainTexture as Texture2D;

            var pheromones = SystemAPI.GetSingletonBuffer<Pheromone>();
            texture2D.SetPixelData(pheromones.AsNativeArray(), 0, 0);
            texture2D.Apply();
        }
    }
}

