using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityMageR : AbilityBasic
{
    [Header("Setup Fields")]
    public GameObject previewPrefab;
    private GameObject previewGO;
    [Space]
    public LayerMask rightClickMask;

    [Header("Spell Settings")]
    public GameObject spellPrefab;
    public GameObject spellPrefab_WithLocalPlayerAuthority;
    private GameObject spellGO;
    [Space]
    public float spellDuration = 6;
    private float spellStartTime;
    public float spellRotationSpeed = 25;

    KeyCode abilityKey;

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
        abilityKey = characterEventController.abilityFourKey;
    }

    bool skipFrame = false;
    protected override void Update()
    {
        base.Update();
        //Debug.Log(spellGO);
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
                /*
                if (spellGO != null)
                {
                    Quaternion wantedRotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
                    spellGO.transform.rotation = Quaternion.RotateTowards(spellGO.transform.rotation, wantedRotation, Time.deltaTime * spellRotationSpeed);
                    this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, wantedRotation, Time.deltaTime * spellRotationSpeed);
                }
                else
                {
                    //Debug.Log("spellGO is not set!");
                }
                */
                Quaternion wantedRotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, wantedRotation, Time.deltaTime * spellRotationSpeed);


                if (Time.time - spellStartTime >= 6 || Input.GetMouseButtonDown(1))
                {
                    if (isServer)
                    {
                        CmdDestroySpellOnServer();
                    }
                    else
                    {
                        TellServerToDestroySpell();
                    }


                    //Destroy(spellGO);
                    IsInAbility = false;
                    CancelAnimation();
                }
            }
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
        Destroy(previewGO);

        TriggerAnimation();
        transform.rotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);

        isAnimating = false;
        IsCasting(false);

        currentCooldown = abilityCooldown;
        IsInAbility = true;
        spellStartTime = Time.time;
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
        spellGO.transform.parent = this.transform;
        NetworkServer.Spawn(spellGO);
        //RpcSetSpellGO(spellGO);
        RpcSyncSpellGoOnClients(spellGO);
    }

    [ClientCallback]
    void TellServerToSpawnSpell(Vector3 spawnPos)
    {
        if (!isServer)
        {
            CmdSpawnSpellOnServer(spawnPos);
        }
    }

    [ClientRpc]
    void RpcSyncSpellGoOnClients(GameObject spellGO)
    {
        spellGO.transform.parent = this.transform;
    }

    /*
    [Command]
    void CmdSpawnSpellOnServer(Vector3 spawnPos)
    {
        spellGO = Instantiate(spellPrefab, spawnPos, transform.rotation);
        NetworkServer.Spawn(spellGO);
        //RpcSetSpellGO(spellGO);
    }

    [Command]
    void CmdSpawnSpellWithClientAuthority(Vector3 spawnPos)
    {
        spellGO = Instantiate(spellPrefab_WithLocalPlayerAuthority, spawnPos, transform.rotation);
        NetworkServer.SpawnWithClientAuthority(spellGO, connectionToClient);
        RpcSetSpellGO(spellGO);
    }

    [ClientCallback]
    void TellServerToSpawnSpell(Vector3 spawnPos)
    {
        if (!isServer)
        {
            //CmdSpawnSpellOnServer(spawnPos);
            CmdSpawnSpellWithClientAuthority(spawnPos);
        }
    }
    */

    [Command]
    void CmdDestroySpellOnServer()
    {
        NetworkServer.Destroy(spellGO);
    }

    [ClientCallback]
    void TellServerToDestroySpell()
    {
        CmdDestroySpellOnServer();
    }

    [ClientRpc]
    void RpcSetSpellGO(GameObject go)
    {
        spellGO = go;
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
