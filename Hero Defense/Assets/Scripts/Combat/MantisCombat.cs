using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Shoots 2 Bullets instead of one 
public class MantisCombat : CharacterCombat
{
    public Transform secondFirePoint;
    [Header("Attack Settings")]
    public float delayToSecondShot = 0.2f;

    [HideInInspector]
    public bool isFiringMortar = false;
    private AbilityMortar mortar;

    [Header("Mortar Settings (set only if AbilityMortar is attached)")]
    public Transform firstMortarPoint;
    public Transform secondMortarPoint;
    public float mortarDelay = 0.3f;
    public float mortarDelayToSecondShot = 0.3f;
    public float mortarDelayToNextAA = 2;
    public float mortarCooldown = 5;
    private float currentMortarCooldown = 0;

    private EnemyAnimator anim;

    public event System.Action OnLeftShotFired;
    public event System.Action OnRightShotFired;
    public event System.Action OnLeftMortarFired;
    public event System.Action OnRightMortarFired;

    public override void Start()
    {
        base.Start();
        mortar = GetComponent<AbilityMortar>();
        anim = GetComponent<EnemyAnimator>();
    }

    protected override void Update()
    {
        base.Update();
        currentMortarCooldown -= Time.deltaTime;
    }

    public override void Attack(CharacterStats targetStats)
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

            isAttacking = true;
            attack = StartCoroutine(ShootTwoProjectiles(targetStats.transform, damageDone, attackDelay * 1 / attackSpeed, delayToSecondShot));


            FireOnAttack();

            attackCooldown = 1.0f / attackSpeed;
            //Debug.Log("attack");
        }
    }

    public void DoubleMortarAttack(Vector3 targetPosition)
    {
        if (currentMortarCooldown <= 0)
        {
            StartCoroutine(ShootMortarTwice(targetPosition, mortarDelay, mortarDelayToSecondShot));
            //mortar.Fire(firePoint.position, targetPosition);
            currentMortarCooldown = mortarCooldown;
        }
    }

    protected IEnumerator ShootTwoProjectiles(Transform target, float damageDone, float delay, float timeBetweenProjectiles)
    {
        isAttacking = true;
        yield return new WaitForSeconds(delay);
        if (OnRightShotFired != null) OnRightShotFired();


        NetworkInstanceId idTarget = target.gameObject.GetComponent<NetworkIdentity>().netId;
        if (isServer)
        {
            //CmdSpawnBulletOnServer(idTarget, damageDone);
            CmdSpawnBulletOnServer(idTarget, damageDone, firePoint.position);
        }
        else
        {
            //TellServerToSpawnBullet(idTarget, damageDone, firePoint.position);
        }

        yield return new WaitForSeconds(timeBetweenProjectiles);
        if (OnLeftShotFired != null) OnLeftShotFired();
        if (isServer)
        {
            //SpawnBullet(target, damageDone);
            CmdSpawnBulletOnServer(idTarget, damageDone, secondFirePoint.position);
        }
        else
        {
            //TellServerToSpawnBullet(idTarget, damageDone, secondFirePoint.position);
        }
        isAttacking = false;
    }

    public bool IsMortarReady
    {
        get
        {
            return currentMortarCooldown <= 0;
        }
    }

    protected IEnumerator ShootMortarTwice(Vector3 targetPosition, float delay, float timeBetweenProjectiles)
    {
        mortar.SpawnPreview(targetPosition);
        anim.StartMortarAnimation();
        isFiringMortar = true;
        yield return new WaitForSeconds(delay);
        if (OnLeftMortarFired != null) OnLeftMortarFired();
        mortar.Fire(firstMortarPoint.position, targetPosition);


        yield return new WaitForSeconds(timeBetweenProjectiles);
        if (OnRightMortarFired != null) OnRightMortarFired();
        mortar.Fire(secondMortarPoint.position, targetPosition);
        yield return new WaitForSeconds(mortarDelayToNextAA);
        mortar.DestroyPreview();
        isFiringMortar = false;
    }

    #region Network Auto Attack
    [Command]
    protected void CmdSpawnBulletOnServer(NetworkInstanceId targetId, float damage, Vector3 spawnPosition)
    {
        WorkAroundCmd(targetId, damage, spawnPosition);
    }

    [Server]
    protected void WorkAroundCmd(NetworkInstanceId targetId, float damage, Vector3 spawnPosition)
    {
        //Debug.Log("CharacterCombat CmDSPawnBulletOnServer");
        Transform targetTransform = NetworkServer.FindLocalObject(targetId).transform;

        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, spawnPosition, firePoint.rotation);
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

    /*
    [ClientCallback]
    protected void TellServerToSpawnBullet(NetworkInstanceId id, float damage, Vector3 spawnPosition)
    {
        //Debug.Log(transform.name + " TransmitBullet(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        if (!isServer)
        {
            CmdSpawnBulletOnServer(id, damage, spawnPosition);
        }
    }
    */
    #endregion
}
