using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainProjection
{
    /*
    private ShapeSettings shapeSettings;
    private ITerrainFilter[] noiseFilters;
    public static float planetRad;

    public Texture2D noiseSampleTexture2D;

    public TerrainProjection(ShapeSettings shapeSettings, SphereSettings sphereSettings)
    {
        this.shapeSettings = shapeSettings;
        noiseFilters = new ITerrainFilter[shapeSettings.noiseLayers.Length];
        planetRad = sphereSettings.planetRadius;

        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = TerrainFilterFactory.CreateNoiseFilter(shapeSettings.noiseLayers[i].terrainSettings);
        }

        for (int q = 0; q < 20; q++)
        {
            for (int i = 0; i < CelestialBodyManager.vectors[q].Length; i++)
            {
                CelestialBodyManager.vectors[q][i] = GetNoiseOfPoint(CelestialBodyManager.vectors[q][i]);
            }
        }
        noiseSampleTexture2D = noiseFilters[0].Text();
    }

    public interface ITerrainFilter
    {
        Texture2D Text();
        float Evaluate(Vector3 point);
    }

    public static class TerrainFilterFactory
    {
        public static ITerrainFilter CreateNoiseFilter(TerrainSettings terrainSettings)
        {
            switch (terrainSettings.filterType)
            {
                case TerrainSettings.FilterType.Custom:
                    return new CustomNoise(terrainSettings.customNoiseSettings);
                case TerrainSettings.FilterType.Shape:
                    return new ShapeNoise(terrainSettings.shapeNoiseSettings);
                case TerrainSettings.FilterType.Detail:
                    return new MountainNoiseFilter(terrainSettings.mountainNoiseSettings);
                case TerrainSettings.FilterType.Ridge:
                    return new RidgeNoiseFilter(terrainSettings.ridgeNoiseSettings);
                case TerrainSettings.FilterType.Cellular:
                    return new CellularNoiseFilter(terrainSettings.cellularNoiseSettings);
                case TerrainSettings.FilterType.Creater:
                    return new CreaterCreator(terrainSettings.createrNoiseSettings);
            }
            return null;
        }
    }

    private Vector3 GetNoiseOfPoint(Vector3 point)
    {
        float firstLayerValue = 0;
        float elevation = 0.0f;

        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(point);
            if (shapeSettings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }

        for (int i = 1; i < noiseFilters.Length; i++)
        {
            if (shapeSettings.noiseLayers[i].enabled)
            {
                float mask = (shapeSettings.noiseLayers[i].useFirstLayerAsMask) ?  firstLayerValue : 1;
                elevation += noiseFilters[i].Evaluate(point) * mask;
            }
        }

        return point * planetRad * (1 + elevation);
    }

    public class CustomNoise : ITerrainFilter
    {
        TerrainSettings.CustomNoiseSettings customNoiseSettings;
        FastNoiseLite noise;

        public Texture2D text;

        public CustomNoise (TerrainSettings.CustomNoiseSettings customNoiseSettings)
        {
            this.customNoiseSettings = customNoiseSettings;
            noise = new FastNoiseLite(customNoiseSettings.general.seed);

            noise.SetFrequency(customNoiseSettings.generalNoise.frequency);
            noise.SetNoiseType(customNoiseSettings.generalNoise.noiseType);
            noise.SetRotationType3D(customNoiseSettings.generalNoise.rotationType3D);

            noise.SetFractalType(customNoiseSettings.fractal.fractalType);
            noise.SetFractalOctaves(customNoiseSettings.fractal.octaves);
            noise.SetFractalLacunarity(customNoiseSettings.fractal.lacunarity);
            noise.SetFractalGain(customNoiseSettings.fractal.gain);
            noise.SetFractalWeightedStrength(customNoiseSettings.fractal.weightedStrength);
            noise.SetFractalPingPongStrength(customNoiseSettings.fractal.pingPongStrength);

            noise.SetCellularDistanceFunction(customNoiseSettings.cellular.cellularDistanceFunction);
            noise.SetCellularReturnType(customNoiseSettings.cellular.cellularReturnType);
            noise.SetCellularJitter(customNoiseSettings.cellular.jitter);

            noise.SetDomainWarpType(customNoiseSettings.domainWarp.domainWarpType);
            noise.SetDomainWarpAmp(customNoiseSettings.domainWarp.amplitude);
            noise.UpdateWarpTransformType3D();

            text = new Texture2D(128, 128);
            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    float newX = x / 64f;
                    float newY = y / 64f;

                    noise.DomainWarp(ref newX, ref newY);

                    float noiseValue = (noise.GetNoise(newX, newY) + 1.0f) * 0.5f;
                    //                    int colorInt = (int) (noiseValue * 255f);
                    Color color = new Color(noiseValue, noiseValue, noiseValue);
                    text.SetPixel(x, y, color);
                }
            }
            text.Apply();
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0.0f;
            float pointXRef = point.x;
            float pointYRef = point.y;
            float pointZRef = point.z;
            noise.DomainWarp(ref pointXRef, ref pointYRef, ref pointZRef);

            noiseValue = (noise.GetNoise(pointXRef + customNoiseSettings.general.offsett.x, pointYRef + customNoiseSettings.general.offsett.y, pointZRef + customNoiseSettings.general.offsett.z) + 1.0f) * 0.5f;
            noiseValue = Mathf.Max(0, noiseValue - customNoiseSettings.general.minValue);
            noiseValue = Mathf.Min(0, noiseValue - customNoiseSettings.general.maxValue);

            return noiseValue * customNoiseSettings.general.amplitude;
        }

        public Texture2D Text()
        {
            return text;
        }
    }

    public class ShapeNoise : ITerrainFilter // numbLayers, lacunarity, persistence, scale (låg scale är rundare), elevation, verticalShift, ofsett
    {
        TerrainSettings.ShapeNoiseSettings shapeNoiseSettings;
        OpenSimplexNoise noise;

        public ShapeNoise(TerrainSettings.ShapeNoiseSettings shapeNoiseSettings)
        {
            this.shapeNoiseSettings = shapeNoiseSettings;
            noise = new OpenSimplexNoise(shapeNoiseSettings.seed);
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0;
            float frequency = shapeNoiseSettings.persistence;
            float amplitude = 1;
            
            for (int i = 0; i < shapeNoiseSettings.octaves; i++)
            {
                float v = noise.Evaluate(point * frequency + shapeNoiseSettings.Offsett);
                noiseValue += (v + 1) * 0.5f * amplitude;
                frequency *= shapeNoiseSettings.scale;
                amplitude *= shapeNoiseSettings.persistence;
            }

            noiseValue = Mathf.Max(0, noiseValue);
            return noiseValue * shapeNoiseSettings.elevation;
        }

        public Texture2D Text()
        {
            return new Texture2D(128, 128);
        }
    }

    public class MountainNoiseFilter : ITerrainFilter
    {
        TerrainSettings.MountainNoiseSettings mountainNoiseSettings;
        OpenSimplexNoise noise;

        public MountainNoiseFilter(TerrainSettings.MountainNoiseSettings mountainNoiseSettings)
        {
            this.mountainNoiseSettings = mountainNoiseSettings;
            noise = new OpenSimplexNoise(mountainNoiseSettings.seed);
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0;
            float frequency = mountainNoiseSettings.baseRoughness;
            float amplitude = 1;
            float weight = 1;

            for (int i = 0; i < mountainNoiseSettings.octaves; i++)
            {
                float v = 1-Mathf.Abs(noise.Evaluate(point * frequency + mountainNoiseSettings.Offsett));
                v *= v;
                v *= weight;
                weight = Mathf.Clamp01(v * mountainNoiseSettings.weightMultiplier);

                noiseValue += v * amplitude;
                frequency *= mountainNoiseSettings.roughness;
                amplitude *= mountainNoiseSettings.persistence;
            }

            noiseValue = Mathf.Max(0, noiseValue - mountainNoiseSettings.minValue);
            return noiseValue * mountainNoiseSettings.strength;
        }

        public Texture2D Text()
        {
            return new Texture2D(128, 128);
        }
    }

    public class RidgeNoiseFilter : ITerrainFilter
    {
        TerrainSettings.RidgeNoiseSettings ridgeNoiseSettings;
        OpenSimplexNoise noise;

        public RidgeNoiseFilter(TerrainSettings.RidgeNoiseSettings ridgeNoiseSettings)
        {
            this.ridgeNoiseSettings = ridgeNoiseSettings;
            noise = new OpenSimplexNoise(ridgeNoiseSettings.seed);
        }

        public float Evaluate(Vector3 point)
        {
            return 0.0f;
        }

        public Texture2D Text()
        {
            return new Texture2D(128, 128);
        }
    }

    public class CellularNoiseFilter : ITerrainFilter
    {
        TerrainSettings.CellularNoiseSettings cellularNoiseSettings;
        OpenSimplexNoise noise;

        public CellularNoiseFilter(TerrainSettings.CellularNoiseSettings cellularNoiseSettings)
        {
            this.cellularNoiseSettings = cellularNoiseSettings;
            noise = new OpenSimplexNoise(cellularNoiseSettings.seed);
        }

        public float Evaluate(Vector3 point)
        {
            return 0.0f;
        }

        public Texture2D Text()
        {
            return new Texture2D(128, 128);
        }
    }

    public class CreaterCreator : ITerrainFilter // allt måste utgå från planet radius
    {
        TerrainSettings.CreaterNoiseSettings createrSettings;
        Creater[] creaters;

        public struct Creater
        {
            public Vector3 point;
            public float createrRad;
        }

        public CreaterCreator(TerrainSettings.CreaterNoiseSettings createrSettings)
        {
            float BiasFunction(float t, float bias)
            {
                float k = Mathf.Pow(1.0f - bias, 3);
                return (t * k) / (t * k - t + 1);
            }

            this.createrSettings = createrSettings;
            Random.InitState(createrSettings.seed);

            creaters = new Creater[createrSettings.numCreaters];
            for (int i = 0; i < createrSettings.numCreaters; i++)
            {
                float rand1to0 = Random.Range(0.0f, 1.0f);
                //                float t = BiasFunction(rand1to0, createrSettings.distribution);
                float t = 0.5f;

                float rad = Mathf.Lerp(createrSettings.radMinMax.x, createrSettings.radMinMax.y, t);
                creaters[i] = new Creater () { point = Random.onUnitSphere, createrRad = rad };
                //Debug.Log(JsonUtility.ToJson(creaters[i], true));
            }
        }

        public float Evaluate(Vector3 point)
        {
            float SmoothMin(float a, float b, float k)
            {
                float h = Mathf.Clamp01((b - a + k) / (2 * k));
                return a * h + b * (1 - h) - k * h * (1 - h);
            }

            float SmoothMax(float a, float b, float k)
            {
                k = -k;
                float h = Mathf.Clamp01((b - a + k) / (2 * k));
                return a * h + b * (1 - h) - k * h * (1 - h);
            }

            float createrHeight = 0;

            for (int i = 0; i < createrSettings.numCreaters; i++)
            {
                float x = Mathf.Acos((point.x * creaters[i].point.x + point.y * creaters[i].point.y + point.z * creaters[i].point.z))/creaters[i].createrRad;

                float cavity = x * x - 1;
                float rimX = Mathf.Min(x - 1 - createrSettings.rimWidth, 0);
                float rim = createrSettings.rimSteepness * rimX * rimX;

//                float createrShape = SmoothMax(cavity, createrSettings.floorHeight, createrSettings.smoothness);
//                createrShape = SmoothMin(createrShape, rim, createrSettings.smoothness);
//                createrHeight += createrShape * creaters[i].createrRad;

            }

            return createrHeight;
            */
    /*
                float createrValue = 0.0f;
                float distanceBetweenPoints = 0.0f;

                float CreaterShape(float x, float rad)
                {
                    if (x > rad)
                    {
                        return 0.0f;
                    }

                    float CavityShape(float dist)
                    {
                        return createrSettings.cavitySteepness * dist * dist + createrSettings.floorHeight*2.0f; // hur långt ner ska creater gå?
                    }

                    float RimShape(float dist)
                    {
                        dist = Mathf.Abs(x) - rad;
                        return createrSettings.rimSteepness * dist * dist;
                    }

                    float FloorShape(float dist)
                    {
                        return createrSettings.floorHeight;
                    }

                    float SmoothMin(float a, float b, float k)
                    {
                        float h = Mathf.Clamp01((b - a + k) / (2 * k));
                        return a * h + b * (1 - h) - k * h * (1 - h);
                    }

                    float SmoothMax(float a, float b, float k)
                    {
                        k = -k;
                        float h = Mathf.Clamp01((b - a + k) / (2 * k));
                        return a * h + b * (1 - h) - k * h * (1 - h);
                    }

                    float createrShape = SmoothMax(CavityShape(x), FloorShape(x), createrSettings.smoothness);
                    createrShape = SmoothMin(createrShape, RimShape(x), createrSettings.smoothness);
                    return createrShape;
                }

                for (int i = 0; i < createrSettings.numCreaters; i++)
                {
                    distanceBetweenPoints = Mathf.Acos((point.x * creaters[i].point.x + point.y * creaters[i].point.y + point.z * creaters[i].point.z));

                    createrValue += CreaterShape(distanceBetweenPoints, creaters[i].createrRad);
                }

                return createrValue;
    */
    /*
            }

            public Texture2D Text()
            {
                return new Texture2D(128, 128);
            }
        }
    */
    public Texture2D noiseSampleTexture2D;
}
