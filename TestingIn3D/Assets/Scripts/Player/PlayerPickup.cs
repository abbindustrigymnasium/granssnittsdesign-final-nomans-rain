using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [SerializeField]
    private PickUpType pickUpType;
    public enum PickUpType { None, AttackSpeed, Damage, DamageReduction, CriticalChance, CriticalDamage, Virus, Bleed, FrostBite, Paralyze, Movementspeed, Health, HealthRegen, LifeSteal, Range, DashCooldown, JumpHeight };

    private PlayerStats playerStats;
    public void InitPickup(PlayerStats playerStats, PickUpType pickUpType)
    {
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        gameObject.AddComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
        gameObject.GetComponent<BoxCollider>().center = new Vector3(0, 0.5f, 0);
        this.pickUpType = pickUpType;
        this.playerStats = playerStats;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            switch (pickUpType)
            {
                case PickUpType.AttackSpeed:
                    playerStats.AttackSpeed = 0;
                    break;
                case PickUpType.Damage:
                    playerStats.Damage = 0;
                    break;
                case PickUpType.DamageReduction:
                    playerStats.DamageReduction = 0;
                    break;
                case PickUpType.CriticalChance:
                    playerStats.CriticalChance = 0;
                    break;
                case PickUpType.CriticalDamage:
                    playerStats.CriticalDamage = 0;
                    break;
                case PickUpType.Virus:
                    playerStats.VirusDamage = 0;
                    playerStats.VirusChance = 0;
                    break;
                case PickUpType.Bleed:
                    playerStats.BleedDamage = 0;
                    playerStats.BleedChance = 0;
                    break;
                case PickUpType.FrostBite:
                    playerStats.FrostBite = 0;
                    break;
                case PickUpType.Paralyze:
                    playerStats.Paralyze = 0;
                    break;
                case PickUpType.Movementspeed:
                    playerStats.Movementspeed = 0;
                    break;
                case PickUpType.Health:
                    playerStats.Health = 0;
                    break;
                case PickUpType.HealthRegen:
                    playerStats.HealthRegen = 0;
                    break;
                case PickUpType.LifeSteal:
                    playerStats.LifeSteal = 0;
                    break;
                case PickUpType.Range:
                    playerStats.Range = 0;
                    break;
                case PickUpType.DashCooldown:
                    playerStats.DashCooldown = 0;
                    break;
                case PickUpType.JumpHeight:
                    playerStats.JumpHeight = 0;
                    break;
            }
            gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(Picked(0.5f));
        }
    }
    IEnumerator Picked(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}