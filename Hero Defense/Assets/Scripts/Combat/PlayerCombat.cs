using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCombat : CharacterCombat
{

    public virtual void Attack(EnemyStats targetStats)
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



    protected IEnumerator DoMeleeDamage(EnemyStats targetStats, float damageDone, float delay)        //TODO
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
        if (isServer)
        {
            targetStats.TakePhysicalDamage(damageDone, this.transform);
        }
        else
        {
            TellServerToDoMeleeDamage(targetStats, damageDone);
        }
    }

    protected void TellServerToDoMeleeDamage(EnemyStats targetStats, float damageDone)
    {
        NetworkInstanceId id = targetStats.transform.gameObject.GetComponent<NetworkIdentity>().netId;

        if (!isServer)
        {
            CmdDoMeleeDamageOnServer(id, damageDone);
        }
    }

    protected override void CmdDoMeleeDamageOnServer(NetworkInstanceId targetId, float damageDone)
    {
        EnemyStats targetStats = NetworkServer.FindLocalObject(targetId).GetComponent<EnemyStats>();

        if (isBlinded)
        {
            targetStats.TakePhysicalDamage(0.0f, this.transform);
        }
        else
        {
            targetStats.TakePhysicalDamage(damageDone, this.transform);
        }
    }

    protected override void SpawnBullet(Transform target, float damageDone)
    {
        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();

        if (projectile != null)
        {
            if (isBlinded)
            {
                projectile.InitBullet(target, 0, this.transform);
            }
            else
            {
                projectile.InitBullet(target, damageDone, this.transform);
            }
        }
        NetworkServer.Spawn(projectileGO);
    }
}
