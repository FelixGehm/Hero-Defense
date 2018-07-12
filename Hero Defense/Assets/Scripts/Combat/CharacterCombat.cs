using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : NetworkBehaviour
{
    public float attackDelay = 0.2f;    //to match damage output with animation

    protected float attackSpeed;
    protected float attackCooldown;

    [HideInInspector]
    public bool isAttacking; //Immer true von start der Animation bis zur eigentlichen attack (also nach dem delay), nicht über die gesamte animation!
    protected Coroutine attack;

    public event System.Action OnAttack;
    public event System.Action OnAttackCanceled;

    protected CharacterStats myStats;

    [HideInInspector]
    public bool isBlinded = false;

    [Header("Set only for Ranged Characters")]
    public bool isRanged = false;
    public GameObject projectilePrefab;
    public Transform firePoint;

    public virtual void Start()
    {
        myStats = GetComponent<CharacterStats>();
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;
    }



    public virtual void Attack(CharacterStats targetStats)
    {
        float damageDone = myStats.physicalDamage.GetValue();   //get normal Damage from Stats
        if (CheckForCrit())                                     //check if crit did happen
        {
            //Debug.Log("CRIT! from "+transform.name);
            damageDone = CalcCritDamage();                      //calc new Damage
        }

        if (attackCooldown <= 0)
        {
            attackSpeed = myStats.attackSpeed.GetValue();

            if (!isRanged)
            {
                isAttacking = true;
                attack = StartCoroutine(DoMeleeDamage(targetStats, damageDone, attackDelay * 1 / attackSpeed));
            }
            else
            {
                isAttacking = true;
                attack = StartCoroutine(ShootProjectile(targetStats.transform, damageDone, attackDelay * 1 / attackSpeed));
            }

            FireOnAttack();

            attackCooldown = 1.0f / attackSpeed;
            //Debug.Log("attack");
        }
    }

    protected void FireOnAttack()
    {
        OnAttack?.Invoke();
    }


    protected IEnumerator DoMeleeDamage(CharacterStats targetStats, float damageDone, float delay)        //TODO
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;

        if (isServer)
        {
            targetStats.TakePhysicalDamage(damageDone);
        }
        else
        {
            TellServerToDoMeleeDamage(targetStats, damageDone);
        }
    }



    protected virtual IEnumerator ShootProjectile(Transform target, float damageDone, float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;

        NetworkInstanceId idTarget = target.gameObject.GetComponent<NetworkIdentity>().netId;

        // Projektil vom Server erzeugen lassen bzw. als Server selbst das Projektil für alle spawnen
        if (isServer)
        {
            //SpawnBullet(target, damageDone);
            CmdSpawnBulletOnServer(idTarget, damageDone);
        }
        else
        {
            TellServerToSpawnBullet(idTarget, damageDone);
        }

    }

    public void CancelAttack()
    {
        if (attack != null)
            StopCoroutine(attack);

        OnAttackCanceled?.Invoke();

        isAttacking = false;
        attackCooldown = 0;
    }

    public float GetAttackCooldown()
    {
        return attackCooldown;
    }

    #region Crit
    protected bool CheckForCrit()
    {
        bool isCrit = false;

        float randomNumber = Random.Range(0.0f, 1.0f);
        float critChance = myStats.critChance.GetValue();

        if (critChance >= randomNumber)
        {
            isCrit = true;
        }
        return isCrit;
    }

    protected float CalcCritDamage()
    {
        float damage = myStats.physicalDamage.GetValue() * myStats.critDamage.GetValue();

        return damage;
    }

    #endregion

    public CharacterStats GetCharacterStats()
    {
        return myStats;
    }

    #region Network

    #region Fernkampf


    [Command]
    protected void CmdSpawnBulletOnServer(NetworkInstanceId targetId, float damage)
    {
        WorkAroundCmd(targetId, damage);
    }

    [Server]
    protected virtual void WorkAroundCmd(NetworkInstanceId targetId, float damage)
    {
        Debug.Log("CharacterCombat CmDSPawnBulletOnServer");
        Transform targetTransform = NetworkServer.FindLocalObject(targetId).transform;

        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

        if (projectile != null)
        {
            if (isBlinded)
            {
                projectile.InitBullet(targetTransform, 0);
            }
            else
            {
                projectile.InitBullet(targetTransform, damage);
            }
        }
        NetworkServer.Spawn(projectileGO);
    }


    [ClientCallback]
    protected virtual void TellServerToSpawnBullet(NetworkInstanceId id, float damage)
    {
        //Debug.Log(transform.name + " TransmitBullet(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        if (!isServer)
        {
            CmdSpawnBulletOnServer(id, damage);
        }
    }

    #endregion

    //Test
    public void TestKill()
    {
        StartCoroutine(DoMeleeDamage(myStats, 100000, 1));
    }
    //test end

    #region Melee

    /// <summary>
    /// Der Server sucht über die NetworkInstanceId die CharacterStats des Ziels des Angriffs und fügt ihm dann Schaden zu.
    /// </summary>
    /// <param name="targetId"></param>
    [Command]
    protected virtual void CmdDoMeleeDamageOnServer(NetworkInstanceId targetId, float damageDone)
    {
        CharacterStats targetStats = NetworkServer.FindLocalObject(targetId).GetComponent<CharacterStats>();

        if (isBlinded)
        {
            targetStats.TakePhysicalDamage(0.0f);
        }
        else
        {
            targetStats.TakePhysicalDamage(damageDone);
        }
    }

    /// <summary>
    /// Leitet aus den CharacterStats des Targets und der dazugehörigen NetworkIdentity die NetworkInstanceId her und übergibt diese dem Command.
    /// Commands können AUSSCHLIEßLICH primitve Datentypen + einige spezielle Netzwerkdatentypen übergeben bekommen. Deswegen dieser Umweg über die ID...
    /// </summary>
    /// <param name="targetStats"></param>
    [ClientCallback]
    protected void TellServerToDoMeleeDamage(CharacterStats targetStats, float damageDone)
    {
        NetworkInstanceId id = targetStats.transform.gameObject.GetComponent<NetworkIdentity>().netId;

        if (!isServer)
        {
            CmdDoMeleeDamageOnServer(id, damageDone);
        }
    }
    #endregion

    #endregion
}