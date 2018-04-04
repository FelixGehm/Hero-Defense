using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : NetworkBehaviour
{
    public float attackDelay = 0.2f;    //to match damage output with animation

    private float attackSpeed;
    private float attackCooldown;

    public event System.Action OnAttack;    //Todo: Event nach CharacterEventController verlegen vielleicht ????(wegen Übersichtlichkeit)

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
        if (attackCooldown <= 0)
        {
            attackSpeed = (float)myStats.attackSpeed.GetValue();

            if (!isRanged)
            {
                StartCoroutine(DoMeleeDamage(targetStats, attackDelay));
            }
            else
            {
                StartCoroutine(ShootProjectile(targetStats.transform, attackDelay));
            }

            if (OnAttack != null)
                OnAttack();

            attackCooldown = 1 / attackSpeed;
        }
    }

    IEnumerator DoMeleeDamage(CharacterStats stats, float delay)        //TODO
    {
        yield return new WaitForSeconds(delay);
        stats.TakePhysicalDamage(myStats.physicalDamage.GetValue());
    }

    IEnumerator ShootProjectile(Transform target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isServer)   // Projektil vom Server erzeugen lassen bzw. als Server selbst das Projektil für alle spawnen
        {
            GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

            if (projectile != null)
            {
                projectile.SetDamage(myStats.physicalDamage.GetValue());
                projectile.SetTarget(target);
            }


//            Debug.Log("ShootProjectile before spawning projectile on server");
            NetworkServer.Spawn(projectileGO);
         //   Debug.Log("ShootProjectile after spawning projectile on server");
        }
        else
        {
            AskServerToSpawnBullet(target, myStats.physicalDamage.GetValue());
        }

    }

    #region Network
    /// <summary>
    /// Für eine (relativ) ausführliche Erklärung zu Command und ClientCallBack:
    ///     siehe CharacterEventManager
    /// </summary>

    [Command]
    void CmdSpawnBulletOnServer(NetworkInstanceId targetId, float damage)
    {
        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

        if (projectile != null)
        {
            projectile.SetDamage(myStats.physicalDamage.GetValue());
            projectile.SetTarget(targetId);
        }

        //Debug.Log(projectileGO);
        NetworkServer.Spawn(projectileGO);
    }


    [ClientCallback]
    void AskServerToSpawnBullet(Transform target, float damage)
    {
        NetworkInstanceId id = target.gameObject.GetComponent<NetworkIdentity>().netId;

        //Debug.Log(transform.name + " TransmitBullet(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        if (!isServer)
        {
            CmdSpawnBulletOnServer(id, damage);
        }
    }

    #endregion
}