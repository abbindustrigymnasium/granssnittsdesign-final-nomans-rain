using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public float damage;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.GetComponent<Damageable>() != null)
        {
            Debug.Log("dmg");
            col.gameObject.GetComponent<Damageable>().health -= damage;
        }
    }
}
