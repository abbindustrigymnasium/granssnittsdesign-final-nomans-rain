using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    // Bry dig ej om detta
    Transform CelestialBodies;
    CelestialBodiesInOrbit celestialBodiesInOrbit;
    public Vector3 startVelocity;

    // Bry dig om detta
    Rigidbody rb;
    Transform cameraTransform;
    [SerializeField]
    private float gravityConstForPlayer = -12f;
    [SerializeField]
    private float rotationSlerpSpeed = 50f;

    void Start()
    {
        // Skit i detta
        CelestialBodies = GameObject.Find("Celestial Bodies").transform;
        celestialBodiesInOrbit = CelestialBodies.GetComponent<CelestialBodiesInOrbit>();
        //rb.AddForce(startVelocity, ForceMode.Acceleration);

        // Tänk på detta
        rb = GetComponent<Rigidbody>();
        cameraTransform = GameObject.Find("PlayerCam").transform;
    }

    public void UpdatePlayerVelocity(float timeStep)
    {
        // Behöver ej ändra eller fucka runt med
        float accelerationSqrMagnitudeNow = 0f;
        Vector3 forceDirNow = Vector3.zero;
        Transform nearestChild = CelestialBodies.transform;
        foreach (Transform child in CelestialBodies) // gå igenom transforms och celestialBodyManagers istället
        {
            float sqrDist = (child.position - transform.position).sqrMagnitude;
            Vector3 forceDir = (child.position - transform.position).normalized;
            Vector3 acceleration = forceDir * celestialBodiesInOrbit.gravitationalConstant * child.GetComponent<CelestialBodyManager>().sphereSettings.mass / sqrDist;

            if (acceleration.sqrMagnitude > accelerationSqrMagnitudeNow)
            {
                accelerationSqrMagnitudeNow = acceleration.sqrMagnitude;
                forceDirNow = -forceDir;
                nearestChild = child;
            }
            //rb.AddForce(acceleration * timeStep * System.Convert.ToSingle(!isGrounded), ForceMode.Acceleration);
            //gameObject.GetComponent<Rigidbody>().AddForce(acceleration * timeStep);
            //Debug.Log(acceleration);
        }

        // Allt nedan kan du ändra/fucka runt med
        Vector3 gravityUp = (transform.position - nearestChild.position).normalized;
        Vector3 localUp = transform.up;

        rb.AddForce(gravityUp * gravityConstForPlayer);

        Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSlerpSpeed * Time.deltaTime);
        //cameraTransform.rotation = transform.rotation; // kameran vill ej rotera sig, idk why fuck this camera tbh
    }
}
