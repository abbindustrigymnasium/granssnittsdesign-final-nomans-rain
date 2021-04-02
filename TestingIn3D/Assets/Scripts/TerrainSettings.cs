using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainSettings
{
    public enum FilterType { Custom, Shape, Detail, Ridge, Cellular, Creater };
    public FilterType filterType;

    public CustomNoiseSettings customNoiseSettings;
    public ShapeNoiseSettings shapeNoiseSettings;
    public DetailedNoiseSettings detailedNoiseSettings;
    public RidgeNoiseSettings ridgeNoiseSettings;
    public MountainNoiseSettings mountainNoiseSettings;
    public CellularNoiseSettings cellularNoiseSettings;
    public CreaterNoiseSettings createrNoiseSettings;
    //    public FastNoiseLite fastNoiseLite;

    public class SharedSettings
    {

    }

    [System.Serializable]
    public class CreaterNoiseSettings
    {
        public int seed = 1337;

        public int numCreaters = 1;
        public float rimWidth = 1;
        public float rimSteepness = 1;

        [Range(-1, 1)]
        public float distributionRad = 0.0f;
        public Vector2 radMinMax = new Vector2(0.05f, 0.2f);

        [Range(-1, 1)]
        public float distributionFloor = 0.0f;
        public Vector2 floorMinMax = new Vector2(0.05f, 0.2f);

        [Range(-1, 1)]
        public float distributionSmoothness = 0.0f;
        public Vector2 smoothnessMinMax = new Vector2(0.05f, 0.2f);
    }

    [System.Serializable]
    public class CustomNoiseSettings : FastNoiseLite
    {
        public General general;
        public GeneralNoise generalNoise;
        public Fractal fractal;
        public Cellular cellular;
        public DomainWarping domainWarp;
        public DomainWarpingFractal domainWarpFractal;

        [System.Serializable]
        public class General
        {
            public float amplitude = 0.1f;
            public Vector3 offsett;

            [Range(0, 1)]
            public float minValue;
            [Range(0, 1)]
            public float smoothingMin;
            [Range(0, 1)]
            public float maxValue;
            [Range(0, 1)]
            public float smoothingMax;

            public bool invert = false;
            public int seed = 1337;
        }

        [System.Serializable]
        public class GeneralNoise
        {
//            public enum NoiseType { OpenSimplex2, OpenSimplex2S, Cellular, Perlin, ValueCubic, Value }
            public NoiseType noiseType = NoiseType.OpenSimplex2;

//            public enum RotationType3D { None, ImproveXYPlanes, ImproveXZPlanes }
            public RotationType3D rotationType3D = RotationType3D.None;

            public float frequency = 0.02f;
        }

        [System.Serializable]
        public class Fractal
        {
//            public enum Type { None, FBm, Ridged, PingPong }
            public FractalType fractalType = FractalType.None;

            public int octaves = 1;
            public float lacunarity = 2.0f;
            public float gain = 0.5f;
            public float weightedStrength = 0.0f;
            public float pingPongStrength = 2.0f;
        }

        [System.Serializable]
        public class Cellular
        {
//            public enum DistanceFunction { Euclidean, EuclideanSq, Manhattan, Hybrid }
            public CellularDistanceFunction cellularDistanceFunction = CellularDistanceFunction.Euclidean;

//            public enum ReturnType { CellValue, Distance, Distance2, Distance2Add, Distance2Sub, Distance2Mul, Distance2Div }
            public CellularReturnType cellularReturnType = CellularReturnType.CellValue;

            public float jitter = 1.0f;
        }

        [System.Serializable]
        public class DomainWarping
        {
//            public enum Type { None, OpenSimplex2, OpenSimplex2Reduced, BasicGrid }
            public DomainWarpType domainWarpType = DomainWarpType.OpenSimplex2;

//            public enum RotationType3D { None, ImproveXYPlanes, ImproveXZPlanes }
            // AAAAAAAAAAAH WRONG WRONG WRONG WRONG AAAAAAAAAH
            public TransformType3D rotationType3D = TransformType3D.None; // 123 krävs ej
            // AAAAAAAAAAAH WRONG WRONG WRONG WRONG AAAAAAAAAH

            public float amplitude = 30.0f;
            public float frequency = 0.005f;
        }

        [System.Serializable]
        public class DomainWarpingFractal
        {
//            public enum FractalType { None, DomainWarpProgressive, DomainWarpIndependent }
            // AAAAAAAAAAAH WRONG WRONG WRONG WRONG AAAAAAAAAH
            public FractalType fractalType = FractalType.None;
            // AAAAAAAAAAAH WRONG WRONG WRONG WRONG AAAAAAAAAH

            public int octaves = 5;
            public float lacunarity = 2.0f;
            public float gain = 0.5f;
        }
    }

    [System.Serializable]
    public class ShapeNoiseSettings : SharedSettings
    {
        public int seed = 1337;
        [Range(1, 10)]
        public int octaves = 1;
        public float period = 1.0f; // små nummer ger större noise bild
        [Range(0,1)]
        public float persistence = 1.0f; // hur mycker varje ny lager contriburerar, 1 = lika mycket contribution, 0 = de andra lagrerna inte contriburerar
        public float lacunarity = 1.0f; // hur mycket mindre nästa noise value blir
        public float scale = 1.0f;
        public float elevation = 1.0f;
        public float verticalShift = 1.0f;
        public Vector3 Offsett;
    }

    [System.Serializable]
    public class DetailedNoiseSettings : SharedSettings
    {
        public int seed = 1337;
    }

    [System.Serializable]
    public class RidgeNoiseSettings : SharedSettings
    {
        public int seed = 1337;
    }

    [System.Serializable]
    public class MountainNoiseSettings : SharedSettings
    {
        public int seed = 1337;
        public float weightMultiplier = 0.8f;
        public float baseRoughness = 1.0f;    
        [Range(1, 10)]
        public int octaves = 1;
        public Vector3 Offsett;
        public float roughness = 1.0f;
        public float persistence = 1.0f;
        public float minValue = 1.0f;
        public float strength = 1.0f;
    }

    [System.Serializable]
    public class CellularNoiseSettings : SharedSettings
    {
        public int seed = 1337;
    }
}
