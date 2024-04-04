using System;
using UnityEngine;
using Unity.Entities;

namespace DOP
{
    public class FoodAuthoring : MonoBehaviour
    {
        public class Baked : Baker<FoodAuthoring>
        {
            public override void Bake(FoodAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                AddComponent<Food>(entity);
            }
        }
    }
}

