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

    public void TakeTrueDamage(float tDamage, Transform damageCauser)
    {
        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        float damage = tDamage;
        damage = Mathf.Max(damage, 0);      // Check if damage is < 0, if yes -> set to 0

        CurrentHealth -= damage;


        Debug.Log("Hier bin ich richtig!");
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

        Debug.Log("Hier bin ich richtig!");
        enemyController.ReceivedDamageFrom(damageCauser);
    }

 
    public void TakeMagicDamage(float mDamage, Transform damageCauser)
    {
        if (!isServer)      // Ausschließlich der Server verursacht so Schaden.
        {
            return;
        }

        CurrentHealth -= CalcTakenPhysicalDamage(mDamage);

        Debug.Log("Hier bin ich richtig!");
        enemyController.ReceivedDamageFrom(damageCauser);
    }


    public override void Die()
    {
        base.Die();

        //death stuff
        NetworkServer.Destroy(gameObject);
    }
}