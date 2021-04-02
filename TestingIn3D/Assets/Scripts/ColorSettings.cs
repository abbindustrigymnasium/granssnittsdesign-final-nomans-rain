using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ColorSettings : ScriptableObject
{
    public Material celestialBodyMaterial;
    public Shore shore;
    public Biomes biomes;
    public Mountains mountains;
    public MaterialProperties materialProperties;

    [System.Serializable]
    public class Shore
    {
        public Color DampShoreColor = new Color(1f, 1f, 1f, 1f);
        public Color DryShoreColor = new Color(1f, 1f, 1f, 1f);

        public float OceanRadius = 5f;
        [Range(0, 1)]
        public float DampShoreHeightAboveWater = 0.1f;
        [Range(0, 1)]
        public float DryShoreHeightAboveWater = 0.1f;
        [Range(0,1)]
        public float OceanBlend = 0.5f;
        [Range(0, 1)]
        public float ShoreBlend = 0.5f;
    }

    [System.Serializable]
    public class Biomes
    {
        public Color BiomeALow;
        public Color BiomeAHigh;

        public Color BiomeBLow;
        public Color BiomeBHigh;

        public Color BiomeCLow;
        public Color BiomeCHigh;

        public Color BiomeDLow;
        public Color BiomeDHigh;

        public float FlatColBlend;
        public float FlatColBlendNoise;
        public float MaxFlatHeight;

        public Texture2D NoiseTex;
        public float NoiseScale;
        public float NoiseScale2;
    }

    [System.Serializable]
    public class Mountains
    {
        public Color MountainLow;
        public Color MountainHigh;

        public float MaxFlatHeight;
        public float SteepnessThreshold;
        public float FlatToSteepBlend;
        public float FlatToSteepNoise;
    }

    [System.Serializable]
    public class MaterialProperties
    {
        public Color FresnelCol;
        public float FresnelStrengthNear;
        public float FresnelStrengthFar;
        public float FresnelPow;
        public float Smoothness;
        public float Metallic;
        public float BodyScale;
        public Vector4 TestParams;
    }
}
