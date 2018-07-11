using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityMageQ : AbilityBasic
{
    public GameObject spellPrefab;
    private GameObject spellGO;

    KeyCode abilityKey;

    private bool hasCasted = false;

    protected override void Start()
    {
        base.Start();
        GetComponent<CharacterEventManager>().OnAbilityOne += Cast;
        abilityKey = characterEventController.abilityOneKey;
    }


    bool skipFrame = false;
    protected override void Update()
    {
        base.Update();

        if (isLocalPlayer && !isCasting && !isAnimating && hasCasted && Input.GetMouseButtonDown(1))
        {
            CancelAnimation();
            hasCasted = false;
        }
    }

    protected override void Cast()
    {
        if (!isLocalPlayer) return;
        playerMotor.MoveToPoint(transform.position);
        IsCasting(true);
        StartCoroutine(CastAbility(transform.position));
    }

    public IEnumerator CastAbility(Vector3 castPosition)
    {
        hasCasted = false;

        TriggerAnimation();           //funktioniert auf clientseite nicht... keine Ahnung, warum
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);
        hasCasted = true;

        isAnimating = false;
        IsCasting(false);

        currentCooldown = abilityCooldown;

        if (isServer)
        {
            CmdSpawnSpellOnServer(castPosition);
        }
        else
        {
            TellServerToSpawnSpell(castPosition);
        }
    }

    [Command]
    void CmdSpawnSpellOnServer(Vector3 spawnPos)
    {
        spellGO = Instantiate(spellPrefab, spawnPos, transform.rotation);
        NetworkServer.Spawn(spellGO);
    }

    [ClientCallback]
    void TellServerToSpawnSpell(Vector3 spawnPos)
    {
        if (!isServer)
            CmdSpawnSpellOnServer(spawnPos);
    }
}
