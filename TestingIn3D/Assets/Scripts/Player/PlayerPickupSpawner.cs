using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupSpawner : MonoBehaviour
{
    private PlayerStats playerStats;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
    }

    private float Sum(float[] arr)
    {
        float sum = 0f;
        for (int i = 0; i < arr.Length; i++)
        {
            sum += arr[i];
        }
        return sum;
    }

    public void SpawnPickup(Vector3 spawnPos)
    {
        float randomValue = Random.Range(0f, Sum(playerStats.SpawnRateArray) + playerStats.pickupSpawnRate.Nothing);

        PlayerPickup.PickUpType pickUpType = PlayerPickup.PickUpType.None;

        float sum = 0f;
        for (int i = 0; i < playerStats.SpawnRateArray.Length; i++)
        {
            if (randomValue >= sum && randomValue <= sum + playerStats.SpawnRateArray[i])
            {
                pickUpType = (PlayerPickup.PickUpType) i+1;
            }
            sum += playerStats.SpawnRateArray[i];
        }

        if (pickUpType == PlayerPickup.PickUpType.None)
        {
            return;
        }

        GameObject instancedObj = Instantiate(Resources.Load<GameObject>("Prefabs/Pickup/" + pickUpType.ToString()));

        instancedObj.transform.position = spawnPos;
        instancedObj.transform.rotation = Quaternion.FromToRotation(Vector3.up, spawnPos.normalized);
        instancedObj.transform.parent = transform;
        instancedObj.AddComponent<PlayerPickup>();
        instancedObj.GetComponent<PlayerPickup>().InitPickup(playerStats, pickUpType);
    }
}
