using UnityEngine;
using Unity.Entities;
using Unity.Rendering;

namespace DOP
{
    public class AntAuthoring : MonoBehaviour
    {
        public Ant ant;
        public Position position;
        public Speed speed;
        public Direction direction;

        class Baker : Baker<AntAuthoring>
        {
            public override void Bake(AntAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent(entity, authoring.ant);
                AddComponent(entity, authoring.position);
                AddComponent(entity, authoring.speed);
                AddComponent(entity, authoring.direction);
                AddComponent(entity, new URPMaterialPropertyBaseColor());
            }
        }
    }
}

