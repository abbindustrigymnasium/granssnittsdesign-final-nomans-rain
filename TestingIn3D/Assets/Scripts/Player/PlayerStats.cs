using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// add function for each ability that returns damage amount

public class PlayerStats : MonoBehaviour
{
    private PlayerAttack playerAttack;

    private float attackSpeedAmount;
    private float attackSpeed;
    [HideInInspector]
    public float AttackSpeed
    {
        get { return attackSpeed; }
        set {
            attackSpeedAmount++;
            attackSpeed = initStats.AttackSpeed + attackSpeedAmount * statsConstants.AttackSpeed;
        }
    }

    private float damageAmount;
    private float damage;
    [HideInInspector]
    public float Damage
    {
        get { return damage; }
        set
        {
            damageAmount++;
            damage = initStats.Damage + statsConstants.Damage * damageAmount;
        }
    }

    private float damageReductionAmount;
    private float damageReduction;
    [HideInInspector]
    public float DamageReduction
    {
        get { return damageReduction; }
        set
        {
            damageReductionAmount++;
            damageReduction = 1f - 1f / (1f + statsConstants.DamageReduction * damageReductionAmount);
        }
    }

    private float criticalChanceAmount;
    private float criticalChance;
    [HideInInspector]
    public float CriticalChance
    {
        get { return criticalChance; }
        set
        {
            criticalChanceAmount++;
            criticalChance = Mathf.Clamp01(criticalChanceAmount * statsConstants.CriticalChance);
        }
    }

    private float criticalDamageAmount;
    private float criticalDamage;
    [HideInInspector]
    public float CriticalDamage
    {
        get { return criticalDamage; }
        set
        {
            criticalDamageAmount++;
            criticalDamage = initStats.CriticalDamage + criticalDamageAmount * statsConstants.CriticalDamage;
        }
    }

    private float virusAmount;
    private float virusDamage;
    private float virusChance;
    [HideInInspector]
    public float VirusDamage
    {
        get { return virusDamage; }
        set
        {
            virusAmount++;
            virusDamage = statsConstants.VirusDamage * virusAmount;
        }
    }
    [HideInInspector]
    public float VirusChance
    {
        get { return virusChance; }
        set
        {
            virusChance = Mathf.Clamp01(statsConstants.VirusChance * virusAmount);
        }
    }

    private float bleedAmount;
    private float bleedDamage;
    private float bleedChance;
    [HideInInspector]
    public float BleedDamage
    {
        get { return bleedDamage; }
        set
        {
            bleedAmount++;
            bleedDamage = statsConstants.BleedDamage * bleedAmount;
        }
    }
    [HideInInspector]
    public float BleedChance
    {
        get { return bleedChance; }
        set
        {
            bleedChance = Mathf.Clamp01(statsConstants.BleedChance * bleedAmount);
        }
    }

    private float frostBiteAmount;
    private float frostBite;
    [HideInInspector]
    public float FrostBite
    {
        get { return frostBite; }
        set
        {
            frostBiteAmount++;
            frostBite = Mathf.Clamp01(statsConstants.FrostBite * frostBiteAmount);
        }
    }

    private float paralyzeAmount;
    private float paralyze;
    [HideInInspector]
    public float Paralyze
    {
        get { return paralyze; }
        set
        {
            paralyzeAmount++;
            paralyze = Mathf.Clamp01(statsConstants.Paralyze * paralyzeAmount);
        }
    }

    private float movementspeedAmount;
    private float movementspeed;
    [HideInInspector]
    public float Movementspeed
    {
        get { return movementspeed; }
        set
        {
            movementspeedAmount++;
            movementspeed = statsConstants.Movementspeed * movementspeedAmount + initStats.Movementspeed;
        }
    }

    private float healthAmount;
    private float health;
    [HideInInspector]
    public float Health
    {
        get { return health; }
        set
        {
            healthAmount++;
            health = statsConstants.Health * healthAmount + initStats.Health;
        }
    }

    private float healthRegenAmount;
    private float healthRegen;
    [HideInInspector]
    public float HealthRegen
    {
        get { return healthRegen; }
        set
        {
            healthRegenAmount++;
            healthRegen = statsConstants.HealthRegen * healthRegenAmount + initStats.HealthRegen;
        }
    }

    private float lifeStealAmount;
    private float lifeSteal;
    [HideInInspector]
    public float LifeSteal
    {
        get { return lifeSteal; }
        set
        {
            lifeStealAmount++;
            lifeSteal = statsConstants.LifeSteal * lifeStealAmount;
        }
    }

    private float rangeAmount;
    private float range;
    [HideInInspector]
    public float Range
    {
        get { return range; }
        set
        {
            rangeAmount++;
            range = statsConstants.Range * rangeAmount + initStats.Range;
            playerAttack.UpdateHitBox();
        }
    }

