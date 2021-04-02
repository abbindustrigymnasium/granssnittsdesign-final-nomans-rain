using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class noiseImageUI : MonoBehaviour
{
    public RawImage[] imageComponents;
    public Texture2D[] textures;
    CelestialBodyManager celestialBody;

    void Start()
    {
        celestialBody = GameObject.Find("Planet").GetComponent<CelestialBodyManager>();
    }

    void Update()
    {
        //        imageComponents = new RawImage[celestialBody.terrainProjectionGPU.noiseSampleTexture2D.Length];
        //        Debug.Log(celestialBody.shapeSettings.noiseLayers.Length);
        for (int i = 0; i < celestialBody.terrainProjectionGPU.noiseSampleTexture2D.Length; i++)
        {
            //            textures[i] = celestialBody.terrainProjectionGPU.noiseSampleTexture2D[i];
            try
            {
                imageComponents[i].texture = celestialBody.terrainProjectionGPU.noiseSampleTexture2D[i];
            }
            catch
            {
                // Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH"); ASDKA SLJDASL JNDAS
            }
        }
    }
}
