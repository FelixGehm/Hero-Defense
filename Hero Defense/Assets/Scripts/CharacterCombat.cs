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



    public void Attack(CharacterStats targetStats)
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

            if (OnAttack != null)
                OnAttack();

            attackCooldown = 1.0f / attackSpeed;
            //Debug.Log("attack");
        }
    }

    IEnumerator DoMeleeDamage(CharacterStats targetStats, float damageDone, float delay)        //TODO
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

    IEnumerator ShootProjectile(Transform target, float damageDone, float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;

        if (isServer)   // Projektil vom Server erzeugen lassen bzw. als Server selbst das Projektil für alle spawnen
        {
            SpawnBullet(target, damageDone);
        }
        else
        {
            TellServerToSpawnBullet(target, damageDone);
        }

    }

    private void SpawnBullet(Transform target, float damageDone)
    {
        //Debug.Log("SpawnBullet(): taget="+ target+", damage ="+damageDone);

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

        if (projectile != null)
        {
            if (isBlinded)
            {
                projectile.InitBullet(target, 0);
            }
            else
            {
                projectile.InitBullet(target, damageDone);
            }
        }
        NetworkServer.Spawn(projectileGO);
    }

    public void CancelAttack()
    {
        Debug.Log("CancelAttack()");
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
    private bool CheckForCrit()
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

    private float CalcCritDamage()
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
    /// <summary>
    /// Für eine (relativ) ausführliche Erklärung zu Command und ClientCallBack:
    /// siehe CharacterEventManager
    /// </summary>

    #region Fernkampf
    [Command]
    void CmdSpawnBulletOnServer(NetworkInstanceId targetId, float damage)
    {
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



            //projectile.SetTarget(transform);
        }

        //Debug.Log(projectileGO);
        NetworkServer.Spawn(projectileGO);
    }

    [ClientCallback]
    void TellServerToSpawnBullet(Transform target, float damage)
    {
        NetworkInstanceId id = target.gameObject.GetComponent<NetworkIdentity>().netId;


        //Debug.Log(transform.name + " TransmitBullet(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        if (!isServer)
        {
            CmdSpawnBulletOnServer(id, damage);               // HIER TAUCHT DIE WARNUNG AUF. ICH GLAUBE ALLES FUNKTIONIERT SO WIE ES SOLL... 
                                                              // ABER DIE WARNUNG NERVT!! UND ICH WEIß NICHT WIE ICH DIE LOS WERDEN KANN :(
                                                              //Debug.Log("CmdSpawnBulletOnServer()");
        }
    }
    #endregion

    #region Melee

    /// <summary>
    /// Der Server sucht über die NetworkInstanceId die CharacterStats des Ziels des Angriffs und fügt ihm dann Schaden zu.
    /// </summary>
    /// <param name="targetId"></param>
    [Command]
    void CmdDoMeleeDamageOnServer(NetworkInstanceId targetId, float damageDone)
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
    void TellServerToDoMeleeDamage(CharacterStats targetStats, float damageDone)
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