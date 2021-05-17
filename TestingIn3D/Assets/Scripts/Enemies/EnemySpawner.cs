using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnRate;
    private float gameTime;

    [SerializeField]
    private float planetMaxRadius;
    [SerializeField]
    private float spawnDistFromPlayer;
    [SerializeField]
    private float spawnArea;
    [SerializeField]
    private float dmgC;
    [SerializeField]
    private float healthC;

    private GameObject enemyPrefab;
    private PlayerPickupSpawner playerPickupSpawner;
    private Transform playerTransform;
    private PlayerAttack playerAttack;

    private void FixedUpdate()
    {
        gameTime += Time.deltaTime;
    }

    public void StartSpawning()
    {
        // Deleta all enemies
        StopAllCoroutines();
        Delete();

        // Spawn all enemies
        if (spawnRate > 0)
        {
            //Spawn(enemyPrefab);
            StartCoroutine(SpawnPrefab(enemyPrefab, 60f / spawnRate));
        }
    }

    private IEnumerator SpawnPrefab(GameObject prefab, float interval)
    {
        while (true)
        {
            Spawn(prefab);

            yield return new WaitForSeconds(interval);
        }
    }

    private Vector3 RandomPointAroundPlayer() 
    {
        float rad = Random.value * 360 * Mathf.Deg2Rad;
        Vector3 position = new Vector3(spawnDistFromPlayer * Mathf.Cos(rad), 0, spawnDistFromPlayer * Mathf.Sin(rad));
        return position;
    }

    public void Spawn(GameObject prefab)
    {
        GameObject enemyPrefab = Instantiate(prefab) as GameObject;
        enemyPrefab.transform.parent = transform;

        // EEEW, works but EEEEEEW
        enemyPrefab.transform.position = RandomPointAroundPlayer();
        enemyPrefab.transform.position = Quaternion.FromToRotation(enemyPrefab.transform.up, playerTransform.up) * enemyPrefab.transform.rotation * enemyPrefab.transform.position;
        enemyPrefab.transform.position = (enemyPrefab.transform.position + playerTransform.position).normalized * (planetMaxRadius + spawnArea) + Random.insideUnitSphere * spawnArea;

        Transform enemyObject = enemyPrefab.transform.Find("Enemy");

        HitboxesEnemy hitbox = enemyObject.gameObject.AddComponent<HitboxesEnemy>();
        float damage = 0.1f + healthC*gameTime;
        float health = 10f + healthC*gameTime;
        EnemyController enemyController = enemyObject.GetComponent<EnemyController>();
        enemyController.Init(playerPickupSpawner, playerTransform, hitbox, playerAttack, damage, health);

        //Damageable damageable = enemyObject.GetComponent<Damageable>();
    }

    public void Delete()
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }

    private void Awake()
    {
        // enemyPrefab = Resources.Load("Prefabs/Enemies/EnemyGameObject") as GameObject;
        enemyPrefab = Resources.Load("Prefabs/Enemies/EnemySpider") as GameObject;
        playerPickupSpawner = GameObject.Find("Pickups").GetComponent<PlayerPickupSpawner>();
        playerTransform = GameObject.Find("Player").transform;
        playerAttack = playerTransform.Find("PlayerHands").GetComponent<PlayerAttack>();
    }
}
