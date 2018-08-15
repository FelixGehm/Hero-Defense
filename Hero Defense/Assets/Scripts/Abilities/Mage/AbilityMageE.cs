using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityMageE : AbilityBasic
{
    [Header("Spell Settings")]
    public float spellSpeed = 5;
    public float maxCastRange = 10;
    public float maxBounceRange = 5;

    public float maxNumberOfBounces = 6;

    [Tooltip("the fixed base damage of the ability")]
    public float baseDamage;
    [Tooltip("the factor the magicDamage of the Character is multiplied with to crate additional damage to the base damage")]
    public float damageFactor;
    public float baseHealAmount;
    public float healFactor;

    private float damage = 10;
    private float healAmount = 10;

    [Header("Setup Fields")]
    public GameObject spellPrefab;
    public LayerMask clickMask;
    public LayerMask groundMask;
    public Transform spawnPoint;
    public GameObject maxRangePrefab;
    private GameObject maxRangeGO;

    private PlayerStats myStats;

    KeyCode abilityKey;

    private GameObject target;

    private bool hasCasted = false;

    private bool followTarget = false;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        GetComponent<CharacterEventManager>().OnAbilityThree += Cast;
        abilityKey = characterEventController.abilityThreeKey;
        myStats = GetComponent<PlayerStats>();
        CalcDamage();
    }

    bool skipFrame;
    protected override void Update()
    {
        base.Update();
        if (isLocalPlayer && maxRangeGO != null)
        {
            maxRangeGO.transform.position = transform.position;

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
                        if (hit.collider.tag == "Enemy" || hit.collider.tag == "Player")
                        {
                            if (Vector3.Distance(hit.transform.position, transform.position) <= maxCastRange)
                            {
                                target = hit.collider.gameObject;
                                StartCoroutine(CastAbility());
                                playerMotor.MoveToPoint(transform.position);
                            }
                            else
                            {
                                target = hit.collider.gameObject;
                                playerMotor.MoveToPoint(target.transform.position);
                                followTarget = true;
                                ShowSpecialCursor(false);
                                Destroy(maxRangeGO);
                            }
                        }
                    }
                }
            }
            skipFrame = false;
        }

        if (isLocalPlayer && followTarget)
        {
            FollowTargetAndCastInRange();

        }

        if (isLocalPlayer && !isCasting && !isAnimating && hasCasted && Input.GetMouseButtonDown(1))
        {
            CancelAnimation();
            hasCasted = false;
        }
    }

    protected override void Cast()
    {
        if (currentCooldown <= 0)
        {
            ShowSpecialCursor(true);
            maxRangeGO = Instantiate(maxRangePrefab);
            IsCasting(true);
            skipFrame = true;
        }

    }

    private void CancelCast()
    {
        ShowSpecialCursor(false);
        IsCasting(false);
        Destroy(maxRangeGO);
    }

    private void ShowSpecialCursor(bool b)
    {
        if (b)
        {
            UICursor.instance.isSelecting = true;
            UICursor.instance.SetMoveCursor();
        }
        else
        {
            UICursor.instance.isSelecting = false;
            UICursor.instance.SetMoveCursor();
        }
    }

    private void CalcDamage()
    {
        float mDmg = myStats.magicDamage.GetValue();
        damage = baseDamage + mDmg * damageFactor;
        healAmount = baseHealAmount + mDmg * healFactor;
    }

    public IEnumerator CastAbility()
    {
        hasCasted = false;
        ShowSpecialCursor(false);
        Destroy(maxRangeGO);

        TriggerAnimation();
        transform.rotation = Quaternion.AngleAxis(GetPlayerAngle(target.transform.position), Vector3.up);
        isAnimating = true;
        IsCasting(true);

        yield return new WaitForSeconds(abilityCastTime);
        hasCasted = true;

        isAnimating = false;
        IsCasting(false);

        currentCooldown = abilityCooldown;
        CalcDamage();
        if (isServer)
        {
            CmdSpawnSpellOnServer(spawnPoint.position, target, spellSpeed, maxBounceRange, damage, healAmount, maxNumberOfBounces);
        }
        else
        {
            TellServerToSpawnSpell(spawnPoint.position, target, spellSpeed, maxBounceRange, damage, healAmount, maxNumberOfBounces);
        }
    }






    [Command]
    void CmdSpawnSpellOnServer(Vector3 spawnPosition, GameObject target, float spellSpeed, float maxBounceRange, float damage, float healAmount, float maxNumberOfBounces)
    {
        GameObject spellGO = Instantiate(spellPrefab, spawnPosition, transform.rotation);

        MageESpell spellScript = spellGO.GetComponent<MageESpell>();
        //Debug.Log("target: " + target.name);
        //Debug.Log("hasScript: " + spellScript != null);
        if (spellScript != null && target != null)
        {
            spellScript.Init(target, spellSpeed, maxBounceRange, damage, healAmount, maxNumberOfBounces, this.transform);
        }
        else
        {
            Debug.Log("Failed to Init spell");
        }
        NetworkServer.Spawn(spellGO);
    }

    [ClientCallback]
    void TellServerToSpawnSpell(Vector3 spawnPosition, GameObject target, float spellSpeed, float maxBounceRange, float damage, float healAmount, float maxNumberOfBounces)
    {
        if (!isServer)
        {
            CmdSpawnSpellOnServer(spawnPosition, target, spellSpeed, maxBounceRange, damage, healAmount, maxNumberOfBounces);
        }
    }

    private void FollowTargetAndCastInRange()
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= maxCastRange)    //cast when in range
        {
            StartCoroutine(CastAbility());
            playerMotor.MoveToPoint(transform.position);
            followTarget = false;
        }
        else
        {
            playerMotor.SetDestination(target.transform.position);      //follow the target
        }
        if (Input.GetMouseButtonDown(1))    //cancel Follow and Cast
        {
            //Debug.Log("Cancel Follow");
            IsCasting(false);
            followTarget = false;
            target = null;
            Vector3 mPos = GetMousePosOnWorld();

            if (mPos != new Vector3(0, 0, 0))
            {
                playerMotor.MoveToPoint(mPos);
            }
            else
            {
                playerMotor.MoveToPoint(transform.position);
            }

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

    private Vector3 GetMousePosOnWorld()
    {
        Vector3 mousePos = new Vector3();

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, groundMask))
        {
            mousePos = hit.point;
        }

        return mousePos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxCastRange);
    }
}
