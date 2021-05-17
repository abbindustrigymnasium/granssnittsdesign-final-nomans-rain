using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CelestialBodiesInOrbit : MonoBehaviour
{
    [TextArea]
    public string Notes = "";

    [Header("Simulate")]
    [SerializeField]
    private bool simulateOrbit = false;
    [SerializeField]
    private int stepAmount = 0;
    [SerializeField]
    private float stepSize = 0;
    [SerializeField]
    private bool resetPos = false;

    private TrailRenderer[] trails;

    [Header("Fysics")]
    public float gravitationalConstant = 6.67408e-11f;
    [SerializeField]
    private float timeUpdateMultiplier = 0.000001f;

    [Header("celestialBodies")]
    public Transform currentPlanetTransform;

    private SunLight sunLight;
    private Transform sunTransform;
    private GameObject enemySpawner;

    private Vector3[] currentVelocities;

    private CelestialBodyManager[] celestialBodyManagers;
    private CelestialBodyOrbit[] celestialBodyOrbits;
    private Transform[] transforms;
    private int childrenAmount;

    private OceanEffects oceanEffects;

    private Transform lastPlanetTransform;

    public void Awake()
    {
        childrenAmount = transform.childCount;
        enemySpawner = GameObject.Find("Enemies");
        oceanEffects = GameObject.Find("Main Camera").GetComponent<OceanEffects>();
        sunTransform = GameObject.Find("Sun").transform;
        sunLight = GameObject.Find("Sun").transform.Find("Directional Light").GetComponent<SunLight>();

        celestialBodyManagers = new CelestialBodyManager[childrenAmount];
        celestialBodyOrbits = new CelestialBodyOrbit[childrenAmount];
        trails = new TrailRenderer[childrenAmount];
        transforms = new Transform[childrenAmount];
        currentVelocities = new Vector3[childrenAmount];

        int i = 0;
        foreach (Transform child in transform)
        {
            celestialBodyManagers[i] = child.gameObject.GetComponent<CelestialBodyManager>();
            transforms[i] = child;
            if (Application.isPlaying)
            {
                celestialBodyManagers[i].InitCelestialBody(this);
                celestialBodyManagers[i].CreateUndetailedMesh();
                celestialBodyManagers[i].SpawnTeleporter();
                child.Find("Trail").gameObject.SetActive(false);
            }
            else
            {
                currentVelocities[i] = celestialBodyManagers[i].sphereSettings.initialVelocity;
                trails[i] = child.Find("Trail").GetComponent<TrailRenderer>();
            }
            celestialBodyOrbits[i] = celestialBodyManagers[i].celestialBodyOrbit;

            i++;
        }
        if (Application.isPlaying)
        {
            OnShaderChange();
        }
    }

    public void OnChangePlanet(Transform newPlanet)
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (enemySpawner != null)
        {
            enemySpawner.GetComponent<EnemySpawner>().StartSpawning();
        }

        sunLight.SetCullingMask(newPlanet);
        Vector3 newPlanetVelocity = newPlanet.GetComponent<CelestialBodyManager>().celestialBodyOrbit.currentVel;
        Vector3 startPoint = currentPlanetTransform.position;

        newPlanet.GetComponent<CelestialBodyManager>().CreateDetailedMesh();
        //newPlanet.GetComponent<CelestialBodyManager>().ShowDetailedMesh();
        newPlanet.GetComponent<CelestialBodyManager>().ProjectFauna();
        newPlanet.GetComponent<CelestialBodyManager>().MakeStatic();
        newPlanet.Find("TeleporterRange(Clone)").GetComponent<TeleporterBehaviour>().enabled = true;

        if (lastPlanetTransform != null)
        {
            // creates a bunch of erros cuzz it tries to combine fauna with the mesh
            lastPlanetTransform.GetComponent<CelestialBodyManager>().InitCelestialBody(this);
            lastPlanetTransform.GetComponent<CelestialBodyManager>().CreateUndetailedMesh();
            lastPlanetTransform.GetComponent<CelestialBodyManager>().SpawnTeleporter();
            lastPlanetTransform.GetComponent<CelestialBodyManager>().HideDetailedMesh();
        }

        for (int i = 0; i < childrenAmount; i++)
        {
            if (transforms[i] == currentPlanetTransform)
            {
                celestialBodyManagers[i].MakeStatic();
            }
            else
            {
                celestialBodyManagers[i].DisableStatic(startPoint);
                currentVelocities[i] = celestialBodyOrbits[i].currentVel - newPlanetVelocity;
            }
        }

        //OnShaderChange();
        // --- \\
        lastPlanetTransform = newPlanet;
        // --- \\
    }

    public void OnShaderChange()
    {
        oceanEffects.SetMaterial(celestialBodyManagers, sunLight);
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            if (simulateOrbit)
            {
                simulateOrbit = false;
                Debug.Log("simulating: " + stepAmount);

                for (int i = 0; i < stepAmount; i++)
                {
                    TakeStep(stepSize);
                }
            }
            else if (resetPos)
            {
                resetPos = false;
                for (int i = 0; i < transforms.Length; i++)
                {
                    transforms[i].position = celestialBodyManagers[i].sphereSettings.worldPos;
                    currentVelocities[i] = celestialBodyManagers[i].sphereSettings.initialVelocity;
                }
            }
        }
        else
        {
            UpdateVelocity(Time.deltaTime * timeUpdateMultiplier);
        }

        // --- \\
        if (lastPlanetTransform != currentPlanetTransform)
        {
            OnChangePlanet(currentPlanetTransform);
        }
        // --- \\
    }

    void TakeStep(float timeStep)
    {
        for (int i = 0; i < childrenAmount; i++)
        {
            for (int q = 0; q < childrenAmount; q++)
            {
                if (q != i)
                {
                    float sqrDist = (transforms[q].position - transforms[i].position).sqrMagnitude;
                    Vector3 forceDir = (transforms[q].position - transforms[i].position).normalized;
                    Vector3 acceleration = forceDir * gravitationalConstant * celestialBodyManagers[q].sphereSettings.mass / sqrDist;
                    currentVelocities[i] += acceleration * timeStep;
                }
            }
            if (transforms[i].name != "Sun")
            {
                transforms[i].position += currentVelocities[i] * timeStep;
            }
        }
    }

    public void UpdateVelocity(float timeStep)
    {
        Vector3 startPlanetTransformVelocity = Vector3.zero;

        for (int i = 0; i < childrenAmount; i++)
        {
            for (int q = 0; q < childrenAmount; q++)
            {
                if (q != i)
                {
                    float sqrDist = (transforms[q].position - transforms[i].position).sqrMagnitude;
                    Vector3 forceDir = (transforms[q].position - transforms[i].position).normalized;
                    Vector3 acceleration = forceDir * gravitationalConstant * celestialBodyManagers[q].sphereSettings.mass / sqrDist;

                    if (transforms[i] == currentPlanetTransform)
                    {
                        celestialBodyOrbits[i].currentVel += acceleration * timeStep;
                        startPlanetTransformVelocity += acceleration * timeStep;
                    }
                    else
                    {
                        celestialBodyOrbits[i].currentVel += acceleration * timeStep;
                        currentVelocities[i] += acceleration * timeStep;
                    }
                }
            }
        }

        for (int i = 0; i < childrenAmount; i++)
        {
            if (transforms[i] != currentPlanetTransform)
            {
                currentVelocities[i] -= startPlanetTransformVelocity;
                celestialBodyManagers[i].celestialBodyOrbit.UpdatePosition(timeStep, transforms[i], currentVelocities[i]);
            }
        }

        Vector4[] sunDirs = new Vector4[sunLight.celestialBodyManagers.Length];
        Vector4[] planetCentre = new Vector4[sunLight.celestialBodyManagers.Length];
        for (int i = 0; i < sunLight.celestialBodyManagers.Length; i++)
        {
            sunDirs[i] = -(Quaternion.LookRotation(sunLight.celestialBodyManagers[i].transform.position - sunTransform.position, sunLight.celestialBodyManagers[i].transform.forward) * Vector3.forward);
            planetCentre[i] = sunLight.celestialBodyManagers[i].transform.position;
        }

        oceanEffects.UpdateOceans(sunDirs, planetCentre);
    }
}
