using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ColorSettings : ScriptableObject
{
    public enum PlanetType { FirstPlanet, SecondPlanet, Moon, Sun };
    public PlanetType planetType;

    public FirstPlanet firstPlanet;
    public SecondPlanet secondPlanet;
    public Moon moon;
    public Sun sun;

    [System.Serializable]
    public class FirstPlanet
    {
        [HideInInspector]
        public Material celestialBodyMaterial;
        public Ocean ocean;
        public Atmosphere atmosphere;
        public Shore shore;
        public Biomes biomes;
        public Mountains mountains;

        [System.Serializable]
        public class Triplanar
        {
            public float BlendSharpness;
            public float ScaleNormalMap;
            public Texture2D NormalMap;
        }

        [System.Serializable]
        public class Ocean
        {
            public Color OceanDeep = new Color(1f, 1f, 1f, 1f);
            public Color OceanShallow = new Color(1f, 1f, 1f, 1f);

            public float OceanRadius;

            public float DepthMultiplier;
            public float AlphaMultiplier;

            [Range(0,1)]
            public float Smoothness;

            public bool SelfGlow;

            public float WaveSpeed;
            public Texture2D WaveNormalA;
            public Texture2D WaveNormalB;
            public float WaveNormalScale;
            [Range(0, 1)]
            public float WaveStrength;
            public Color SpecularCol;
        }

        [System.Serializable]
        public class Atmosphere
        {
            public float atmosphereRadius;
            public int numInScatteringPoints;
            public int numOpticalDepthPoints;
            public Vector3 scatteringCoefficients;
            public float scatteringStrength;
            public float densityFalloff;
        }

        [System.Serializable]
        public class Shore : Triplanar
        {
            public Color DryShoreColor = new Color(1f, 1f, 1f, 1f);
            public Color DampShoreColor = new Color(1f, 1f, 1f, 1f);

            [Range(0, 1)]
            public float DryShoreHeightAboveWater = 0.1f;
            [Range(0, 1)]
            public float DampShoreHeightAboveWater = 0.1f;
            [Range(0, 1)]
            public float OceanBlend = 0.5f;
            [Range(0, 1)]
            public float ShoreBlend = 0.5f;
        }

        [System.Serializable]
        public class Biomes : Triplanar
        {
            public Color BiomeALow = new Color(1f, 1f, 1f, 1f);
            public Color BiomeAHigh = new Color(1f, 1f, 1f, 1f);
            /*
            public Color BiomeBLow;
            public Color BiomeBHigh;

            public Color BiomeCLow;
            public Color BiomeCHigh;

            public Color BiomeDLow;
            public Color BiomeDHigh;
            */
            [Range(0, 1)]
            public float BiomeHeightAboveShore;
            [Range(0, 1)]
            public float FlatColBlend;
            /*
            public float FlatColBlendNoise;
            public float MaxFlatHeight;

            public Texture2D NoiseTex;
            public float NoiseScale;
            public float NoiseScale2;
            */
        }

        [System.Serializable]
        public class Mountains : Triplanar
        {
            public Color MountainLow = new Color(1f, 1f, 1f, 1f);
            public Color MountainHigh = new Color(1f, 1f, 1f, 1f);

            public float MaxFlatHeight;
            public float MountainTopBlend;
            public float SteepnessThresholdLow;
            public float SteepnessThresholdHigh;
            public float MountainBlend;
            public float HighUpMountainSteepnessDistribution;
            //public float SteepBands;
            //public float SteepBandsStrength;
            //public float FlatToSteepNoise;
        }
    }

    [System.Serializable]
    public class SecondPlanet
    {
        [HideInInspector]
        public Material celestialBodyMaterial;
        public Ocean ocean;
        public Atmosphere atmosphere;

        [System.Serializable]
        public class Ocean
        {
            public Color OceanDeep = new Color(1f, 1f, 1f, 1f);
            public Color OceanShallow = new Color(1f, 1f, 1f, 1f);

            public float OceanRadius;

            public float DepthMultiplier;
            public float AlphaMultiplier;

            [Range(0, 1)]
            public float Smoothness;

            public bool SelfGlow;

            public float WaveSpeed;
            public Texture2D WaveNormalA;
            public Texture2D WaveNormalB;
            public float WaveNormalScale;
            [Range(0, 1)]
            public float WaveStrength;
            public Color SpecularCol;
        }

        [System.Serializable]
        public class Atmosphere
        {
            public float atmosphereRadius;
            public int numInScatteringPoints;
            public int numOpticalDepthPoints;
            public Vector3 scatteringCoefficients;
            public float scatteringStrength;
            public float densityFalloff;
        }
    }

    [System.Serializable]
    public class Moon
    {
        [HideInInspector]
        public Material celestialBodyMaterial;
        public NormalMap normalMap;

        [System.Serializable]
        public class NormalMap
        {
            public Texture2D NormalMapTexture;

            //public float ScaleTexture;
            public float BlendSharpness;
            public float ScaleNormalMap;
        }
    }

    [System.Serializable]
    public class Sun
    {
        [HideInInspector]
        public Material celestialBodyMaterial;

        public Color _SunColorUndertone = new Color(1f, 1f, 1f, 1f);
        public Color _SunColorMidtone = new Color(1f, 1f, 1f, 1f);
        public Color _SunColorOvertone = new Color(1f, 1f, 1f, 1f);
    }
}
