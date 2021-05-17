using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyOrbit
{
    public Vector3 currentVel;

    public CelestialBodyOrbit(SphereSettings sphereSettings)
    {
        currentVel = sphereSettings.initialVelocity;
    }

    public void UpdateCurrentVel(Vector3 velChange)
    {
        currentVel += velChange;
    }

    public void UpdatePosition(float timeStep, Transform transform, Vector3 currentVelocity)
    {
        transform.position += currentVelocity * timeStep;
    }
}
