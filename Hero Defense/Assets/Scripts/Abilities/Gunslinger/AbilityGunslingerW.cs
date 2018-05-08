using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterStats))]
public class AbilityGunslingerW : AbilityBasic
{
    public float manaPerSeconds;


    public float physicalDamageBonus = 5;
    Stat physicalDamage;

    public float critChanceBonus = 0.05f;
    Stat critChance;

    private bool bonusIsActive = false;

    private PlayerMotor motor;
    private CharacterEventController cec;
    private PlayerController pc;


    // Use this for initialization
    void Start()
    {
        GetComponent<CharacterEventManager>().OnAbilityTwo += Cast;

        CharacterStats myStats = GetComponent<CharacterStats>();

        motor = GetComponent<PlayerMotor>();
        cec = GetComponent<CharacterEventController>();
        pc = GetComponent<PlayerController>();

        physicalDamage = myStats.physicalDamage;
        critChance = myStats.critChance;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        //TODO Consume Mana && Check for mana to deactivate
    }

    protected override void Cast()
    {
        

        if (isLocalPlayer && currentCooldown <= 0)
        {
            currentCooldown = abilityCooldown;

            if (!bonusIsActive)
            {
                pc.isCasting = true;
                cec.isCasting = true;
                motor.MoveToPoint(transform.position);
                StartCoroutine(ActivateBuff());
            }
            else
            {
                bonusIsActive = false;
                physicalDamage.RemoveModifier(physicalDamageBonus);
                critChance.RemoveModifier(critChanceBonus);
            }
        }
    }

    private IEnumerator ActivateBuff()
    {
        bonusIsActive = true;

        // FIRE ANIMATION HERE! 
        yield return new WaitForSeconds(abilityCastTime);

        // ANIMATION HERE vorbei! 
        physicalDamage.AddModifier(physicalDamageBonus);
        critChance.AddModifier(critChanceBonus);

        pc.isCasting = false;
        cec.isCasting = false;
    }


}
