using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour
{
    //to match damage output with animation
    public float attackDelay = 0.2f;

    private float attackSpeed;
    private float attackCooldown;

    public event System.Action OnAttack;


    CharacterStats myStats;

    [Header("Set only for Ranged Characters")]
    public bool isRanged = false;
    public GameObject projectilePrefab;
    public Transform firePoint;

    void Start()
    {
        myStats = GetComponent<CharacterStats>();
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    public void Attack(CharacterStats targetStats)
    {
        //Debug.Log("Attack(CharacterStats targetStats):");
            if (attackCooldown <= 0)
            {
                attackSpeed = (float)myStats.attackSpeed.GetValue();

                if (!isRanged)
                {
                    StartCoroutine(DoDamage(targetStats, attackDelay));
                }
                else
                {
                //shootProjectile(targetStats.transform);
                InstantiateAndReturnProjectile(targetStats.transform);
            }


                if (OnAttack != null)
                    OnAttack();

                attackCooldown = 1 / attackSpeed;
            }        
    }

    public void Attack(NetworkCharacterStats targetStats)
    {
        //Debug.Log("Attack(NetworkCharacterStats targetStats):");
        if (attackCooldown <= 0)
            {
                attackSpeed = (float)myStats.attackSpeed.GetValue();

                if (!isRanged)
                {
                    StartCoroutine(DoDamage(targetStats, attackDelay));
                }
                else
                {
                //shootProjectile(targetStats.transform);
                InstantiateAndReturnProjectile(targetStats.transform);
                }


                if (OnAttack != null)
                    OnAttack();

                attackCooldown = 1 / attackSpeed;
            }
    }

    IEnumerator DoDamage(CharacterStats stats, float delay)
    {
        yield return new WaitForSeconds(delay);
        stats.TakeDamage(myStats.damage.GetValue());
    }

    IEnumerator DoDamage(NetworkCharacterStats stats, float delay)
    {
        yield return new WaitForSeconds(delay);
        stats.TakeDamage(myStats.damage.GetValue());
    }

    private void shootProjectile(Transform target)
    {
        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetDamage(myStats.damage.GetValue());
            projectile.SetTarget(target);
        }
    }

    private  GameObject InstantiateAndReturnProjectile(Transform target)
    {
        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.SetDamage(myStats.damage.GetValue());
            projectile.SetTarget(target);
        }
        return projectileGO;
    }
}
