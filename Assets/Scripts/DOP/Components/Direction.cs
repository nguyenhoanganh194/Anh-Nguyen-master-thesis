using Unity.Entities;
using Unity.Mathematics;

namespace DOP
{
    public struct Direction : IComponentData
    {
        public float direction; // angle around Z
    }
}