    private float dashCooldownAmount;
    private float dashCooldown;
    [HideInInspector]
    public float DashCooldown
    {
        get { return dashCooldown; }
        set
        {
            dashCooldownAmount++;
            dashCooldown = 1f / (1f + statsConstants.DashCooldown * dashCooldownAmount);
        }
    }

    private float jumpHeightAmount;
    private float jumpHeight;
    [HideInInspector]
    public float JumpHeight
    {
        get { return jumpHeight; }
        set
        {
            jumpHeightAmount++;
            jumpHeight = statsConstants.JumpHeight * jumpHeightAmount + initStats.JumpHeight;
        }
    }

    [HideInInspector]
    public float[] SpawnRateArray;

    public PickupSpawnRate pickupSpawnRate;
    [System.Serializable]
    public class PickupSpawnRate
    {
        public float Nothing;
        public float AttackSpeed;
        public float Damage;
        public float DamageReduction;
        public float CriticalChance;
        public float CriticalDamage;
        public float Virus;
        public float Bleed;
        public float FrostBite;
        public float Paralyze;
        public float Movementspeed;
        public float Health;
        public float HealthRegen;
        public float LifeSteal;
        public float Range;
        public float DashCooldown;
        public float JumpHeight;
    }

    [SerializeField]
    private StatsConstants statsConstants;
    [System.Serializable]
    private class StatsConstants
    {
        public float AttackSpeed = 0.1f;
        public float Damage = 0.075f;
        public float DamageReduction = 0.2f;
        public float CriticalChance = 0.1f;
        public float CriticalDamage = 0.025f;
        public float VirusDamage = 0.02f;
        public float VirusChance = 0.1f;
        public float BleedDamage = 0.04f;
        public float BleedChance = 0.15f;
        public float FrostBite = 0.15f;
        public float Paralyze = 0.5f;
        public float Movementspeed = 0.1f;
        public float Health = 0.1f;
        public float HealthRegen = 0.075f;
        public float LifeSteal = 0.05f;
        public float Range = 0.02f;
        public float DashCooldown = 0.1f;
        public float JumpHeight = 0.1f;
    }

    [SerializeField]
    private InitStats initStats;
    [System.Serializable]
    private class InitStats
    {
        public float AttackSpeed = 1f;
        public float Damage = 1f;
        public float DamageReduction = 0f;
        public float CriticalChance = 0f;
        public float CriticalDamage = 2f;
        public float VirusDamage = 0f;
        public float VirusChance = 0f;
        public float BleedDamage = 0f;
        public float BleedChance = 0f;
        public float FrostBite = 0f;
        public float Paralyze = 0f;
        public float Movementspeed = 1f;
        public float Health = 1f;
        public float HealthRegen = 1f;
        public float LifeSteal = 0f;
        public float Range = 1f;
        public float DashCooldown = 1f;
        public float JumpHeight = 1f;
    }

    void Start()
    {
        playerAttack = GameObject.Find("PlayerHands").GetComponent<PlayerAttack>();

        // init values
        attackSpeed = initStats.AttackSpeed;
        damage = initStats.Damage;
        damageReduction = initStats.DamageReduction;
        criticalChance = initStats.CriticalChance;
        criticalDamage = initStats.CriticalDamage;
        virusDamage = initStats.VirusDamage;
        virusChance = initStats.VirusChance;
        bleedDamage = initStats.BleedDamage;
        bleedChance = initStats.BleedChance;
        frostBite = initStats.FrostBite;
        frostBite = initStats.Paralyze;
        movementspeed = initStats.Movementspeed;
        health = initStats.Health;
        healthRegen = initStats.HealthRegen;
        lifeSteal = initStats.LifeSteal;
        range = initStats.Range;
        dashCooldown = initStats.DashCooldown;
        jumpHeight = initStats.JumpHeight;

        // init spawnRate array
        SpawnRateArray = new float[16];
        SpawnRateArray[0] = pickupSpawnRate.AttackSpeed;
        SpawnRateArray[1] = pickupSpawnRate.Damage;
        SpawnRateArray[2] = pickupSpawnRate.DamageReduction;
        SpawnRateArray[3] = pickupSpawnRate.CriticalChance;
        SpawnRateArray[4] = pickupSpawnRate.CriticalDamage;
        SpawnRateArray[5] = pickupSpawnRate.Virus;
        SpawnRateArray[6] = pickupSpawnRate.Bleed;
        SpawnRateArray[7] = pickupSpawnRate.FrostBite;
        SpawnRateArray[8] = pickupSpawnRate.Paralyze;
        SpawnRateArray[9] = pickupSpawnRate.Movementspeed;
        SpawnRateArray[10] = pickupSpawnRate.Health;
        SpawnRateArray[11] = pickupSpawnRate.HealthRegen;
        SpawnRateArray[12] = pickupSpawnRate.LifeSteal;
        SpawnRateArray[13] = pickupSpawnRate.Range;
        SpawnRateArray[14] = pickupSpawnRate.DashCooldown;
        SpawnRateArray[15] = pickupSpawnRate.JumpHeight;
    }
}
