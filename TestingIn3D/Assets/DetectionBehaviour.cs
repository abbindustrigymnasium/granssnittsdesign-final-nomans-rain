using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionBehaviour : MonoBehaviour
{
    public GameObject spider;
    private EnemyController spiderController;
    private Animator animator;

    private void Start()
    {
        spiderController = spider.GetComponent<EnemyController>();
        animator = spider.GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider col)
    {
      if (col.name == "Player" )
        {
            if (!animator.GetBool("IsAttacking")) { 
                spiderController.EnemyAttackPlayer(col.transform);
            }
        }   
    }
}
