using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarScript : MonoBehaviour
{
    [Header("Limits material usage to singular")]
    public bool optimize;
    public bool rotateStars;

    public StarSettings starSettings;
    [HideInInspector]
    public bool starSettingsFoldout = true;
    [HideInInspector]
    public Vector3[][] vectors;
    [HideInInspector]
    public int[][] triangles;
    private Mesh[] starMesh;

    float BiasFunction(float t, float bias)
    {
        float k = Mathf.Pow(1.0f - bias, 3);
        return (t * k) / (t * k - t + 1);
    }

    void Awake()
    {
        OnStarSettingsUpdated();
    }

    public void OnStarSettingsUpdated()
    {
        if (Application.isPlaying)
        {
            Random.InitState(starSettings.seed);
            CreateStars();
            PlaceStars();

            RenderMyMesh();

            if (optimize)
            {
                CombineStars();
            }
        }
    }

    public void CreateStars()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        starMesh = new Mesh[starSettings.numStars];

        vectors = new Vector3[starSettings.numStars][];
        triangles = new int[starSettings.numStars][];

        for (int i = 0; i < starSettings.numStars; i++)
        {
            GameObject childMeshComponent = new GameObject();

            childMeshComponent.transform.parent = gameObject.transform;

            Mesh childMesh = new Mesh();
            childMesh.MarkDynamic();

            childMeshComponent.AddComponent(typeof(MeshFilter));
            childMeshComponent.AddComponent(typeof(MeshRenderer));
            childMeshComponent.GetComponent<MeshFilter>().mesh = childMesh;
            starMesh[i] = childMesh;
            childMeshComponent.GetComponent<MeshRenderer>().sharedMaterial = starSettings.starMaterial;

            Color randColor = starSettings.spectrum.Evaluate(Random.Range(0f, 1f));
            childMeshComponent.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", randColor);
            //childMeshComponent.isStatic = true;
        }
    }

    public void PlaceStars()
    {
        for (int i = 0; i < starSettings.numStars; i++)
        {
            Vector3[] newVec = new Vector3[6];
            int[] newTri = new int[15];

            Vector3 normalizedPoint = Random.onUnitSphere;
            Vector3 point = normalizedPoint * starSettings.distanceRadius;
            float starRadius = Mathf.Lerp(starSettings.starRadiusMinMax.x, starSettings.starRadiusMinMax.y, BiasFunction(Random.Range(0f,1f), starSettings.starRadiusDistribution));

            Vector3 axisA = Vector3.Cross(normalizedPoint, Vector3.up).normalized;
            if (axisA == Vector3.zero)
            {
                axisA = Vector3.Cross(normalizedPoint, Vector3.forward).normalized;
            }
            Vector3 axisB = Vector3.Cross(normalizedPoint, axisA);

            newVec[0] = point;
            int randomDeg = Random.Range(0, 360);
            for (int w = 1; w < 6; w++)
            {
                float degree = ((72f * (w - 1) * Mathf.PI) + randomDeg) / 180f;
                Vector3 newPoint = point + starRadius * (axisA * Mathf.Sin(degree) + axisB * Mathf.Cos(degree));
                newVec[w] = newPoint;

                newTri[(w - 1) * 3] = 0;
                newTri[(w - 1) * 3 + 1] = w;
                if (w == 5)
                {
                    newTri[(w - 1) * 3 + 2] = 1;
                }
                else
                {
                    newTri[(w - 1) * 3 + 2] = w + 1;
                }
            }

            vectors[i] = newVec;
            triangles[i] = newTri;
        }
    }

    private void CombineStars()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        transform.GetComponent<MeshRenderer>().sharedMaterial = starSettings.starMaterial;

        int i = 1;
        while (i < meshFilters.Length)
        {
            combine[i - 1].mesh = meshFilters[i].sharedMesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;
            //meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        //transform.gameObject.SetActive(true);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void RenderMyMesh()
    {
        for (int i = 0; i < starSettings.numStars; i++)
        {
            starMesh[i].Clear();

            starMesh[i].vertices = vectors[i];
            starMesh[i].triangles = triangles[i];

            starMesh[i].RecalculateNormals();
            starMesh[i].RecalculateTangents();
            starMesh[i].RecalculateBounds();
        }
    }

    public void RotateStars(Vector3 normal)
    {
        if (optimize && rotateStars)
        {
            transform.rotation = Quaternion.LookRotation(normal);
        }
    }
}
