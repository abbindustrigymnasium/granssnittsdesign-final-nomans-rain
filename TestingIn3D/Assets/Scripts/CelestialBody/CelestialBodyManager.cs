using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyManager : MonoBehaviour
{
    [TextArea]
    public string Notes = "";

    private Mesh[] mesh = new Mesh[20];
    private GameObject[] childMeshComponents = new GameObject[20];

    private CreateDetailedMesh createDetailedMesh;
    [HideInInspector]
    public static Vector3[][] vectors = new Vector3[20][];
    private static Vector3[][] vectors_saved = new Vector3[20][];
    [HideInInspector]
    public static int[][] triangles = new int[20][];

    private SimpleSphereProjection simpleSphereProjection;
    public TerrainProjection terrainProjection;
    public TerrainProjectionGPU terrainProjectionGPU;

    public FaunaProjection faunaProjection;
    public SpawnTeleporter spawnTeleporter;

    public ColorSettings colorSettings;
    public SphereSettings sphereSettings;
    public ShapeSettings shapeSettings;

    private FaunaTerrainSettings faunaTerrainSettings;
    public FaunaSettings faunaSettings;

    [HideInInspector]
    public bool colorSettingsFoldout;
    [HideInInspector]
    public bool sphereSettingsFoldout;
    [HideInInspector]
    public bool terrainSettingsFoldout;
    [HideInInspector]
    public bool faunaSettingsFoldout;

    public CelestialBodyOrbit celestialBodyOrbit;

    private GameObject childComponentForMeshes;

    public IColor planetColor;

    [HideInInspector]
    public Material planetMaterial;

    [HideInInspector]
    public float oceanRadius;

    [HideInInspector]
    public Texture2D[] noiseTextures;

    [HideInInspector]
    public int layer;

    private int initTime;

    [HideInInspector]
    public struct OceanAtmosphere
    {
        public bool hasShader;

        public Vector4 planetCentre;
        public Color oceanDeep;
        public Color oceanShallow;
        public Color specularCol;
        public float oceanRadius;
        public float depthMultiplier;
        public float alphaMultiplier;
        public float smoothness;
        public bool selfGlow;

        public float atmosphereRadius;
        public int numInScatteringPoints;
        public int numOpticalDepthPoints;
        public Vector4 scatteringCoefficients;
        public float scatteringStrength;
        public float densityFalloff;

        public float waveSpeed;
        public Texture2D waveNormalA;
        public Texture2D waveNormalB;
        public float waveNormalScale;
        public float waveStrength;
    }
    [HideInInspector]
    public OceanAtmosphere oceanAtmosphere;

    private CelestialBodiesInOrbit celestialBodiesInOrbit;

    private bool hasDetailedMesh = false;
    private void EnableRecursive(GameObject obj, bool setEnabled)
    {
        obj.SetActive(setEnabled);

        foreach (Transform child in obj.transform)
        {
            EnableRecursive(child.gameObject, setEnabled);
        }
    }
    public void ShowDetailedMesh()
    {
        childComponentForMeshes.GetComponent<MeshRenderer>().enabled = false;
        if (hasDetailedMesh)
        {
            EnableRecursive(childComponentForMeshes, true);
        }
        else
        {
            hasDetailedMesh = true;
            CreateDetailedMesh();
        }
    }
    public void HideDetailedMesh()
    {
        childComponentForMeshes.GetComponent<MeshRenderer>().enabled = true;
        EnableRecursive(childComponentForMeshes, false);
        childComponentForMeshes.SetActive(true);
    }

    private void StaticRecursive(GameObject obj, bool setStatic)
    {
        obj.isStatic = setStatic;

        foreach (Transform child in obj.transform)
        {
            StaticRecursive(child.gameObject, setStatic);
        }
    }
    public void MakeStatic()
    {
        transform.position = Vector3.zero;
        StaticRecursive(gameObject, true);
    }
    public void DisableStatic(Vector3 posDiff)
    {
        StaticRecursive(gameObject, false);
        transform.position -= posDiff;
    }

    public void CreateDetailedMesh()
    {
        childComponentForMeshes.GetComponent<MeshRenderer>().enabled = false;
        sphereSettings.resolution = SphereSettings.Resolution.MaxRes;
        faunaSettings.enabled = true;

        for (int i = 0; i < 20; i++)
        {
            if (childMeshComponents[i] != null)
            {
                Destroy(childMeshComponents[i]);
            }
            GameObject childMeshComponent = new GameObject();
            childMeshComponent.layer = layer;
            childMeshComponent.transform.position = childComponentForMeshes.gameObject.transform.position;

            childMeshComponent.AddComponent(typeof(MeshFilter));
            childMeshComponent.AddComponent(typeof(MeshRenderer));

            childMeshComponent.AddComponent<MeshCollider>();
            childMeshComponent.name = "Mesh_" + i;
            childMeshComponent.transform.parent = childComponentForMeshes.transform;

            Mesh childMesh = SetMeshIndexFormat(false);

            childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh = childMesh;
            childMeshComponent.GetComponent<MeshCollider>().sharedMesh = childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh;
            childMeshComponent.GetComponent<MeshCollider>().enabled = true;
            childMeshComponents[i] = childMeshComponent;
            mesh[i] = childMesh;
        }

        celestialBodyOrbit = new CelestialBodyOrbit(sphereSettings);
        createDetailedMesh = new CreateDetailedMesh(sphereSettings);

        GenerateCelestialBody();
    }
    public void CreateUndetailedMesh()
    {
        sphereSettings.resolution = SphereSettings.Resolution.LowRes;

        // "delete" fauna
        faunaSettings.enabled = false;


        GenerateCelestialBody();

        Vector3 oldpos = transform.position;
        transform.position = Vector3.zero;

        MeshFilter[] meshFilters = childComponentForMeshes.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        childComponentForMeshes.GetComponent<MeshRenderer>().sharedMaterial = planetMaterial;

        int i = 1;
        while (i < meshFilters.Length)
        {
            combine[i - 1].mesh = meshFilters[i].sharedMesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }
        childComponentForMeshes.GetComponent<MeshFilter>().mesh = new Mesh();
        childComponentForMeshes.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

        foreach (Transform child in childComponentForMeshes.transform)
        {
            Destroy(child.gameObject);
        }

        transform.position = oldpos;
    }

    public void InitAsTest()
    {
        sphereSettings.resolution = SphereSettings.Resolution.customResolution;

        transform.position = sphereSettings.worldPos;
        oceanAtmosphere.planetCentre = sphereSettings.worldPos;

        childComponentForMeshes = new GameObject();
        childComponentForMeshes.transform.parent = gameObject.transform;
        childComponentForMeshes.name = "AllMeshes";
        childComponentForMeshes.AddComponent(typeof(MeshFilter));
        childComponentForMeshes.AddComponent(typeof(MeshRenderer));
        childComponentForMeshes.layer = 9;
        childComponentForMeshes.transform.position = transform.gameObject.transform.position;

        for (int i = 0; i < 20; i++)
        {
            GameObject childMeshComponent = new GameObject();
            childMeshComponent.layer = 9;
            childMeshComponent.transform.position = childComponentForMeshes.gameObject.transform.position;

            childMeshComponent.AddComponent(typeof(MeshFilter));
            childMeshComponent.AddComponent(typeof(MeshRenderer));

            childMeshComponent.AddComponent<MeshCollider>();
            childMeshComponent.name = "Mesh_" + i;
            childMeshComponent.transform.parent = childComponentForMeshes.transform;

            Mesh childMesh = SetMeshIndexFormat(false);

            childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh = childMesh;
            childMeshComponent.GetComponent<MeshCollider>().sharedMesh = childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh;
            childMeshComponent.GetComponent<MeshCollider>().enabled = true;
            childMeshComponents[i] = childMeshComponent;
            mesh[i] = childMesh;
        }

        celestialBodyOrbit = new CelestialBodyOrbit(sphereSettings);
        createDetailedMesh = new CreateDetailedMesh(sphereSettings);

        initTime = System.DateTime.Now.Millisecond;

        GenerateCelestialBody();
        MakeStatic();
    }

    public void InitAsAsteroid()
    {
        sphereSettings.resolution = SphereSettings.Resolution.customResolution;

        childComponentForMeshes = new GameObject();
        childComponentForMeshes.transform.parent = gameObject.transform;
        childComponentForMeshes.name = "AllMeshes";

        for (int i = 0; i < 20; i++)
        {
            GameObject childMeshComponent = new GameObject();

            childMeshComponent.AddComponent(typeof(MeshFilter));
            childMeshComponent.AddComponent(typeof(MeshRenderer));

            childMeshComponent.name = "Mesh_" + i;
            childMeshComponent.transform.parent = childComponentForMeshes.transform;

            Mesh childMesh = SetMeshIndexFormat(false);

            childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh = childMesh;
            childMeshComponents[i] = childMeshComponent;
            mesh[i] = childMesh;
        }

        createDetailedMesh = new CreateDetailedMesh(sphereSettings);

        initTime = System.DateTime.Now.Millisecond;

        createDetailedMesh.CreateDetailedTriangles();
        vectors = createDetailedMesh.GetVectors();
        triangles = createDetailedMesh.GetTriangles();
        simpleSphereProjection = new SimpleSphereProjection();
        planetColor = ColorFactory.CreateColor(colorSettings);
        planetMaterial = planetColor.GetColors();

        terrainProjectionGPU = new TerrainProjectionGPU(shapeSettings, sphereSettings, ref noiseTextures, initTime);
        for (int i = 0; i < 20; i++)
        {
            mesh[i].vertices = vectors[i];
            mesh[i].triangles = triangles[i];
            mesh[i].RecalculateNormals();
            mesh[i].RecalculateTangents();
            mesh[i].RecalculateBounds();
        }
    }

    public void InitCelestialBody(CelestialBodiesInOrbit celestialBodiesInOrbit)
    {
        initTime = System.DateTime.Now.Millisecond;
        if (childComponentForMeshes)
        {
            return;
        }

        this.celestialBodiesInOrbit = celestialBodiesInOrbit;

        transform.position = sphereSettings.worldPos;
        oceanAtmosphere.planetCentre = sphereSettings.worldPos;

        childComponentForMeshes = new GameObject();
        childComponentForMeshes.transform.parent = gameObject.transform;
        childComponentForMeshes.name = "AllMeshes";
        childComponentForMeshes.AddComponent(typeof(MeshFilter));
        childComponentForMeshes.AddComponent(typeof(MeshRenderer));
        childComponentForMeshes.layer = layer;
        childComponentForMeshes.transform.position = transform.gameObject.transform.position;

        for (int i = 0; i < 20; i++)
        {
            GameObject childMeshComponent = new GameObject();
            childMeshComponent.layer = layer;
            childMeshComponent.transform.position = childComponentForMeshes.gameObject.transform.position;

            childMeshComponent.AddComponent(typeof(MeshFilter));
            childMeshComponent.AddComponent(typeof(MeshRenderer));

            childMeshComponent.name = "Mesh_" + i;
            childMeshComponent.transform.parent = childComponentForMeshes.transform;

            Mesh childMesh = SetMeshIndexFormat(false);

            childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh = childMesh;
            childMeshComponents[i] = childMeshComponent;
            mesh[i] = childMesh;
        }

        celestialBodyOrbit = new CelestialBodyOrbit(sphereSettings);
        createDetailedMesh = new CreateDetailedMesh(sphereSettings);
    }
    
    private Mesh SetMeshIndexFormat(bool isDetailed)
    {
        if (isDetailed)
        {
            return new Mesh()
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
        }
        return new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt16
        };
    }

    public void SpawnTeleporter()
    {
        spawnTeleporter = new SpawnTeleporter(oceanRadius, layer, transform, vectors);
    }

    public void OnSphereSettingsUpdated()
    {
        InitCelestialBody(celestialBodiesInOrbit);
        CreateDetailedMesh();
        SpawnTeleporter();
    }

    public void GenerateCelestialBody()
    {
        createDetailedMesh.CreateDetailedTriangles();
        vectors = createDetailedMesh.GetVectors();
        triangles = createDetailedMesh.GetTriangles();
        simpleSphereProjection = new SimpleSphereProjection();

        for (int i = 0; i < 20; i++)
        {
            vectors_saved[i] = (Vector3[])vectors[i].Clone();
        }

        terrainProjectionGPU = new TerrainProjectionGPU(shapeSettings, sphereSettings, ref noiseTextures, initTime);
        OnColorSettingsUpdated(true);
        ProjectFauna();
        RenderMyMesh();
    }

    public void OnTerrainSettingsUpdated()
    {
        for (int i = 0; i < 20; i++)
        {
            vectors[i] = (Vector3[])vectors_saved[i].Clone();
        }
        terrainProjectionGPU = new TerrainProjectionGPU(shapeSettings, sphereSettings, ref noiseTextures, initTime);
        ProjectFauna();
        RenderMyMesh();
    }

    public void OnFaunaSettingsUpdated()
    {
        ProjectFauna();
    }

    public void ProjectFauna()
    {
        faunaProjection = new FaunaProjection(faunaTerrainSettings, faunaSettings, gameObject, oceanRadius * oceanRadius, layer);
    }

    void RenderMyMesh()
    {
        for (int i = 0; i < 20; i++)
        {
            mesh[i] = SetMeshIndexFormat(true);
            childMeshComponents[i].GetComponent<MeshFilter>().mesh = mesh[i];
            if (childMeshComponents[i].GetComponent<MeshCollider>() != null)
            {
                childMeshComponents[i].GetComponent<MeshCollider>().sharedMesh = mesh[i];
            }

            mesh[i].Clear();

            mesh[i].vertices = vectors[i];
            mesh[i].triangles = triangles[i];

            mesh[i].RecalculateNormals();
            mesh[i].RecalculateTangents();
            mesh[i].RecalculateBounds();
        }
    }

    private void OnColorSettingsUpdated(bool isInit)
    {
        planetColor = ColorFactory.CreateColor(colorSettings);
        oceanRadius = planetColor.GetOceanRadius();
        planetMaterial = planetColor.GetColors();
        oceanAtmosphere = planetColor.GetOceanAtmosphere(oceanAtmosphere.planetCentre);

        foreach (Transform child in childComponentForMeshes.transform)
        {
            child.gameObject.GetComponent<MeshRenderer>().sharedMaterial = planetMaterial;
        }
    }

    public void OnColorSettingsUpdated()
    {
        planetColor = ColorFactory.CreateColor(colorSettings);
        oceanRadius = planetColor.GetOceanRadius();
        planetMaterial = planetColor.GetColors();
        oceanAtmosphere = planetColor.GetOceanAtmosphere(oceanAtmosphere.planetCentre);

        foreach (Transform child in childComponentForMeshes.transform)
        {
            child.gameObject.GetComponent<MeshRenderer>().sharedMaterial = planetMaterial;
        }

        //ProjectFauna();

        if (celestialBodiesInOrbit != null)
        {
            celestialBodiesInOrbit.OnShaderChange();
        }
    }

    public interface IColor
    {
        Material GetColors();
        float GetOceanRadius();
        OceanAtmosphere GetOceanAtmosphere(Vector3 planetCentre);
    }

    public static class ColorFactory
    {
        public static IColor CreateColor(ColorSettings colorSettings)
        {
            switch (colorSettings.planetType)
            {
                case ColorSettings.PlanetType.FirstPlanet:
                    return new FirstPlanetColor(colorSettings.firstPlanet);
                case ColorSettings.PlanetType.SecondPlanet:
                    return new SecondPlanetColor(colorSettings.secondPlanet);
                case ColorSettings.PlanetType.Moon:
                    return new MoonColor(colorSettings.moon);
                case ColorSettings.PlanetType.Sun:
                    return new SunColor(colorSettings.sun);
            }
            return null;
        }
    }

    public class FirstPlanetColor : IColor
    {
        ColorSettings.FirstPlanet colorSettings;
        CelestialBodyManager.OceanAtmosphere oceanAtmosphere;

        public FirstPlanetColor(ColorSettings.FirstPlanet colorSettings)
        {
            this.colorSettings = colorSettings;
            
            colorSettings.celestialBodyMaterial = new Material(Shader.Find("Planet/First"));

            // Ocean
            oceanAtmosphere.hasShader = true;
            oceanAtmosphere.oceanDeep = colorSettings.ocean.OceanDeep;
            oceanAtmosphere.oceanShallow = colorSettings.ocean.OceanShallow;
            oceanAtmosphere.specularCol = colorSettings.ocean.SpecularCol;
            oceanAtmosphere.oceanRadius = colorSettings.ocean.OceanRadius;
            oceanAtmosphere.depthMultiplier = colorSettings.ocean.DepthMultiplier;
            oceanAtmosphere.alphaMultiplier = colorSettings.ocean.AlphaMultiplier;
            oceanAtmosphere.smoothness = colorSettings.ocean.Smoothness;
            oceanAtmosphere.selfGlow = colorSettings.ocean.SelfGlow;

            // Atmosphere
            oceanAtmosphere.atmosphereRadius = colorSettings.atmosphere.atmosphereRadius;
            oceanAtmosphere.numInScatteringPoints = colorSettings.atmosphere.numInScatteringPoints;
            oceanAtmosphere.numOpticalDepthPoints = colorSettings.atmosphere.numOpticalDepthPoints;
            oceanAtmosphere.scatteringCoefficients = colorSettings.atmosphere.scatteringCoefficients;
            oceanAtmosphere.scatteringStrength = colorSettings.atmosphere.scatteringStrength;
            oceanAtmosphere.densityFalloff = colorSettings.atmosphere.densityFalloff;

            oceanAtmosphere.waveSpeed = colorSettings.ocean.WaveSpeed;
            oceanAtmosphere.waveNormalA = colorSettings.ocean.WaveNormalA;
            oceanAtmosphere.waveNormalB = colorSettings.ocean.WaveNormalB;
            oceanAtmosphere.waveNormalScale = colorSettings.ocean.WaveNormalScale;
            oceanAtmosphere.waveStrength = colorSettings.ocean.WaveStrength;

            // Shore
            colorSettings.celestialBodyMaterial.SetColor("_DampShoreColor", colorSettings.shore.DampShoreColor);
            colorSettings.celestialBodyMaterial.SetColor("_DryShoreColor", colorSettings.shore.DryShoreColor);
            colorSettings.celestialBodyMaterial.SetFloat("_OceanRadius", colorSettings.ocean.OceanRadius);
            colorSettings.celestialBodyMaterial.SetFloat("_DampShoreHeightAboveWater", colorSettings.shore.DampShoreHeightAboveWater);
            colorSettings.celestialBodyMaterial.SetFloat("_DryShoreHeightAboveWater", colorSettings.shore.DryShoreHeightAboveWater);
            colorSettings.celestialBodyMaterial.SetFloat("_ShoreBlend", colorSettings.shore.ShoreBlend);
            colorSettings.celestialBodyMaterial.SetFloat("_OceanBlend", colorSettings.shore.OceanBlend);
            // triplanar
            colorSettings.celestialBodyMaterial.SetFloat("_BlendSharpnessShore", colorSettings.shore.BlendSharpness);
            colorSettings.celestialBodyMaterial.SetFloat("_ScaleNormalMapShore", colorSettings.shore.ScaleNormalMap);
            colorSettings.celestialBodyMaterial.SetTexture("_NormalMapShore", colorSettings.shore.NormalMap);

            // Biome
            colorSettings.celestialBodyMaterial.SetFloat("_FlatColBlend", colorSettings.biomes.FlatColBlend);
            colorSettings.celestialBodyMaterial.SetFloat("_BiomeHeightAboveShore", colorSettings.biomes.BiomeHeightAboveShore);
            colorSettings.celestialBodyMaterial.SetColor("_BiomeALow", colorSettings.biomes.BiomeALow);
            colorSettings.celestialBodyMaterial.SetColor("_BiomeAHigh", colorSettings.biomes.BiomeAHigh);
            // triplanar
            colorSettings.celestialBodyMaterial.SetFloat("_BlendSharpnessGrass", colorSettings.biomes.BlendSharpness);
            colorSettings.celestialBodyMaterial.SetFloat("_ScaleNormalMapGrass", colorSettings.biomes.ScaleNormalMap);
            colorSettings.celestialBodyMaterial.SetTexture("_NormalMapGrass", colorSettings.biomes.NormalMap);

            // Mountain
            colorSettings.celestialBodyMaterial.SetColor("_MountainLow", colorSettings.mountains.MountainLow);
            colorSettings.celestialBodyMaterial.SetColor("_MountainHigh", colorSettings.mountains.MountainHigh);
            colorSettings.celestialBodyMaterial.SetFloat("_MountainTopBlend", colorSettings.mountains.MountainTopBlend);
            colorSettings.celestialBodyMaterial.SetFloat("_MaxFlatHeight", colorSettings.mountains.MaxFlatHeight);
            colorSettings.celestialBodyMaterial.SetFloat("_SteepnessThresholdLow", colorSettings.mountains.SteepnessThresholdLow);
            colorSettings.celestialBodyMaterial.SetFloat("_SteepnessThresholdHigh", colorSettings.mountains.SteepnessThresholdHigh);
            colorSettings.celestialBodyMaterial.SetFloat("_MountainBlend", colorSettings.mountains.MountainBlend);
            colorSettings.celestialBodyMaterial.SetFloat("_HighUpMountainSteepnessDistribution", colorSettings.mountains.HighUpMountainSteepnessDistribution);
            // triplanar
            colorSettings.celestialBodyMaterial.SetFloat("_BlendSharpnessMountain", colorSettings.mountains.BlendSharpness);
            colorSettings.celestialBodyMaterial.SetFloat("_ScaleNormalMapMountain", colorSettings.mountains.ScaleNormalMap);
            colorSettings.celestialBodyMaterial.SetTexture("_NormalMapMountain", colorSettings.mountains.NormalMap);
        }

        public OceanAtmosphere GetOceanAtmosphere(Vector3 planetCentre)
        {
            oceanAtmosphere.planetCentre = planetCentre;
            return oceanAtmosphere;
        }

        public Material GetColors()
        {
            return colorSettings.celestialBodyMaterial;
        }

        public float GetOceanRadius()
        {
            return colorSettings.ocean.OceanRadius;
        }
    }

    public class SecondPlanetColor : IColor
    {
        ColorSettings.SecondPlanet colorSettings;
        CelestialBodyManager.OceanAtmosphere oceanAtmosphere;

        public SecondPlanetColor(ColorSettings.SecondPlanet colorSettings)
        {
            this.colorSettings = colorSettings;
            colorSettings.celestialBodyMaterial = new Material(Shader.Find("Planet/First"));

            // Ocean
            oceanAtmosphere.hasShader = true;
            oceanAtmosphere.oceanDeep = colorSettings.ocean.OceanDeep;
            oceanAtmosphere.oceanShallow = colorSettings.ocean.OceanShallow;
            oceanAtmosphere.specularCol = colorSettings.ocean.SpecularCol;
            oceanAtmosphere.oceanRadius = colorSettings.ocean.OceanRadius;
            oceanAtmosphere.depthMultiplier = colorSettings.ocean.DepthMultiplier;
            oceanAtmosphere.alphaMultiplier = colorSettings.ocean.AlphaMultiplier;
            oceanAtmosphere.smoothness = colorSettings.ocean.Smoothness;
            oceanAtmosphere.selfGlow = colorSettings.ocean.SelfGlow;

            // Atmosphere
            oceanAtmosphere.atmosphereRadius = colorSettings.atmosphere.atmosphereRadius;
            oceanAtmosphere.numInScatteringPoints = colorSettings.atmosphere.numInScatteringPoints;
            oceanAtmosphere.numOpticalDepthPoints = colorSettings.atmosphere.numOpticalDepthPoints;
            oceanAtmosphere.scatteringCoefficients = colorSettings.atmosphere.scatteringCoefficients;
            oceanAtmosphere.scatteringStrength = colorSettings.atmosphere.scatteringStrength;
            oceanAtmosphere.densityFalloff = colorSettings.atmosphere.densityFalloff;

            oceanAtmosphere.waveSpeed = colorSettings.ocean.WaveSpeed;
            oceanAtmosphere.waveNormalA = colorSettings.ocean.WaveNormalA;
            oceanAtmosphere.waveNormalB = colorSettings.ocean.WaveNormalB;
            oceanAtmosphere.waveNormalScale = colorSettings.ocean.WaveNormalScale;
            oceanAtmosphere.waveStrength = colorSettings.ocean.WaveStrength;
        }

        public OceanAtmosphere GetOceanAtmosphere(Vector3 planetCentre)
        {
            oceanAtmosphere.planetCentre = planetCentre;
            return oceanAtmosphere;
        }

        public Material GetColors()
        {
            return colorSettings.celestialBodyMaterial;
        }

        public float GetOceanRadius()
        {
            return colorSettings.ocean.OceanRadius;
        }
    }

    public class MoonColor : IColor
    {
        ColorSettings.Moon colorSettings;
        CelestialBodyManager.OceanAtmosphere oceanAtmosphere;

        public MoonColor(ColorSettings.Moon colorSettings)
        {
            this.colorSettings = colorSettings;
            colorSettings.celestialBodyMaterial = new Material(Shader.Find("Planet/Moon"));
            //colorSettings.celestialBodyMaterial = Resources.Load("PlanetMaterials/Moon Material", typeof(Material)) as Material;

            colorSettings.celestialBodyMaterial.SetTexture("_NormalMapA", colorSettings.normalMap.NormalMapTexture);
            //colorSettings.celestialBodyMaterial.SetFloat("_ScaleTexture", colorSettings.normalMap.ScaleTexture);
            colorSettings.celestialBodyMaterial.SetFloat("_BlendSharpness", colorSettings.normalMap.BlendSharpness);
            colorSettings.celestialBodyMaterial.SetFloat("_ScaleNormalMapA", colorSettings.normalMap.ScaleNormalMap);

            oceanAtmosphere.hasShader = false;
        }

        public OceanAtmosphere GetOceanAtmosphere(Vector3 planetCentre)
        {
            oceanAtmosphere.planetCentre = planetCentre;
            return oceanAtmosphere;
        }

        public Material GetColors()
        {
            return colorSettings.celestialBodyMaterial;
        }

        public float GetOceanRadius()
        {
            return 0;
        }
    }

    public class SunColor : IColor
    {
        ColorSettings.Sun colorSettings;
        CelestialBodyManager.OceanAtmosphere oceanAtmosphere;

        public SunColor(ColorSettings.Sun colorSettings)
        {
            this.colorSettings = colorSettings;
            colorSettings.celestialBodyMaterial = new Material(Shader.Find("Planet/Sun With Noise"));
            //colorSettings.celestialBodyMaterial = Resources.Load("PlanetMaterials/Sun With Noise Material", typeof(Material)) as Material;

            colorSettings.celestialBodyMaterial.SetColor("_SunColorUndertone", colorSettings._SunColorUndertone);
            colorSettings.celestialBodyMaterial.SetColor("_SunColorMidtone", colorSettings._SunColorMidtone);
            colorSettings.celestialBodyMaterial.SetColor("_SunColorOvertone", colorSettings._SunColorOvertone);

            oceanAtmosphere.hasShader = false;
        }

        public OceanAtmosphere GetOceanAtmosphere(Vector3 planetCentre)
        {
            oceanAtmosphere.planetCentre = planetCentre;
            return oceanAtmosphere;
        }

        public Material GetColors()
        {
            return colorSettings.celestialBodyMaterial;
        }

        public float GetOceanRadius()
        {
            return 0;
        }
    }

    private void OnDrawGizmos()
    {
        if (vectors.Length == 0)
        {
            return;
        }

/*        for (int i = 0; i < vectors.Length; i++)
        {
            Gizmos.DrawSphere(vectors[i], 0.05f);
        }*/
/*
        Gizmos.color = Color.red;
        for (int i = 0; i < triangles.Length / 3; i++)
        {
            int z = i * 3;
            Gizmos.DrawLine(vectors[triangles[z]], vectors[triangles[z + 1]]);
            Gizmos.DrawLine(vectors[triangles[z]], vectors[triangles[z + 2]]);
            Gizmos.DrawLine(vectors[triangles[z + 1]], vectors[triangles[z + 2]]);
        }
*/
    }
}
