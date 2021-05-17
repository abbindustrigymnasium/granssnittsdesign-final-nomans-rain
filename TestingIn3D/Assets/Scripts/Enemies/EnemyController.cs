using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float gravityConstForEnemy = -12f;
    [SerializeField]
    private float standingUprightSlerpSpeed = 50f;

    [Header("Wander")]
    [SerializeField]
    private float wandererStrength = 1f;
    [SerializeField]
    private float walkSpeed = 2f;
    [SerializeField]
    private float runSpeed = 5f;
    [SerializeField]
    private float steerStrength = 2f;
    [SerializeField]
    private float standingLookSlerpSpeed = 50f;
    [SerializeField]
    private float viewSqrMagnitude = 2f;

    private Vector3 desiredDirection;
    private Vector3 desiredVelocity;
    private Vector3 desiredSteeringForce;
    private Vector3 acceleration;

    [SerializeField]
    public float health;
    public float damage;

    private PlayerPickupSpawner playerPickupSpawner;
    [SerializeField]
    private Transform playerTransform;

    private float timeFrostbitten = 0f;
    private float timeParalyzed = 0f;
    private float frostbitten;

    private float currentSpeed;

    private Coroutine infectRoutine;
    private Coroutine bleedRoutine;

    private bool dying;

    private HitboxesEnemy hitbox;
    private PlayerAttack playerAttack;
    

    public Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void Init(PlayerPickupSpawner playerPickupSpawner, Transform playerTransform, HitboxesEnemy hitbox, PlayerAttack playerAttack, float damage, float health)
    {
        this.playerPickupSpawner = playerPickupSpawner;
        this.playerTransform = playerTransform;
        this.hitbox = hitbox;
        this.playerAttack = playerAttack;
        this.damage = damage;
        this.health = health;
    }

    private bool shouldDoDamage = false;
    private void Update()
    {
        /*
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("EnemyAttack"))
        {
            return;
        }
        */
        Rotation();
        timeParalyzed -= Time.deltaTime;
        timeFrostbitten -= Time.deltaTime;
        frostbitten = (timeFrostbitten <= 0f) ? 1f : 0.5f; // slow amount is now 0.5f;

        if (hitbox.playerInHitbox && animator.GetCurrentAnimatorStateInfo(0).IsName("spideranimation") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
        {
            if (!shouldDoDamage) {
                Debug.Log("damaged player for " + damage + " damage");
                playerAttack.TakeDamage(damage);
            }
            shouldDoDamage = true;
            return;
        }
        else if (shouldDoDamage && !animator.GetCurrentAnimatorStateInfo(0).IsName("spideranimation"))
        {
            shouldDoDamage = false;
        }
        if (timeParalyzed <= 0f)
        {
            if ((playerTransform.position - transform.position).sqrMagnitude > viewSqrMagnitude)
            {
                EnemyWander();
            }
            else
            {
                EnemyGoPlayer();
            }

            EnemyWalkToPoint();

            //EnemyAttackPlayer();
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    public void Rotation()
    {
        Vector3 gravityUp = (transform.position).normalized;
        Vector3 localUp = transform.up;

        rb.AddForce(gravityUp * gravityConstForEnemy);

        // fix transform tangent to sphere
        Quaternion standingUpright = Quaternion.FromToRotation(localUp, gravityUp) * transform.rotation;
        standingUpright = Quaternion.Slerp(transform.rotation, standingUpright, standingUprightSlerpSpeed * Time.deltaTime);
        //transform.rotation = standingUpright;
        //return;


        // rotate transform forward to rb.velocity
        // translate rb.velocity to standingUpright
        Quaternion lookToWalkDir = Quaternion.FromToRotation(standingUpright * Vector3.forward, rb.velocity) * standingUpright;
        //Debug.Log(standingUpright);
        //Debug.Log(lookToWalkDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookToWalkDir, standingLookSlerpSpeed * Time.deltaTime);
    }

    public void EnemyWalkToPoint()
    {
        desiredVelocity = desiredDirection * currentSpeed * frostbitten;
        desiredSteeringForce = (desiredVelocity - rb.velocity) * steerStrength;
        acceleration = Vector3.ClampMagnitude(desiredSteeringForce, steerStrength);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity + acceleration * Time.deltaTime, currentSpeed * frostbitten);
    }

    public void EnemyWander()
    {
        desiredDirection = (desiredDirection + Random.insideUnitSphere * wandererStrength).normalized;
        currentSpeed = walkSpeed;
    }

    public void EnemyGoPlayer()
    {
        desiredDirection = (playerTransform.position - transform.position).normalized;
        currentSpeed = runSpeed;
    }

    public void EnemyAttackPlayer(Transform player)
    {
        StartCoroutine(EnemyDamagePlayer(player));
    }

    private IEnumerator EnemyDamagePlayer(Transform player)
    {
        //Debug.Log("started animation");
        //animator.Play("EnemyAttack", 0);
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length/2f);

        animator.SetBool("IsAttacking", true);
        //animator.SetFloat("Motion", 0.0f);
        yield return new WaitForSeconds(1);
        rb.velocity = (player.position - gameObject.transform.position).normalized * 2;
        animator.SetBool("IsAttacking", false);


    }

    // onhit
    public void Hit(float flatDamage, float bleedDamage, bool getBleed, float infectDamage, bool getVirus, bool getParalyze, bool getFrostbite)
    {
        DamageEnemy(flatDamage);
        Bleed(bleedDamage, getBleed);
        Infect(infectDamage, getVirus);
        Paralyze(getParalyze);
        Frostbite(getFrostbite);

        if (dying)
        {
            playerPickupSpawner.SpawnPickup(transform.position);
            Destroy(transform.parent.gameObject);
        }
    }

    public void DamageEnemy(float damage)
    {
        health -= damage;
        if (health <= 0f)
        {
            dying = true;
        }
    }

    public void Infect(float damage, bool invoke)
    {
        if (!invoke)
        {
            return;
        }
        infectRoutine = StartCoroutine(InvokeMethod(() => DamageEnemy(damage), 0.25f, 7));
    }
    public void Bleed(float damage, bool invoke)
    {
        if (!invoke)
        {
            return;
        }
        bleedRoutine = StartCoroutine(InvokeMethod(() => DamageEnemy(damage), 0.25f, 3));
    }
    public void Frostbite(bool invoke)
    {
        if (!invoke)
        {
            return;
        }
        timeFrostbitten = 1f; // time to be frostbitten is now 1f
    }
    public void Paralyze(bool invoke)
    {
        if (!invoke)
        {
            return;
        }
        timeParalyzed = 0.5f; // time to be paralyzed is now 0.5f
    }

    public IEnumerator InvokeMethod(System.Action method, float interval, int invokeCount)
    {
        for (int i = 0; i < invokeCount; i++)
        {
            method();

            yield return new WaitForSeconds(interval);
        }
    }
}
