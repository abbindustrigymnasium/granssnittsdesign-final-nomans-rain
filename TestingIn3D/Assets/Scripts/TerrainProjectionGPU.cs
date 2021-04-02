using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainProjectionGPU
{
    private ShapeSettings shapeSettings;
    private ITerrainFilter[] noiseFilters;
    public static float planetRad;

    public Texture2D[] noiseSampleTexture2D;

    public TerrainProjectionGPU(ShapeSettings shapeSettings, SphereSettings sphereSettings)
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
            CelestialBodyManager.vectors[q] = GetNoiseOfSide(CelestialBodyManager.vectors[q]);
/*
            for (int i = 0; i < CelestialBodyManager.vectors[q].Length; i++)
            {
                CelestialBodyManager.vectors[q][i] = GetNoiseOfPoint(CelestialBodyManager.vectors[q][i]);
            }
*/
        }

        noiseSampleTexture2D = new Texture2D[noiseFilters.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseSampleTexture2D[i] = noiseFilters[i].Text();
        }
    }

    public interface ITerrainFilter
    {
        ComputeShader Compute();
        Texture2D Text();
        //        float[] Evaluate(Vector3[] points, ComputeShader computeShader);
        float[] EvaluateSide(float[] heights, Vector3[] points, float[] heightsFirstValue, bool useFirstLayerAsMask);
    }

    public static class TerrainFilterFactory
    {
        public static ITerrainFilter CreateNoiseFilter(TerrainSettings terrainSettings)
        {
            switch (terrainSettings.filterType)
            {
                case TerrainSettings.FilterType.Custom:
                    return new CustomNoise(terrainSettings.customNoiseSettings);
                case TerrainSettings.FilterType.Creater:
                    return new Creators(terrainSettings.createrNoiseSettings);
            }
            return null;
        }
    }

    private Vector3[] GetNoiseOfSide(Vector3[] points)
    {
        float[] heights = new float[points.Length];
        float[] heightsFirstValue = new float[points.Length];

        if (noiseFilters.Length > 0)
        {
            heightsFirstValue = noiseFilters[0].EvaluateSide(heights, points, heightsFirstValue, false);

            if (shapeSettings.noiseLayers[0].enabled)
            {
                heights = (float[])heightsFirstValue.Clone();
            }
/*
            for (int i = 0; i < heightsFirstValue.Length; i++)
            {
                heightsFirstValue[i] -= 0.6f;
            }
*/
        }

        for (int i = 1; i < noiseFilters.Length; i++)
        {
            if (shapeSettings.noiseLayers[i].enabled)
            {
                heights = noiseFilters[i].EvaluateSide(heights, points, heightsFirstValue, shapeSettings.noiseLayers[i].useFirstLayerAsMask);
            }
        }

        for (int i = 0; i < points.Length; i++)
        {
            points[i] *= heights[i] * planetRad;
        }

        return points;
    }

