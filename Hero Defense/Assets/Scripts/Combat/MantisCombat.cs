using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Shoots 2 Bullets instead of one 
public class MantisCombat : CharacterCombat
{
    public Transform secondFirePoint;
    public float delayToSecondShot = 0.2f;
    



    public override void Attack(CharacterStats targetStats)
    {
        Debug.Log("test");
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

    protected IEnumerator ShootTwoProjectiles(Transform target, float damageDone, float delay, float timeBetweenProjectiles)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;

        NetworkInstanceId idTarget = target.gameObject.GetComponent<NetworkIdentity>().netId;
        if (isServer)
        {
            //CmdSpawnBulletOnServer(idTarget, damageDone);
            CmdSpawnBulletOnServer(idTarget, damageDone, firePoint.position);
        }
        else
        {
            TellServerToSpawnBullet(idTarget, damageDone, firePoint.position);
        }

        yield return new WaitForSeconds(timeBetweenProjectiles);

        if (isServer)
        {
            //SpawnBullet(target, damageDone);
            CmdSpawnBulletOnServer(idTarget, damageDone, secondFirePoint.position);
        }
        else
        {
            TellServerToSpawnBullet(idTarget, damageDone, secondFirePoint.position);
        }
    }



    [Command]
    protected void CmdSpawnBulletOnServer(NetworkInstanceId targetId, float damage, Vector3 spawnPosition)
    {
        WorkAroundCmd(targetId, damage, spawnPosition);
    }

    [Server]
    protected void WorkAroundCmd(NetworkInstanceId targetId, float damage, Vector3 spawnPosition)
    {
        Debug.Log("CharacterCombat CmDSPawnBulletOnServer");
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


    [ClientCallback]
    protected void TellServerToSpawnBullet(NetworkInstanceId id, float damage, Vector3 spawnPosition)
    {
        //Debug.Log(transform.name + " TransmitBullet(): isServer = " + isServer + " hasAuthority = " + hasAuthority);
        if (!isServer)
        {
            CmdSpawnBulletOnServer(id, damage, spawnPosition);
        }
    }
}
