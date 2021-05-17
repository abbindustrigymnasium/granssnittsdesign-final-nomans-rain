using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitboxes : MonoBehaviour
{
    public class Enemy : MonoBehaviour
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

    public class Player : MonoBehaviour
    {
        public List<EnemyController> currentEnemiesInHitbox = new List<EnemyController>();

        private float hitBoxColliderInitRadius;
        private CapsuleCollider hitBoxCollider;

        void Start()
        {
            hitBoxCollider = gameObject.GetComponent<CapsuleCollider>();
            hitBoxColliderInitRadius = hitBoxCollider.radius;
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                return;
            }
            currentEnemiesInHitbox.Add(enemyController);
        }

        private void OnTriggerExit(Collider other)
        {
            EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
            if (enemyController == null)
            {
                return;
            }
            currentEnemiesInHitbox.Remove(enemyController);
        }

        public void UpdateHitBox(float Range)
        {
            hitBoxCollider.radius = hitBoxColliderInitRadius * Range;
        }
    }
}