/*
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
                float mask = (shapeSettings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                elevation += noiseFilters[i].Evaluate(point) * mask;
            }
        }

        return point * planetRad * (1 + elevation);
    }
*/

    public class Creators : ITerrainFilter
    {
        TerrainSettings.CreaterNoiseSettings createrSettings;
        ComputeShader compute;
        Creater[] creaters;

        public struct Creater
        {
            public Vector3 centre;
            public float radius;
            public float floor;
            public float smoothness;
        }

        public Creators(TerrainSettings.CreaterNoiseSettings createrSettings)
        {
            this.createrSettings = createrSettings;
            Random.InitState(createrSettings.seed);

            float BiasFunction(float t, float bias)
            {
                float k = Mathf.Pow(1.0f - bias, 3);
                return (t * k) / (t * k - t + 1);
            }

            creaters = new Creater[createrSettings.numCreaters];
            for (int i = 0; i < createrSettings.numCreaters; i++)
            {
                float rand1to0 = Random.Range(0.0f, 1.0f);
                float t = BiasFunction(rand1to0, createrSettings.distributionRad);
                float rad = Mathf.Lerp(createrSettings.radMinMax.x, createrSettings.radMinMax.y, t);

                rand1to0 = Random.Range(0.0f, 1.0f);
                t = BiasFunction(rand1to0, createrSettings.distributionFloor);
                float floor = Mathf.Lerp(createrSettings.floorMinMax.x, createrSettings.floorMinMax.y, t);

                rand1to0 = Random.Range(0.0f, 1.0f);
                t = BiasFunction(rand1to0, createrSettings.distributionSmoothness);
                float smoothness = Mathf.Lerp(createrSettings.smoothnessMinMax.x, createrSettings.smoothnessMinMax.y, t);

                creaters[i] = new Creater() { centre = Random.onUnitSphere, radius = rad, floor = floor, smoothness = smoothness };
            }
        }

        public float[] EvaluateSide(float[] heights, Vector3[] points, float[] heightsFirstValue, bool useFirstLayerAsMask)
        {
            compute = (ComputeShader)Resources.Load("OtherComputeShaders/CreaterComputeShader");

            compute.SetInt("numCreaters", creaters.Length);
            compute.SetInt("verticesAmount", points.Length);
            compute.SetFloat("rimSteepness", createrSettings.rimSteepness);
            compute.SetFloat("rimWidth", createrSettings.rimWidth);

            ComputeBuffer buffer = new ComputeBuffer(points.Length, 3 * sizeof(float));
            buffer.SetData(points);
            compute.SetBuffer(0, "vectors", buffer);

            ComputeBuffer heightBuffer = new ComputeBuffer(points.Length, sizeof(float));
            heightBuffer.SetData(heights);
            compute.SetBuffer(0, "heights", heightBuffer);

            ComputeBuffer createrBuffer = new ComputeBuffer(creaters.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Creater)));
            createrBuffer.SetData(creaters);
            compute.SetBuffer(0, "creaters", createrBuffer);

            compute.Dispatch(0, 512, 1, 1);

            heightBuffer.GetData(heights);

            buffer.Dispose();
            heightBuffer.Dispose();
            createrBuffer.Dispose();

            return heights;
        }

        public Texture2D Text()
        {
            return new Texture2D(128, 128);
        }

        public ComputeShader Compute()
        {
            return compute;
        }
    }

    public class CustomNoise : ITerrainFilter
    {
        TerrainSettings.CustomNoiseSettings customNoiseSettings;
        FastNoiseLite noise;

        public Texture2D text;

        ComputeShader compute;

        public CustomNoise(TerrainSettings.CustomNoiseSettings customNoiseSettings)
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
                    Color color = new Color(noiseValue, noiseValue, noiseValue);
                    text.SetPixel(x, y, color);
                }
            }
            text.Apply();
        }

        public float[] EvaluateSide(float[] heights, Vector3[] points, float[] heightsFirstValue, bool useFirstLayerAsMask)
        {
            switch (customNoiseSettings.generalNoise.noiseType)
            {
                case FastNoiseLite.NoiseType.OpenSimplex2:
                    compute = (ComputeShader)Resources.Load("OtherNoiseFilterComputeShaders/OpenSimplex2");
                    break;
                case FastNoiseLite.NoiseType.OpenSimplex2S:
                    compute = (ComputeShader)Resources.Load("OtherNoiseFilterComputeShaders/OpenSimplex2S");
                    break;
                case FastNoiseLite.NoiseType.Perlin:
                    compute = (ComputeShader)Resources.Load("OtherNoiseFilterComputeShaders/Perlin");
                    break;
                case FastNoiseLite.NoiseType.ValueCubic:
                    compute = (ComputeShader)Resources.Load("OtherNoiseFilterComputeShaders/ValueCubic");
                    break;
                case FastNoiseLite.NoiseType.Value:
                    compute = (ComputeShader)Resources.Load("OtherNoiseFilterComputeShaders/Value");
                    break;

                case FastNoiseLite.NoiseType.Cellular:

                    switch (customNoiseSettings.cellular.cellularDistanceFunction)
                    {
                        case FastNoiseLite.CellularDistanceFunction.Euclidean:
                            compute = (ComputeShader)Resources.Load("CellularComputeShaders/CellularDistanceEuclidean");
                            break;
                        case FastNoiseLite.CellularDistanceFunction.EuclideanSq:
                            compute = (ComputeShader)Resources.Load("CellularComputeShaders/CellularDistanceEuclideanSQ");
                            break;
                        case FastNoiseLite.CellularDistanceFunction.Manhattan:
                            compute = (ComputeShader)Resources.Load("CellularComputeShaders/CellularDistanceManhattan");
                            break;
                        case FastNoiseLite.CellularDistanceFunction.Hybrid:
                            compute = (ComputeShader)Resources.Load("CellularComputeShaders/CellularDistanceHybrid");
                            break;
                    }

                    compute.SetInt("_cellularDistanceFunction", (int)customNoiseSettings.cellular.cellularDistanceFunction);
                    compute.SetInt("_cellularReturnType", (int)customNoiseSettings.cellular.cellularReturnType);
                    compute.SetFloat("_jitter", customNoiseSettings.cellular.jitter);
                    break;
            }
            compute.SetBool("_useFirstLayerAsMask", useFirstLayerAsMask);

            compute.SetBool("_invert", customNoiseSettings.general.invert);

            compute.SetInt("_seed", customNoiseSettings.general.seed);
            compute.SetFloat("_frequency", customNoiseSettings.generalNoise.frequency);
            compute.SetInt("_rotationType3D", (int)customNoiseSettings.generalNoise.rotationType3D);

            compute.SetInt("_fractalType", (int)customNoiseSettings.fractal.fractalType);
            compute.SetInt("_octaves", customNoiseSettings.fractal.octaves);
            compute.SetFloat("_lacunarity", customNoiseSettings.fractal.lacunarity);
            compute.SetFloat("_gain", customNoiseSettings.fractal.gain);
            compute.SetFloat("_weightedStrength", customNoiseSettings.fractal.weightedStrength);
            compute.SetFloat("_pingPongStrength", customNoiseSettings.fractal.pingPongStrength);

            compute.SetInt("_domainWarpType", (int)customNoiseSettings.domainWarp.domainWarpType);
            compute.SetFloat("_amplitude_warp", customNoiseSettings.domainWarp.amplitude);

            compute.SetFloats("_offsett", new float[3] { customNoiseSettings.general.offsett.x, customNoiseSettings.general.offsett.y, customNoiseSettings.general.offsett.z });
            compute.SetFloat("_minValue", customNoiseSettings.general.minValue);
            compute.SetFloat("_maxValue", customNoiseSettings.general.maxValue);
            compute.SetFloat("_planetRad", planetRad);
            compute.SetFloat("_smoothingMin", customNoiseSettings.general.smoothingMin);
            compute.SetFloat("_smoothingMax", customNoiseSettings.general.smoothingMax);
            compute.SetFloat("_amplitude", customNoiseSettings.general.amplitude);

            compute.SetInt("verticesAmount", points.Length);
            ComputeBuffer buffer = new ComputeBuffer(points.Length, 3 * sizeof(float));
            buffer.SetData(points);
            compute.SetBuffer(0, "vectors", buffer);

            ComputeBuffer heightBuffer = new ComputeBuffer(points.Length, sizeof(float));
            heightBuffer.SetData(heights);
            compute.SetBuffer(0, "heights", heightBuffer);

            ComputeBuffer heightBufferFirstValue = new ComputeBuffer(points.Length, sizeof(float));
            heightBufferFirstValue.SetData(heightsFirstValue);
            compute.SetBuffer(0, "heightsFirstValue", heightBufferFirstValue);

            compute.Dispatch(0, 512, 1, 1);

            heightBuffer.GetData(heights);

            buffer.Dispose();
            heightBuffer.Dispose();
            heightBufferFirstValue.Dispose();

            return heights;
        }

        public Texture2D Text()
        {
            return text;
        }

        public ComputeShader Compute()
        {
            return compute;
        }
    }
}