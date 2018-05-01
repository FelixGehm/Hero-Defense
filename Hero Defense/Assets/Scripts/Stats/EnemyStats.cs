using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EnemyStats : CharacterStats {


    public override void Die()
    {
        base.Die();

        //death stuff
        NetworkServer.Destroy(gameObject);
    }
}