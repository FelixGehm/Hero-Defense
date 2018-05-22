using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterEventController))]
public abstract class AbilityBasic : NetworkBehaviour
{
    public enum Slot
    {
        Q, W, E, R, Revive
    }

    public Slot abilitySlot;

    public float abilityCooldown = 4.0f;
    //[HideInInspector]
    public float currentCooldown = 0.0f;

    public float abilityCastTime = 0.3f;


    protected Camera cam;
    protected CharacterEventController characterEventController;
    protected PlayerController playerController;
    protected PlayerMotor playerMotor;



    protected bool isCasting = false;
    protected bool isAnimating = false;

    public event System.Action OnAbilityCasting;

    protected virtual void Start()
    {
        if (isLocalPlayer)
            RegisterAbilityToUI();

        cam = Camera.main;
        characterEventController = GetComponent<CharacterEventController>();
        playerController = GetComponent<PlayerController>();
        playerMotor = GetComponent<PlayerMotor>();
    }

    protected virtual void Update()
    {
        if (isLocalPlayer)
        {
            if (!isCasting && currentCooldown > 0)
            {
                currentCooldown -= Time.deltaTime;
            }
        }
    }

    protected abstract void Cast();

    protected void TriggerAnimation()
    {
        if (OnAbilityCasting != null)
            OnAbilityCasting();
    }

    private void RegisterAbilityToUI()
    {
        if (!GameObject.Find("QImage"))
        {
            Debug.Log("Ability UI Missing.");
            return;
        }

        switch (abilitySlot)
        {
            case Slot.Q:
                GameObject.Find("QImage").GetComponent<AbilityUI>().RegisterAbilityToUI(this);
                break;
            case Slot.W:
                GameObject.Find("WImage").GetComponent<AbilityUI>().RegisterAbilityToUI(this);
                break;
            case Slot.E:
                GameObject.Find("EImage").GetComponent<AbilityUI>().RegisterAbilityToUI(this);
                break;
            case Slot.R:
                GameObject.Find("RImage").GetComponent<AbilityUI>().RegisterAbilityToUI(this);
                break;
            case Slot.Revive:
                Debug.Log("implmentation hier fehlt noch");
                break;

        }
    }



}
