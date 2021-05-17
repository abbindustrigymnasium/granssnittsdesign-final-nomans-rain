using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxesEnemy : MonoBehaviour
{
    public bool playerInHitbox;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInHitbox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInHitbox = false;
        }
    }
}
