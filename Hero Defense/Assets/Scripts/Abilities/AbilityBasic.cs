using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterEventController))]
public abstract class AbilityBasic : NetworkBehaviour
{

    public float abilityCooldown = 4.0f;
    protected float currentCooldown = 0.0f;

    public float abilityCastTime = 0.3f;


    protected bool isCasting = false;
    protected bool isAnimating = false;
    
    void Start()
    {

    }
        
    protected virtual void Update()
    {
        if (isLocalPlayer)
        {
            if(!isCasting)
            {
                currentCooldown -= Time.deltaTime;
            }
        }
    }

    protected abstract void Cast();
    


}
