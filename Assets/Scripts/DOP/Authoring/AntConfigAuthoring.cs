using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace DOP
{
    public class AntConfigAuthoring : MonoBehaviour
    {
        public GameObject antPrefab;
        public float antTargetSpeed;
        public float antAccel;
        public int antCount;
        public float antScale;

        public float randomSteerStrength;
        public float resourceSteerStrength;

        class Baker : Baker<AntConfigAuthoring>
        {
            public override void Bake(AntConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var antConfig = new AntConfig();
                antConfig.antTargetSpeed = authoring.antTargetSpeed;
                antConfig.antAccel = authoring.antAccel;
                antConfig.antCount = authoring.antCount;
                antConfig.antScale = authoring.antScale;

                antConfig.randomSteerStrength = authoring.randomSteerStrength;
                antConfig.resourceSteerStrength = authoring.resourceSteerStrength;
                antConfig.antPrefab = GetEntity(authoring.antPrefab, TransformUsageFlags.Renderable);
                AddComponent(entity, antConfig);
            }
        }
    }


    
}
