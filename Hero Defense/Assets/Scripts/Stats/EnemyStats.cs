using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyStats : CharacterStats {

    EnemyController enemyController;

    public int cashValue = 20;
    public int partsValue = 0;
    public Inventory inventory;

    public override void Awake()
    {
        base.Awake();

        enemyController = GetComponent<EnemyController>();

        inventory = GameObject.Find("_Game").GetComponent<Inventory>();

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

        SyncedCurrentHealth -= damage;

        enemyController.ReceivedDamageFrom(damageCauser);
    }
    
    public void TakePhysicalDamage(float pDamage, Transform damageCauser)
    {
        //Debug.Log("TakePhyDam");

        if (!isServer)      // Ausschließlich der Server verursacht Schaden.
        {
            return;
        }

        SyncedCurrentHealth -= CalcTakenPhysicalDamage(pDamage);
        enemyController.ReceivedDamageFrom(damageCauser);
    }

 
    public void TakeMagicDamage(float mDamage, Transform damageCauser)
    {
        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        SyncedCurrentHealth -= CalcTakenPhysicalDamage(mDamage);
        enemyController.ReceivedDamageFrom(damageCauser);
    }


    public override void Die()
    {
        base.Die();

        inventory.AddCash(cashValue);
        inventory.AddParts(partsValue);

        NetworkServer.Destroy(gameObject);        
    }
}