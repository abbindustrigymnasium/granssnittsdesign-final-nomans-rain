using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public float speed = 10f;
    private float timeAlive;
    Rigidbody rb;
    public LayerMask collisionMask; 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.MovePosition(transform.position + speed * transform.forward * Time.fixedDeltaTime);
        timeAlive += Time.fixedDeltaTime;
        if (timeAlive >= 5)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter (Collider col)
    {
        if ((collisionMask.value & (1<<col.gameObject.layer)) != 0) {
            Destroy(gameObject);
        }
    }
}
