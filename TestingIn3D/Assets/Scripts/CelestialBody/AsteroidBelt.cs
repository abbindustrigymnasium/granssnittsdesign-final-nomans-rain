using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidBelt : MonoBehaviour
{
    public int asteroidAmount;
    public float asteroidCircleRad;
    public float asteroidSphereRad;
    public float asteroidDistBetween;
    public float asteroidOrbitSpeed;
    public Vector2 asteroidScale;

    public GameObject asteroidPrefab;

    private List<Vector3> allPos = new List<Vector3>();

    private Vector3 RandomPointOnCircle()
    {
        float rad = Random.Range(0, 359) * Mathf.Deg2Rad;
        Vector3 position = new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
        return position * asteroidCircleRad;
    }

    void Start()
    {
        for (int i = 0; i < asteroidAmount; i++)
        {
            SpawnAsteroid();
        }
        CombineAsteroids();
    }

    private void SpawnAsteroid()
    {
        Vector3 cirlcePos = RandomPointOnCircle();
        Vector3 spawnPos = cirlcePos + Random.insideUnitSphere * asteroidSphereRad;

        foreach (Vector3 pos in allPos)
        {
            if ((pos-spawnPos).sqrMagnitude < asteroidDistBetween) // take into account their respective scales
            {
                return;
            }
        }
        allPos.Add(spawnPos);

        GameObject asteroid = Instantiate(asteroidPrefab) as GameObject;
        Transform at = asteroid.transform;
        asteroid.transform.parent = transform;

        CelestialBodyManager cb = asteroid.GetComponent<CelestialBodyManager>();
        cb.InitAsAsteroid();

        asteroid.transform.position = spawnPos;
        asteroid.transform.rotation = transform.rotation;
        float scale = Random.Range(asteroidScale.x, asteroidScale.y);
        asteroid.transform.localScale = new Vector3(scale, scale, scale);
        gameObject.layer = transform.parent.GetComponent<CelestialBodyManager>().layer;
        cb.enabled = false;
    }

    private void CombineAsteroids()
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>();
        foreach (MeshFilter child in GetComponentsInChildren<MeshFilter>())
        {
            if (child.name != "AllMeshes")
            {
                meshFilters.Add(child);
            }
        }

        MeshFilter[] meshFiltersArray = meshFilters.ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilters.Count - 1];

        transform.GetComponent<MeshRenderer>().sharedMaterial = GameObject.Find("Asteroid(Clone)").GetComponent<CelestialBodyManager>().planetMaterial;

        int i = 1;
        while (i < meshFiltersArray.Length)
        {
            combine[i - 1].mesh = meshFiltersArray[i].sharedMesh;
            combine[i - 1].transform = meshFiltersArray[i].transform.localToWorldMatrix;

            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void Update()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * asteroidOrbitSpeed);
    }
}
