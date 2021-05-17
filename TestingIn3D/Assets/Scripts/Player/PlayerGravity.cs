using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerGravity : MonoBehaviour
{
    Rigidbody rb;
    CinemachineFreeLook vcam;
    public float gravityConstForPlayer = -12f;
    [HideInInspector]
    public float gravity;
    [SerializeField]
    private float standingUprightSlerpSpeed = 50f;
    [SerializeField]
    private float lookSlerpSpeed = 10f;

    private Transform camMainTransform;
    private Transform camPosTransform;

    void Start()
    {
        //                            DEBUG                            //
        // ----------------------------------------------------------- //
        GameObject DebugMode = GameObject.Find("TestPlanet");
        if (DebugMode != null)
        {
            CelestialBodyManager cbm = DebugMode.GetComponent<CelestialBodyManager>();
            cbm.InitAsTest();

            if (GameObject.Find("Enemies"))
            {
                GameObject.Find("Enemies").GetComponent<EnemySpawner>().StartSpawning();
            }
        }
        // ----------------------------------------------------------- //

        rb = GetComponent<Rigidbody>();
        camMainTransform = GameObject.Find("Main Camera").transform;
        camPosTransform = GameObject.Find("PlayerCamPos").transform;
        gravity = gravityConstForPlayer;
    }

    public void FixedUpdate()
    {
        camPosTransform.position = transform.position;

        Vector3 gravityUp = camPosTransform.position.normalized;
        Vector3 localUp = camPosTransform.up;

        rb.AddForce(gravityUp * gravity);

        Quaternion standingUpright = Quaternion.FromToRotation(localUp, gravityUp) * camPosTransform.rotation;
        camPosTransform.rotation = Quaternion.Slerp(camPosTransform.rotation, standingUpright, standingUprightSlerpSpeed * Time.deltaTime);

        Quaternion lookDirectionLocalUp = Quaternion.FromToRotation(camMainTransform.up, camPosTransform.up) * camMainTransform.rotation;
        Quaternion targetLookDirection = Quaternion.FromToRotation(camPosTransform.forward, lookDirectionLocalUp * Vector3.forward) * camPosTransform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetLookDirection, lookSlerpSpeed * Time.deltaTime);
    }
}
