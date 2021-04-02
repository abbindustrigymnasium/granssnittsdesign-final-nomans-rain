using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunLight : MonoBehaviour
{
    Transform[] planets;
    void Start()
    {
        planets = GameObject.Find("Celestial Bodies").GetComponent<CelestialBodiesInOrbit>().transforms;
    }

    void Update()
    {
        transform.rotation = Quaternion.FromToRotation(transform.forward, planets[0].position) * transform.rotation;
    }
}
