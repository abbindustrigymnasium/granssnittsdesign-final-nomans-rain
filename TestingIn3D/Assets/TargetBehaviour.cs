using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    public GameObject player;
    private PlayerMovement pMove;
    // Start is called before the first frame update
    void Start()
    {
        pMove = player.GetComponent<PlayerMovement>();    
    }

    // Update is called once per frame
    void Update()
    {
      if (pMove.moveDir != Vector3.zero)
        {
            transform.localPosition = Vector3.Slerp(transform.localPosition, pMove.moveDir*2.3f, 0.1f);
        } 
    }
}
