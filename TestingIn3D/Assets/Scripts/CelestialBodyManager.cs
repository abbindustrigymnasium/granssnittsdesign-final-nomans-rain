using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyManager : MonoBehaviour
{
    [TextArea]
    public string Notes = "";

    //    public ComputeShader computeShader;
    private Mesh[] mesh = new Mesh[20];

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

    public Material oceanMaterial;

    private GameObject childComponentForMeshes;

    private void Start()
    {
        this.transform.position = sphereSettings.worldPos;

        childComponentForMeshes = new GameObject();
        childComponentForMeshes.transform.parent = gameObject.transform;
        childComponentForMeshes.name = "AllMeshes";
/*
        childComponentForMeshes.AddComponent(typeof(Rigidbody));
        childComponentForMeshes.GetComponent<Rigidbody>().useGravity = false;
        //childComponentForMeshes.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
        //childComponentForMeshes.GetComponent<Rigidbody>().isKinematic = true;
        childComponentForMeshes.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
*/
        childComponentForMeshes.transform.position = transform.gameObject.transform.position;

        for (int i = 0; i < 20; i++)
        {
            GameObject childMeshComponent = new GameObject();
            childMeshComponent.layer = 9;
            childMeshComponent.transform.position = childComponentForMeshes.gameObject.transform.position;

            childMeshComponent.AddComponent(typeof(MeshFilter));
            childMeshComponent.AddComponent(typeof(MeshRenderer));

            /* FOR MOVING MESH
            childMeshComponent.AddComponent(typeof(Rigidbody));
            childMeshComponent.GetComponent<Rigidbody>().useGravity = false;
            childMeshComponent.GetComponent<Rigidbody>().angularDrag = 0;
            //childMeshComponent.GetComponent<Rigidbody>().isKinematic = false;
            childMeshComponent.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            childMeshComponent.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            childMeshComponent.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            //childMeshComponent.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
            childMeshComponent.GetComponent<Rigidbody>().centerOfMass = Vector3.zero;
            childMeshComponent.GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity;
            */

            childMeshComponent.AddComponent<MeshCollider>();
            //childMeshComponent.GetComponent<MeshCollider>().convex = true; FOR MOVING MESH
            childMeshComponent.name = "Mesh_" + i;
            childMeshComponent.transform.parent = childComponentForMeshes.transform;

            Mesh childMesh = new Mesh()
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            childMesh.MarkDynamic();
            childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh = childMesh;
            childMeshComponent.GetComponent<MeshCollider>().sharedMesh = childMeshComponent.gameObject.GetComponent<MeshFilter>().mesh;

            mesh[i] = childMesh;
        }

        celestialBodyOrbit = new CelestialBodyOrbit(sphereSettings);

        createDetailedMesh = new CreateDetailedMesh(sphereSettings);
        GenerateCelestialBody();
        GenerateColors();
    }

    public void OnSphereSettingsUpdated()
    {
        GenerateCelestialBody();
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

        terrainProjectionGPU = new TerrainProjectionGPU(shapeSettings, sphereSettings);
        ProjectFauna();
        //        terrainProjection = new TerrainProjection(shapeSettings, sphereSettings);
        RenderMyMesh();
    }

    public void OnTerrainSettingsUpdated()
    {
        for (int i = 0; i < 20; i++)
        {
            vectors[i] = (Vector3[])vectors_saved[i].Clone();
        }

        terrainProjectionGPU = new TerrainProjectionGPU(shapeSettings, sphereSettings);
        ProjectFauna();
        //        terrainProjection = new TerrainProjection(shapeSettings, sphereSettings);
        RenderMyMesh();
    }

    public void OnFaunaSettingsUpdated()
    {
        ProjectFauna();
    }

    public void ProjectFauna()
    {
        faunaProjection = new FaunaProjection(faunaTerrainSettings, faunaSettings, gameObject, colorSettings.shore.OceanRadius * colorSettings.shore.OceanRadius);
    }

    void RenderMyMesh()
    {
        for (int i = 0; i < 20; i++)
        {
            mesh[i].Clear();

            mesh[i].vertices = vectors[i];
            mesh[i].triangles = triangles[i];

            mesh[i].RecalculateNormals();
            mesh[i].RecalculateTangents();
            mesh[i].RecalculateBounds();
        }
    }

    public void OnColorSettingsUpdated()
    {
        GenerateColors();
        ProjectFauna();
    }

    void GenerateColors() // set variables
    {
        if (oceanMaterial != null) // shore
        {
            colorSettings.celestialBodyMaterial.SetColor("_DampShoreColor", colorSettings.shore.DampShoreColor);
            colorSettings.celestialBodyMaterial.SetColor("_DryShoreColor", colorSettings.shore.DryShoreColor);

            colorSettings.celestialBodyMaterial.SetFloat("_OceanRadius", colorSettings.shore.OceanRadius);
            oceanMaterial.SetFloat("_OceanRadius", colorSettings.shore.OceanRadius);
            
            colorSettings.celestialBodyMaterial.SetFloat("_DampShoreHeightAboveWater", colorSettings.shore.DampShoreHeightAboveWater);
            colorSettings.celestialBodyMaterial.SetFloat("_DryShoreHeightAboveWater", colorSettings.shore.DryShoreHeightAboveWater);
            colorSettings.celestialBodyMaterial.SetFloat("_ShoreBlend", colorSettings.shore.ShoreBlend);
            colorSettings.celestialBodyMaterial.SetFloat("_OceanBlend", colorSettings.shore.OceanBlend);
        }

        foreach (Transform child in childComponentForMeshes.transform)
        {
            child.gameObject.GetComponent<MeshRenderer>().sharedMaterial = colorSettings.celestialBodyMaterial;
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
