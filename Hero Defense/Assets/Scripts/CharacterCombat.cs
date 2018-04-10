using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : NetworkBehaviour
{
    public float attackDelay = 0.2f;    //to match damage output with animation

    private float attackSpeed;
    private float attackCooldown;

    [HideInInspector]
    public bool isAttacking; //Immer true von start der Animation bis zur eigentlichen attack (also nach dem delay), nicht über die gesamte animation!
    private Coroutine attack;

    public event System.Action OnAttack;    //Todo: Event nach CharacterEventController verlegen vielleicht ????(wegen Übersichtlichkeit)  //ich finde das sollte hier bleiben, weil nur combatbezogen (Felix)
    public event System.Action OnAttackCanceled;

    CharacterStats myStats;

    [HideInInspector]
    public bool isBlinded = false;

    [Header("Set only for Ranged Characters")]
    public bool isRanged = false;
    public GameObject projectilePrefab;
    public Transform firePoint;

    void Start()
    {
        myStats = GetComponent<CharacterStats>();

        //attackDelay = 1.0f/ myStats.attackSpeed.GetValue();

    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    /// <summary>
    /// Wird genutzt von dem EnemyController um an die Stats ranzukommen und weitere abhängigkeiten zu vermeiden.
    /// </summary>
    /// <returns></returns>
    public CharacterStats GetCharacterStats()
    {
        return myStats;
    }

    public void Attack(CharacterStats targetStats)
    {
        if (attackCooldown <= 0)
        {
            attackSpeed = myStats.attackSpeed.GetValue();

            if (!isRanged)
            {
                isAttacking = true;
                attack = StartCoroutine(DoMeleeDamage(targetStats, attackDelay));
            }
            else
            {
                isAttacking = true;
                attack = StartCoroutine(ShootProjectile(targetStats.transform, attackDelay));
            }

            if (OnAttack != null)
                OnAttack();

            attackCooldown = 1.0f / attackSpeed;
        }
    }

    IEnumerator DoMeleeDamage(CharacterStats stats, float delay)        //TODO
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        stats.TakePhysicalDamage(myStats.physicalDamage.GetValue());
    }

    IEnumerator ShootProjectile(Transform target, float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        if (isServer)   // Projektil vom Server erzeugen lassen bzw. als Server selbst das Projektil für alle spawnen
        {
            GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

            if (projectile != null)
            {
                if(isBlinded)
                {
                    projectile.SetDamage(0);
                } else
                {
                    projectile.SetDamage(myStats.physicalDamage.GetValue());
                }
                
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

    public void CancelAttack()
    {
        if (attack != null)
            StopCoroutine(attack);

        if (OnAttackCanceled != null)
            OnAttackCanceled();
    }

    public float GetAttackCooldown()
    {
        return attackCooldown;
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
            if (isBlinded)
            {
                projectile.SetDamage(0);
            }
            else
            {
                projectile.SetDamage(myStats.physicalDamage.GetValue());
            }
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