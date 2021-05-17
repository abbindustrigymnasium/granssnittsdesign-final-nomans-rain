using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private int combo = 0;

    private float timeSinceAttacked;
    public float attackCooldown;

    private bool queuedHeavy;
    private bool queuedLight;

    private Rigidbody rb;
    private PlayerMovement pMove;
    private PlayerStats playerStats;

    public float currentHealth = 1;
    public float regenBase = 0.01f;
    HealthBar healthBarScript;

    private HitboxesPlayer hitbox1;
    private HitboxesPlayer hitbox2;
    private List<EnemyController> dyingEnemies = new List<EnemyController>();
    private Animator animator;

    void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
        pMove = transform.parent.GetComponent<PlayerMovement>();
        playerStats = transform.parent.GetComponent<PlayerStats>();
        healthBarScript = GameObject.Find("Health bar").GetComponent<HealthBar>();
        healthBarScript.SetMaxHealth(1f);
        hitbox1 = GameObject.Find("Hitbox1").GetComponent<HitboxesPlayer>();
        hitbox2 = GameObject.Find("Hitbox2").GetComponent<HitboxesPlayer>();
        animator = transform.parent.gameObject.GetComponent<Animator>();
    }

    private void Update()
    {
        float healthAfterRegen = currentHealth + regenBase * playerStats.HealthRegen * Time.deltaTime;
        currentHealth = (healthAfterRegen > playerStats.Health) ? playerStats.Health : healthAfterRegen;
        healthBarScript.SetMaxHealth(playerStats.Health);
        healthBarScript.SetHealth(currentHealth);
    }

    private void FixedUpdate()
    {
       attackCooldown -= Time.deltaTime;
       if (attackCooldown <=0f)
       {
            pMove.canMove = true;
            animator.SetInteger("Attack", 0);
       }
        if (attackCooldown <= -0.5f)
        {
                combo = 0;
        }
        if (queuedHeavy)
        {
            animator.SetInteger("Attack", 3);
            if (attackCooldown <= 0f && pMove.isGrounded)
            {
                queuedHeavy = false;
                pMove.canMove = false;
                attackCooldown = 1.5f / playerStats.AttackSpeed;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (attackCooldown <= 0f)
                {
                    animator.SetInteger("Attack", combo + 1);
                    if (combo == 0)
                    {
                        // light attack one
                        DamageEnemiesNormal();
                        attackCooldown = 0.5f / playerStats.AttackSpeed;
                        rb.velocity += transform.forward;
                        combo = 1;
                    }
                    else if (combo == 1)
                    {
                        // light attack two
                        DamageEnemiesCombo();

                        attackCooldown = 1f / playerStats.AttackSpeed;
                        rb.velocity = transform.forward;
                        combo = 0;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                if (attackCooldown <= 0.3f)
                {
                    queuedHeavy = true;
                    rb.velocity += transform.up * -3;
                    if (pMove.isJumping)
                    {
                        pMove.canMove = false;
                    }
                }
            }
        }
    }

    // simplify mesh for each pickup
    // each pickup should be fully lit
    private void DamageEnemiesNormal()
    {
        float flatDamage;
        float flatBleedDamage = playerStats.Damage * playerStats.BleedDamage;
        float flatVirusDamage = playerStats.Damage * playerStats.VirusDamage;
        bool getBleed;
        bool getVirus;
        bool getParalyze;
        bool getFrostbite;

        dyingEnemies.Clear();

        foreach (EnemyController enemyController in hitbox1.currentEnemiesInHitbox)
        {
            if (enemyController == null)
            {
                dyingEnemies.Add(enemyController);
                continue;
            }
            flatDamage = playerStats.Damage * ((Random.value < playerStats.CriticalChance) ? playerStats.CriticalDamage : 1f);

            float healthAfterLifeSteal = currentHealth + flatDamage * playerStats.LifeSteal;
            currentHealth = (healthAfterLifeSteal > playerStats.Health) ? playerStats.Health : healthAfterLifeSteal;

            getBleed = Random.value < playerStats.BleedChance;
            getVirus = Random.value < playerStats.VirusChance;
            getParalyze = Random.value < playerStats.Paralyze;
            getFrostbite = Random.value < playerStats.FrostBite;

            enemyController.Hit(flatDamage, flatBleedDamage, getBleed, flatVirusDamage, getVirus, getParalyze, getFrostbite);
        }
        foreach (EnemyController enemyController in dyingEnemies)
        {
            hitbox1.currentEnemiesInHitbox.Remove(enemyController);
        }
    }

    private void DamageEnemiesCombo()
    {
        float flatDamage;
        float flatBleedDamage = playerStats.Damage * playerStats.BleedDamage;
        float flatVirusDamage = playerStats.Damage * playerStats.VirusDamage;
        bool getBleed;
        bool getVirus;
        bool getParalyze;
        bool getFrostbite;

        dyingEnemies.Clear();

        foreach (EnemyController enemyController in hitbox2.currentEnemiesInHitbox)
        {
            if (enemyController == null)
            {
                dyingEnemies.Add(enemyController);
                continue;
            }
            flatDamage = playerStats.Damage * ((Random.value < playerStats.CriticalChance) ? playerStats.CriticalDamage : 1f);

            float healthAfterLifeSteal = currentHealth + flatDamage * playerStats.LifeSteal;
            currentHealth = (healthAfterLifeSteal > playerStats.Health) ? playerStats.Health : healthAfterLifeSteal;

            getBleed = Random.value < playerStats.BleedChance;
            getVirus = Random.value < playerStats.VirusChance;
            getParalyze = Random.value < playerStats.Paralyze;
            getFrostbite = Random.value < playerStats.FrostBite;

            enemyController.Hit(flatDamage, flatBleedDamage, getBleed, flatVirusDamage, getVirus, getParalyze, getFrostbite);
        }
        foreach (EnemyController enemyController in dyingEnemies)
        {
            hitbox2.currentEnemiesInHitbox.Remove(enemyController);
        }
    }

    private void DamageEnemiesHeavy()
    {

    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage * (1-playerStats.DamageReduction);
        Debug.Log(playerStats.DamageReduction);
        if (currentHealth <= 0f)
        {
            Debug.Log("de ä över fam");
        }
    }

    public void UpdateHitBox()
    {
        hitbox1.UpdateHitBox(playerStats.Range);
        hitbox2.UpdateHitBox(playerStats.Range);
    }
}
