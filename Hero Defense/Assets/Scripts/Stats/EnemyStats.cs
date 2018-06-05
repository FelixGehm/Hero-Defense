using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyStats : CharacterStats {

    EnemyController enemyController;

    public override void Awake()
    {
        base.Awake();

        enemyController = GetComponent<EnemyController>();

    }

    /*
     *  Wenn beim Schaden machen Aggro erhalten werden soll,
     *  dann eine der drei folgenden Methoden verwenden beim Schaden machen!
     */

    public void TakeTrueDamage(float tDamage, Transform damageCauser)
    {
        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        float damage = tDamage;
        damage = Mathf.Max(damage, 0);      // Check if damage is < 0, if yes -> set to 0

        CurrentHealth -= damage;

        enemyController.ReceivedDamageFrom(damageCauser);
    }
    
    public void TakePhysicalDamage(float pDamage, Transform damageCauser)
    {
        //Debug.Log("TakePhyDam");

        if (!isServer)      // Ausschließlich der Server verursacht Schaden.
        {
            return;
        }

        CurrentHealth -= CalcTakenPhysicalDamage(pDamage);
        enemyController.ReceivedDamageFrom(damageCauser);
    }

 
    public void TakeMagicDamage(float mDamage, Transform damageCauser)
    {
        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        CurrentHealth -= CalcTakenPhysicalDamage(mDamage);
        enemyController.ReceivedDamageFrom(damageCauser);
    }


    public override void Die()
    {
        base.Die();

        //death stuff
        NetworkServer.Destroy(gameObject);
    }
}