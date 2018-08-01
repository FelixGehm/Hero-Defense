﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityMageW : AbilityBasic
{
    public float maxCastRange = 4;
    public GameObject spellPrefab;

    public GameObject previewPrefab;
    private GameObject previewGO;

    public GameObject maxRangePrefab;
    private GameObject maxRangeGO;

    public GameObject spawnIndicatorPrefab;
    private GameObject spawnIndicatorGO;

    public LayerMask rightClickMask;

    [Tooltip("the fixed base damage of the ability")]
    public float baseDamage;
    [Tooltip("the factor the magicDamage of the Character is multiplied with to crate additional damage to the base damage")]
    public float damageFactor;
    public float baseHealAmount;
    public float healFactor;

    private float damage = 20;
    private float healAmount = 20;

    private PlayerStats myStats;

    KeyCode abilityKey;

    private Vector3 spawnPos;

    private bool hasCasted = false;

    protected override void Start()
    {
        base.Start();
        GetComponent<CharacterEventManager>().OnAbilityTwo += Cast;
        abilityKey = characterEventController.abilityTwoKey;
        myStats = GetComponent<PlayerStats>();
        CalcDamage();
    }

    bool skipFrame = false;
    protected override void Update()
    {
        base.Update();

        if (isLocalPlayer && previewGO != null && maxRangeGO != null)
        {
            spawnPos = GetMousePosOnWorld();

            if (GetDistanceBetweenPlayerAndTargetPos(spawnPos) > maxCastRange)
            {
                Vector3 direction = Vector3.Normalize(spawnPos - transform.position);

                spawnPos = transform.position + maxCastRange * direction;
                //spawnPos.y = 0.1f;
            }


            previewGO.transform.position = new Vector3(spawnPos.x, 0.1f, spawnPos.z);
            maxRangeGO.transform.position = transform.position;



            if (!skipFrame && isCasting)
            {
                if (Input.GetMouseButtonDown(1) && !isAnimating)
                {
                    CancelCast();
                }

                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(abilityKey))
                {
                    playerMotor.MoveToPoint(transform.position);
                    StartCoroutine(CastAbility(spawnPos));
                }
            }
            skipFrame = false;
        }

        if (isLocalPlayer && !isCasting && !isAnimating && hasCasted && Input.GetMouseButtonDown(1))
        {
            CancelAnimation();
            hasCasted = false;
        }
    }

    protected override void Cast()
    {
        if (isLocalPlayer && currentCooldown <= 0)
        {
            skipFrame = true;
            SpawnPreview();
        }
    }

    private void CancelCast()
    {
        isCasting = false;
        playerController.IsCasting = false;
        characterEventController.isCasting = false;
        Destroy(previewGO);
        Destroy(maxRangeGO);
    }

    private void SpawnPreview()
    {
        isCasting = true;
        playerController.IsCasting = true;
        characterEventController.isCasting = true;

        previewGO = Instantiate(previewPrefab);
        maxRangeGO = Instantiate(maxRangePrefab);
    }

    private void CalcDamage()
    {
        float mDmg = myStats.magicDamage.GetValue();
        damage = baseDamage + mDmg * damageFactor;
        healAmount = baseHealAmount + mDmg * healFactor;
    }

    public IEnumerator CastAbility(Vector3 castPosition)
    {
        if (isServer)
        {
            CmdSpawnIndicatorOnServer(castPosition);
        }
        else
        {
            TellServerToSpawnIndicator(castPosition);
        }
        hasCasted = false;
        Destroy(previewGO);
        Destroy(maxRangeGO);

        TriggerAnimation();
        transform.rotation = Quaternion.AngleAxis(GetPlayerAngle(), Vector3.up);
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);
        hasCasted = true;

        isAnimating = false;
        isCasting = false;
        playerController.IsCasting = false;
        characterEventController.isCasting = false;

        currentCooldown = abilityCooldown;
        CalcDamage();
        if (isServer)
        {
            CmdSpawnSpellOnServer(castPosition);
            CmdDestroyIndicatorOnServer();
        }
        else
        {
            TellServerToSpawnSpell(castPosition);
            TellServerToDestroyIndicator();
        }
    }




    [Command]
    void CmdSpawnSpellOnServer(Vector3 spawnPosition)
    {
        GameObject spellGO = Instantiate(spellPrefab, spawnPosition, transform.rotation);
        MageWSpell spellScript = spellGO.GetComponent<MageWSpell>();
        spellScript.Init(this.transform, damage, healAmount);
        NetworkServer.Spawn(spellGO);
    }

    [ClientCallback]
    void TellServerToSpawnSpell(Vector3 spawnPosition)
    {
        if (!isServer)
        {
            CmdSpawnSpellOnServer(spawnPosition);
        }
    }

    [Command]
    void CmdSpawnIndicatorOnServer(Vector3 spawnPosition)
    {
        spawnIndicatorGO = Instantiate(spawnIndicatorPrefab, spawnPosition, spawnIndicatorPrefab.transform.rotation);
        NetworkServer.Spawn(spawnIndicatorGO);
    }

    [ClientCallback]
    void TellServerToSpawnIndicator(Vector3 spawnPosition)
    {
        if (!isServer)
        {
            CmdSpawnIndicatorOnServer(spawnPosition);
        }
    }

    [Command]
    void CmdDestroyIndicatorOnServer()
    {
        NetworkServer.Destroy(spawnIndicatorGO);
    }

    [ClientCallback]
    void TellServerToDestroyIndicator()
    {
        CmdDestroyIndicatorOnServer();
    }

    private Vector3 GetMousePosOnWorld()
    {
        Vector3 mousePos = new Vector3();

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, rightClickMask))
        {
            mousePos = hit.point;
        }

        return mousePos;
    }

    private float GetDistanceBetweenPlayerAndTargetPos(Vector3 targetPos)
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        return distance;
    }

    private float GetPlayerAngle()
    {
        float angle = 0;
        Vector3 direction = Vector3.Normalize(spawnPos - transform.position);
        angle = Vector3.Angle(Vector3.forward, direction);

        if (direction.x < 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }

    /*
    protected float GetAngleFromDirection()
    {
        float angle = 0.0f;
        Vector3 direction = GetDirectionVectorBetweenPlayerAndMouse();
        angle = Vector3.Angle(Vector3.forward, direction);

        if (direction.x < 0.0f)
        {
            angle = 360.0f - angle;
        }
        //Debug.Log("angle = " + angle);
        return angle;
    }

    protected Vector3 GetDirectionVectorBetweenPlayerAndMouse()
    {
        Vector3 playerPos = transform.position;

        Vector3 mousePos = new Vector3();


        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, rightClickMask))
        {
            mousePos = hit.point;
        }

        Vector3 direction = Vector3.Normalize(mousePos - playerPos);

        return direction;
    }
    */
}
