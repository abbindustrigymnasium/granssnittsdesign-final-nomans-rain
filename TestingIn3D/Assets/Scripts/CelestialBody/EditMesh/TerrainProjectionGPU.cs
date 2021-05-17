using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainProjectionGPU
{
    private ShapeSettings shapeSettings;
    private ITerrainFilter[] noiseFilters;
    public static float planetRad;

    //public Texture2D[] noiseSampleTexture2D;

    public TerrainProjectionGPU(ShapeSettings shapeSettings, SphereSettings sphereSettings, ref Texture2D[] noiseTextures, int time)
    {
        this.shapeSettings = shapeSettings;
        noiseFilters = new ITerrainFilter[shapeSettings.noiseLayers.Length];
        planetRad = sphereSettings.planetRadius;

        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = TerrainFilterFactory.CreateNoiseFilter(shapeSettings.noiseLayers[i].terrainSettings, time);
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


        noiseTextures = new Texture2D[noiseFilters.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseTextures[i] = noiseFilters[i].Text();
        }
    }

    public interface ITerrainFilter
    {
        Texture2D Text();
        //        float[] Evaluate(Vector3[] points, ComputeShader computeShader);
        float[] EvaluateSide(float[] heights, float[] maskHeights, Vector3[] points);
    }

    public static class TerrainFilterFactory
    {
        public static ITerrainFilter CreateNoiseFilter(TerrainSettings terrainSettings, int time)
        {
            switch (terrainSettings.filterType)
            {
                case TerrainSettings.FilterType.Custom:
                    return new CustomNoise(terrainSettings.customNoiseSettings, time);
                case TerrainSettings.FilterType.Creater:
                    return new Creators(terrainSettings.createrNoiseSettings, time);
            }
            return null;
        }
    }

    private Vector3[] GetNoiseOfSide(Vector3[] points)
    {
        int layersEnabled = 0;

        float[] heights = new float[points.Length];
        float[] newHeights = new float[points.Length];
        float[] maskHeights = new float[points.Length];
        for (int i = 0; i < maskHeights.Length; i++)
        {
            maskHeights[i] = 1f;
        }
        /*float[] heightsFirstValue = new float[points.Length];

        if (noiseFilters.Length > 0)
        {
            heightsFirstValue = noiseFilters[0].EvaluateSide(heights, maskHeights, points);

            if (shapeSettings.noiseLayers[0].enabled)
            {
                heights = (float[])heightsFirstValue.Clone();
            }
        }*/

        for (int i = 0; i < noiseFilters.Length; i++)
        {
            if (shapeSettings.noiseLayers[i].enabled)
            {
                layersEnabled++;
                newHeights = noiseFilters[i].EvaluateSide(heights, maskHeights, points);

                for (int q = 0; q < points.Length; q++)
                {
                    heights[q] += newHeights[q];
                }
            }
        }

        if (layersEnabled != 0)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] *= heights[i] * planetRad;
            }
        }
        else
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] *= planetRad;
            }
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

        public Creators(TerrainSettings.CreaterNoiseSettings createrSettings, int time)
        {
            this.createrSettings = createrSettings;

            Random.InitState(createrSettings.randomizeOnStart ? time : createrSettings.seed);

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
                float floor = -Mathf.Lerp(createrSettings.floorMinMax.x, createrSettings.floorMinMax.y, t);

                rand1to0 = Random.Range(0.0f, 1.0f);
                t = BiasFunction(rand1to0, createrSettings.distributionSmoothness);
                float smoothness = Mathf.Lerp(createrSettings.smoothnessMinMax.x, createrSettings.smoothnessMinMax.y, t);

                creaters[i] = new Creater() { centre = Random.onUnitSphere, radius = rad, floor = floor, smoothness = smoothness };
            }
        }

        public float[] EvaluateSide(float[] heights, float[] maskHeights, Vector3[] points)
        {
            compute = (ComputeShader)Resources.Load("OtherComputeShaders/CreaterComputeShader");

            float[] newHeights = new float[heights.Length];

            compute.SetInt("numCreaters", creaters.Length);
            compute.SetInt("verticesAmount", points.Length);
            compute.SetFloat("rimSteepness", createrSettings.rimSteepness);
            compute.SetFloat("rimWidth", createrSettings.rimWidth);

            ComputeBuffer buffer = new ComputeBuffer(points.Length, 3 * sizeof(float));
            buffer.SetData(points);
            compute.SetBuffer(0, "vectors", buffer);

            ComputeBuffer heightBuffer = new ComputeBuffer(points.Length, sizeof(float));
            heightBuffer.SetData(newHeights);
            compute.SetBuffer(0, "heights", heightBuffer);

            ComputeBuffer createrBuffer = new ComputeBuffer(creaters.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Creater)));
            createrBuffer.SetData(creaters);
            compute.SetBuffer(0, "creaters", createrBuffer);

            compute.Dispatch(0, 512, 1, 1);

            heightBuffer.GetData(newHeights);

            buffer.Dispose();
            heightBuffer.Dispose();
            createrBuffer.Dispose();

            return newHeights;
        }

        public Texture2D Text()
        {
            return null;
        }
    }

    public class CustomNoise : ITerrainFilter
    {
        TerrainSettings.CustomNoiseSettings customNoiseSettings;
        FastNoiseLite noise;
        ComputeShader compute;
        int dateTimeSeed = 0;

        public CustomNoise(TerrainSettings.CustomNoiseSettings customNoiseSettings, int time)
        {
            this.customNoiseSettings = customNoiseSettings;
            dateTimeSeed = time;
            noise = new FastNoiseLite(customNoiseSettings.noise.general.randomizeOnStart ? dateTimeSeed : customNoiseSettings.noise.general.seed);

            noise.SetFrequency(customNoiseSettings.noise.generalNoise.frequency);
            noise.SetNoiseType(customNoiseSettings.noise.generalNoise.noiseType);
            noise.SetRotationType3D(customNoiseSettings.noise.generalNoise.rotationType3D);

            noise.SetFractalType(customNoiseSettings.noise.fractal.fractalType);
            noise.SetFractalOctaves(customNoiseSettings.noise.fractal.octaves);
            noise.SetFractalLacunarity(customNoiseSettings.noise.fractal.lacunarity);
            noise.SetFractalGain(customNoiseSettings.noise.fractal.gain);
            noise.SetFractalWeightedStrength(customNoiseSettings.noise.fractal.weightedStrength);
            noise.SetFractalPingPongStrength(customNoiseSettings.noise.fractal.pingPongStrength);

            noise.SetCellularDistanceFunction(customNoiseSettings.noise.cellular.cellularDistanceFunction);
            noise.SetCellularReturnType(customNoiseSettings.noise.cellular.cellularReturnType);
            noise.SetCellularJitter(customNoiseSettings.noise.cellular.jitter);

            noise.SetDomainWarpType(customNoiseSettings.noise.domainWarp.domainWarpType);
            noise.SetDomainWarpAmp(customNoiseSettings.noise.domainWarp.amplitude);
            noise.UpdateWarpTransformType3D();

            customNoiseSettings.noiseTextureSample = new Texture2D(128, 128);
            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    float newX = x / 64f;
                    float newY = y / 64f;

                    noise.DomainWarp(ref newX, ref newY);

                    float noiseValue = (noise.GetNoise(newX, newY) + 1.0f) * 0.5f;
                    Color color = new Color(noiseValue, noiseValue, noiseValue);
                    customNoiseSettings.noiseTextureSample.SetPixel(x, y, color);
                }
            }
            customNoiseSettings.noiseTextureSample.Apply();
        }

        public Texture2D Text()
        {
            return customNoiseSettings.noiseTextureSample;
        }

        public float[] GetHeightForSettings(TerrainSettings.CustomNoiseSettings.Noise customNoiseSettings, float[] heights, Vector3[] points) {
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
            compute.SetBool("_invert", customNoiseSettings.general.invert);

            compute.SetInt("_seed", customNoiseSettings.general.randomizeOnStart ? dateTimeSeed : customNoiseSettings.general.seed);
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
            compute.SetFloat("_amplitude", customNoiseSettings.general.amplitude);
            compute.SetFloat("_smoothingMax", customNoiseSettings.general.smoothingMax);

            compute.SetInt("verticesAmount", points.Length);

            ComputeBuffer buffer = new ComputeBuffer(points.Length, 3 * sizeof(float));
            buffer.SetData(points);
            compute.SetBuffer(0, "vectors", buffer);

            ComputeBuffer heightBuffer = new ComputeBuffer(points.Length, sizeof(float));
            heightBuffer.SetData(heights);
            compute.SetBuffer(0, "heights", heightBuffer);

            /*
            ComputeBuffer maskValue = new ComputeBuffer(points.Length, sizeof(float));
            maskValue.SetData(maskHeights);
            compute.SetBuffer(0, "maskValue", maskValue);
            */

            compute.Dispatch(0, 512, 1, 1);

            heightBuffer.GetData(heights);

            buffer.Dispose();
            heightBuffer.Dispose();
            //maskValue.Dispose();

            return heights;
        }

        public float[] GetHeightForSettings(TerrainSettings.CustomNoiseSettings.Mask customNoiseSettings, float[] heights, Vector3[] points)
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

            /*
            ComputeBuffer maskValue = new ComputeBuffer(points.Length, sizeof(float));
            maskValue.SetData(maskHeights);
            compute.SetBuffer(0, "maskValue", maskValue);
            */

            compute.Dispatch(0, 512, 1, 1);

            heightBuffer.GetData(heights);

            buffer.Dispose();
            heightBuffer.Dispose();
            //maskValue.Dispose();

            return heights;
        }

        public float[] EvaluateSide(float[] heights, float[] maskHeights, Vector3[] points)
        {
            if (customNoiseSettings.mask.useMask)
            {
                maskHeights = GetHeightForSettings(customNoiseSettings.mask, maskHeights, points); // tar in settings och empty array av punkter, ger tillbaka ny array med värden
            }
            return GetHeightForSettings(customNoiseSettings.noise, maskHeights, points); // tar in settings och empty array av punkter, ger tillbaka ny array med värden * med gammla arrays värden
        }
    }
}