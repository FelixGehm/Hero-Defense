using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkCharacterStats))]
public class NetworkCharacterCombat : NetworkBehaviour
{

    [Header("Dieses Script ersetzt das CharacterCombat Script!")]
    [Header("")]


    //to match damage output with animation
    public float attackDelay = 0.2f;

    private float attackSpeed;
    private float attackCooldown;

    public event System.Action OnAttack;


    NetworkCharacterStats myNetworkStats;
    CharacterStats myCharacterStats;


    [Header("Set only for Ranged Characters")]
    public bool isRanged = false;
    public GameObject projectilePrefab;
    public Transform firePoint;

    void Start()
    {
        myNetworkStats = GetComponent<NetworkCharacterStats>();
        myCharacterStats = myNetworkStats.getStats();
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    public void Attack(NetworkCharacterStats targetStats)
    {
        //Debug.Log("Attack(NetworkCharacterStats targetStats):");
        if (attackCooldown <= 0)
        {
            attackSpeed = (float)myCharacterStats.attackSpeed.GetValue();

            if (!isRanged)
            {
                StartCoroutine(DoDamage(targetStats, attackDelay));
            }
            else
            {
                ShootProjectile(targetStats.transform);
            }

            if (OnAttack != null)
                OnAttack();

            attackCooldown = 1 / attackSpeed;
        }
    }

    IEnumerator DoDamage(NetworkCharacterStats stats, float delay)
    {
        yield return new WaitForSeconds(delay);
        stats.TakeDamage(myCharacterStats.damage.GetValue());
    }

    private void ShootProjectile(Transform target)
    {
        // Locales Projektil erzeugen und losschicken
        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

        if (projectile != null)
        {
            projectile.SetDamage(myCharacterStats.damage.GetValue());
            projectile.SetTarget(target);
        }

        // Projektil vom Server erzeugen lassen bzw. als Server selbst das Projektil für alle spawnen
        if (isServer)
        {
            NetworkServer.Spawn(projectileGO);
        }
        else
        {
            TransmitBullet(target, myCharacterStats.damage.GetValue());
        }
    }



    // Wird vom Server ausgeführt sobald das Cmd von einem Client aufgerufen wird
    [Command]
    void CmdSpawnBulletOnServer(NetworkInstanceId targetId, float damage)
    {
        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

        if (projectile != null)
        {
            projectile.SetDamage(myCharacterStats.damage.GetValue());
            projectile.SetTarget(targetId);
        }

        Debug.Log(projectileGO);
        NetworkServer.Spawn(projectileGO);
    }


    // Wird ausschließlich von Clients ausgeführt, nicht vom Host/Server
    [ClientCallback]
    void TransmitBullet(Transform target, float damage)
    {
        NetworkInstanceId id = target.gameObject.GetComponent<NetworkIdentity>().netId;

        Debug.Log(transform.name + " TransmitBullet(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        if (!isServer)
        {
            CmdSpawnBulletOnServer(id, damage);
        }
    }
}