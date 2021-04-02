using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBodyOrbit
{
    private SphereSettings sphereSettings;
    public Vector3 currentVelocity;

    public CelestialBodyOrbit(SphereSettings sphereSettings)
    {
        this.sphereSettings = sphereSettings;
        currentVelocity = sphereSettings.initialVelocity;
    }

    public void UpdatePosition(float timeStep, Transform transform, Material oceanMaterial)
    {
        /*
        foreach (Transform child in transform.Find("AllMeshes"))
        {
            //child.gameObject.GetComponent<Rigidbody>().position += currentVelocity * timeStep;
            //child.gameObject.GetComponent<Rigidbody>().AddForce(currentVelocity * timeStep);
            child.gameObject.GetComponent<Rigidbody>().MovePosition(child.position + currentVelocity * timeStep);
        }
        */

        //transform.gameObject.GetComponent<Rigidbody>().MovePosition(transform.position + currentVelocity * timeStep);
        //transform.gameObject.GetComponent<Rigidbody>().position += currentVelocity * timeStep;
        transform.position += currentVelocity * timeStep;
        if (oceanMaterial != null)
        {
            oceanMaterial.SetVector("_OceanCentre", transform.position);
        }
/*        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<Rigidbody>().position += currentVelocity * timeStep;
        }*/
    }
}
