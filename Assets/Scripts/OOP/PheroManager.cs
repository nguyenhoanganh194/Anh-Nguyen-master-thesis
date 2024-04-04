using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheroManager : MonoBehaviour
{
    public float[] pheromones;
    public Material basePheromoneMaterial;
    public Renderer pheromoneRenderer;

    public float trailDecay = 0.999f;

    public void SetUpPheromones(int mapSize)
    {
        var material = pheromoneRenderer.material;
        var texture2D = new Texture2D(mapSize, mapSize, TextureFormat.RFloat, false);
        material.mainTexture = texture2D;
        pheromones = new float[mapSize * mapSize];
    }

    public void UpdatePheromones()
    {
        for (int i = 0; i < pheromones.Length; i++)
        {
            pheromones[i] *= trailDecay;
        }
        var texture2D = pheromoneRenderer.material.mainTexture as Texture2D;
        texture2D.SetPixelData(pheromones, 0, 0);
        texture2D.Apply();
    }
}
