using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityMageR : AbilityBasic
{
    [Header("Setup Fields")]
    public Transform spawnPosition;
    public GameObject previewPrefab;
    private GameObject previewGO;
    [Space]
    public LayerMask rightClickMask;

    [Header("Spell Settings")]
    public GameObject spellPrefab;
    private GameObject spellGO;

    KeyCode abilityKey;

    private bool hasCasted = false;

    private bool isInAbility = false;
    public bool IsInAbility
    {
        get
        {
            return isInAbility;
        }
        set
        {
            playerController.IsCasting = value;
            isInAbility = value;
        }
    }

    protected override void Start()
    {
        base.Start();
        GetComponent<CharacterEventManager>().OnAbilityFour += Cast;
        abilityKey = characterEventController.abilityOneKey;
    }

    bool skipFrame = false;
    protected override void Update()
    {
        base.Update();

        if (isLocalPlayer)
        {
            if (isCasting)
            {
                if (previewGO != null)
                {
                    // Position und Rotation der Preview anpassen
                    previewGO.transform.position = transform.position;
                    previewGO.transform.rotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
                }

                if (!skipFrame)
                {
                    if (Input.GetMouseButtonDown(1) && !isAnimating)        // RightClick
                    {
                        CancelCast();
                    }

                    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(abilityKey))        // LeftClick or AbilityKey
                    {
                        playerMotor.MoveToPoint(transform.position);
                        StartCoroutine(CastAbility(transform.position));
                    }
                }
            }
            skipFrame = false;

            if (IsInAbility)
            {
                spellGO.transform.rotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
            }
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
            IsCasting(true);
            SpawnPreview();
            skipFrame = true;
        }
    }

    private void CancelCast()
    {
        IsCasting(false);
        Destroy(previewGO);
    }

    private void SpawnPreview()
    {
        IsCasting(true);
        previewGO = Instantiate(previewPrefab);
    }


    public IEnumerator CastAbility(Vector3 spawnPos)
    {
        hasCasted = false;
        Destroy(previewGO);

        TriggerAnimation();
        transform.rotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);
        hasCasted = true;

        isAnimating = false;
        IsCasting(false);

        currentCooldown = abilityCooldown;
        IsInAbility = true;
        if (isServer)
        {
            CmdSpawnSpellOnServer(spawnPos);
        }
        else
        {
            TellServerToSpawnSpell(spawnPos);
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
        {
            CmdSpawnSpellOnServer(spawnPos);
        }
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
}
