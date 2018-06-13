using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityMageE : AbilityBasic
{

    public GameObject spellPrefab;

    public Texture2D selectTargetIndicator;

    public LayerMask clickMask;

    public Transform spawnPoint;

    KeyCode abilityKey;

    private GameObject target;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        GetComponent<CharacterEventManager>().OnAbilityThree += Cast;
        abilityKey = characterEventController.abilityThreeKey;
    }

    bool skipFrame;
    protected override void Update()
    {
        base.Update();
        if (isLocalPlayer)
        {


            if (!skipFrame && isCasting)
            {
                if (Input.GetMouseButtonDown(1) && !isAnimating)
                {
                    CancelCast();
                }

                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(abilityKey))
                {
                    RaycastHit hit;
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100, clickMask))
                    {
                        if (hit.collider.tag == "Enemy")
                        {
                            target = hit.collider.gameObject;
                            StartCoroutine(CastAbility());
                        }
                    }
                }
            }
            skipFrame = false;
        }
    }

    protected override void Cast()
    {
        Debug.Log("AbilityMageE.Cast()");
        ShowSpecialCursor(true);
        IsCasting(true);
        skipFrame = true;
    }

    private void CancelCast()
    {
        ShowSpecialCursor(false);
        IsCasting(false);
    }

    private void ShowSpecialCursor(bool b)
    {
        if (b)
        {
            //set mouse indicator to target
            Cursor.SetCursor(selectTargetIndicator, new Vector2(selectTargetIndicator.width / 2, selectTargetIndicator.height / 2), CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

    public IEnumerator CastAbility()
    {
        ShowSpecialCursor(false);

        TriggerAnimation();
        transform.rotation = Quaternion.AngleAxis(GetPlayerAngle(target.transform.position), Vector3.up);
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);

        isAnimating = false;
        IsCasting(false);

        currentCooldown = abilityCooldown;

        if (isServer)
        {
            CmdSpawnSpellOnServer(spawnPoint.position);
        }
        else
        {
            TellServerToSpawnSpell(spawnPoint.position);
        }
    }




    [Command]
    void CmdSpawnSpellOnServer(Vector3 spawnPosition)
    {
        GameObject spellGO = Instantiate(spellPrefab, spawnPosition, transform.rotation);
        NetworkServer.Spawn(spellGO);

        MageESpell spellScript = spellGO.GetComponent<MageESpell>();
        if (spellScript != null && target != null)
        {
            spellScript.Init(target);
        }
        else
        {
            Debug.Log("Failed to Init spell");
        }
    }

    [ClientCallback]
    void TellServerToSpawnSpell(Vector3 spawnPosition)
    {
        if (!isServer)
        {
            CmdSpawnSpellOnServer(spawnPosition);
        }
    }

    private float GetPlayerAngle(Vector3 target)
    {
        float angle = 0;
        Vector3 direction = Vector3.Normalize(target - transform.position);
        angle = Vector3.Angle(Vector3.forward, direction);

        if (direction.x < 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }
}
