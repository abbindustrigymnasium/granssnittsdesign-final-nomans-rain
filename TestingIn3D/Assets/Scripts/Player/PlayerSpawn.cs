using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerSpawn : MonoBehaviour
{
    public Vector3 initPos = new Vector3(125, 0, -100);
    // if collision stop all movement on rb = GameObject.Find("Player").GetComponent<Rigidbody>();
    // start with camera panned to planet???

    private void Start()
    {
        transform.position = initPos;
    }
}
