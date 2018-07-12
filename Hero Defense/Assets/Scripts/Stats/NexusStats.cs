using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NexusStats : CharacterStats
{



    public event System.Action OnNexusDestroyed;

    public override void Die()
    {
        base.Die();

        OnNexusDestroyed?.Invoke();

        //Destroy Object, Animationen und so, vllt coole Kamerafahrt, GameLost einblenden, keine ahnung
    }




}
