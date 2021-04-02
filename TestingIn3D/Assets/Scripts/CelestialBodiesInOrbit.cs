using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CelestialBodiesInOrbit : MonoBehaviour
{
    [Header("Simulate")]
    public bool simulateOrbit = false;
    public int stepAmount = 0;
    public float stepSize = 0;
    public bool resetPos = false;

    private TrailRenderer[] trails;

    [Header("Fysics")]
    public float gravitationalConstant = 6.67408e-11f;

    [Header("celestialBodies")]
    public Vector3[] currentVelocities;
    public CelestialBodyManager[] celestialBodyManagers;
    public Transform[] transforms;
    private int childrenAmount = 0;

    [Header("Player")]
    PlayerGravity playerGravity;

    public void Start()
    {
        if (GameObject.Find("Player"))
        {
            playerGravity = GameObject.Find("Player").GetComponent<PlayerGravity>();
        }

        celestialBodyManagers = new CelestialBodyManager[transform.childCount];

        trails = new TrailRenderer[transform.childCount];
        transforms = new Transform[transform.childCount];
        foreach (Transform child in transform)
        {
            if (transform == child.parent)
            {
                celestialBodyManagers[childrenAmount] = child.gameObject.GetComponent<CelestialBodyManager>();
                if (Application.isPlaying)
                {
                    transforms[childrenAmount] = child;
                    child.Find("Trail").gameObject.SetActive(false);
                }
                else
                {
                    transforms[childrenAmount] = child;
                    trails[childrenAmount] = child.Find("Trail").GetComponent<TrailRenderer>();
                }

                childrenAmount++;
            }
        }
    }

    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
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
    }

    private void FixedUpdate()
    {
        UpdateVelocity(Time.deltaTime / 10000f);
        //UpdateVelocity(Time.deltaTime / 1000f);
        //UpdateVelocity(Time.deltaTime / 100f);
        //UpdateVelocity(0);
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
            //            trails[i].AddPosition(transforms[i].position);
        }
    }

    public void UpdateVelocity(float timeStep)
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
                    celestialBodyManagers[i].celestialBodyOrbit.currentVelocity += acceleration * timeStep;
                }
            }
            if (transforms[i].name != "Sun")
            {
                celestialBodyManagers[i].celestialBodyOrbit.UpdatePosition(timeStep, transforms[i], celestialBodyManagers[i].oceanMaterial);
            }
//            Debug.Log(celestialBodyManagers[i].celestialBodyOrbit.currentVelocity);
        }
        //playerGravity.updatePlayerVelocity(Time.deltaTime / 5000f);

        if (GameObject.Find("Player"))
        {
            playerGravity.UpdatePlayerVelocity(timeStep);
        }
    }
}
