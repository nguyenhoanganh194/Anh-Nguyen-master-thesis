using Unity.Entities;

namespace DOP
{
    public struct Ant : IComponentData
    {
        public float wallSteering;
        public float pheroSteering;
        public float resourceSteering;
        public bool hasResource;
    }
}

