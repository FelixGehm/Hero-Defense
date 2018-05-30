using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AbilityGunslingerR : AbilityBasic
{
    public GameObject previewPrefab;
    private GameObject previewGameObject;

    public GameObject projectilePrefab;

    [Space]
    public float projectilePhysicalDamageStart = 40.0f;
    public float projectilePhysicalDamageMax = 100.0f;
    public float projectileSpeed = 1.0f;

    public float chargeTime = 2.0f;


    public LayerMask rightClickMask;

    float timeAtCastStart;
    float timeAtShooting;

    [Space]
    public Transform firePoint;
    public float range = 10.0f;

    public float timeBetweenShots = 0.1f;
    public int maxNoTargets = 6;


    public List<Transform> targets;


    KeyCode abilityKey;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        GetComponent<CharacterEventManager>().OnAbilityFour += Cast;
        abilityKey = characterEventController.abilityFourKey;

        targets = new List<Transform>();
    }

    bool skipFrame = true;
    bool firstAnimTriggered = false;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (isLocalPlayer)
        {
            if (isCasting)
            {
                if (!skipFrame)
                {
                    if (Input.GetMouseButtonDown(1) && !isAnimating)        // RightClick
                    {
                        CancelCast();
                    }

                    if (Input.GetMouseButtonDown(0))        // LeftClick 
                    {
                        if (targets.Count < 6)
                        {
                            MarkTarget();
                        }
                        else
                        {
                            StartCoroutine(ShootProjectiles());
                        }

                    }

                    if (Input.GetKeyDown(abilityKey))
                    {
                        StartCoroutine(ShootProjectiles());
                    }
                }
                skipFrame = false;
            }
            if (Input.GetKeyUp(abilityKey) && !firstAnimTriggered)  //Workaround
            {
                TriggerAnimation();
                firstAnimTriggered = true;
            }
        }
        Debug.Log(firstAnimTriggered);
    }

    protected override void Cast()
    {
        //TriggerAnimation();                      // Felix: Wird die Animation hier getriggert funktioneirt sie beim Client nicht. Ich kann leider nicht nachvollziehen woran das liegen könnte. Methoden werden alle korrekt aufgerufen.
        playerMotor.MoveToPoint(transform.position);    // Stehen bleiben

        skipFrame = true;
        isCasting = true;
        playerController.IsCasting = true;
        characterEventController.isCasting = true;

        timeAtCastStart = Time.time;

        SpawnPreview();
    }

    void SpawnPreview()
    {
        //Debug.Log("SpawnPreView()");
        previewGameObject = Instantiate(previewPrefab, transform.position, Quaternion.Euler(90, 0, 0));
    }

    void CancelCast()
    {
        firstAnimTriggered = false;
        CancelAnimation();
        Destroy(previewGameObject);

        foreach (Transform t in targets)
        {
            GameObject haloInstance = t.Find("GFX").Find("GunslingerRIndicator").gameObject;
            haloInstance.SetActive(false);
        }

        if (targets.Count > 0)
        {
            currentCooldown = abilityCooldown;
        }
        targets.Clear();

        isCasting = false;
        playerController.IsCasting = false;
        characterEventController.isCasting = false;
    }

    void MarkTarget()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, rightClickMask))
        {
            //Debug.Log("RayCast hit: " + hit.collider.tag);
            if (hit.collider.tag == "Enemy")
            {
                Transform foundTarget = hit.transform;

                float distance = Vector3.Distance(foundTarget.position, transform.position);

                if (distance <= range)
                {
                    if (!targets.Contains(foundTarget))
                    {
                        targets.Add(foundTarget);
                    }

                    // activate for target selection indicator
                    GameObject haloInstance = foundTarget.transform.Find("GFX").Find("GunslingerRIndicator").gameObject;
                    haloInstance.SetActive(true);
                }
            }
        }
    }

    private IEnumerator ShootProjectiles()
    {
        
        //TriggerAnimation();
        //CancelAnimation();
        timeAtShooting = Time.time;
        foreach (Transform t in targets)
        {
            GameObject haloInstance = t.Find("GFX").Find("GunslingerRIndicator").gameObject;
            haloInstance.SetActive(false);
        }

        Destroy(previewGameObject);

        #region damage calculation
        // Calculate damage from projectiles
        float deltaT = timeAtShooting - timeAtCastStart;
        float percentDamage;

        if (deltaT >= chargeTime)
        {
            percentDamage = 1.0f;
        }
        else
        {
            percentDamage = deltaT / chargeTime;
        }
        float projectilePhysicalDamage = Mathf.Lerp(projectilePhysicalDamageStart, projectilePhysicalDamageMax, percentDamage);
        #endregion

        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);
        firstAnimTriggered = false;

        bool isFirstShot = true;
        foreach (Transform target in targets)
        {
            // Animation ? 
            TriggerSecondAnimation();
            if (!isFirstShot)
            {
                yield return new WaitForSeconds(timeBetweenShots);
            }
            else
            {
                isFirstShot = false;
            }

            NetworkInstanceId targetID = target.GetComponent<NetworkIdentity>().netId;

            if (isServer)
            {
                CmdSpawnProjectileOnServer(projectilePhysicalDamage, targetID);
            }
            else
            {
                TellServerToSpawnProjectile(projectilePhysicalDamage, targetID);
            }
        }

        isAnimating = false;
        isCasting = false;
        playerController.IsCasting = false;
        characterEventController.isCasting = false;

        targets.Clear();
        currentCooldown = abilityCooldown;
    }

    [Command]
    void CmdSpawnProjectileOnServer(float damage, NetworkInstanceId id)
    {
        //Debug.Log("CmdSpawnProjectileOnServer()");

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, transform.rotation);
        Debug.Log(projectileGO);

        NetworkProjectile projectile = projectileGO.GetComponent<NetworkProjectile>();
        if (projectile != null)
        {
            Transform targetTransform = NetworkServer.FindLocalObject(id).transform;
            Debug.Log(targetTransform.name);

            projectile.InitBullet(targetTransform, damage);
            projectile.speed = projectileSpeed;
        }


        NetworkServer.Spawn(projectileGO);
    }

    [ClientCallback]
    void TellServerToSpawnProjectile(float damage, NetworkInstanceId id)
    {
        if (!isServer)
        {
            CmdSpawnProjectileOnServer(damage, id);
        }
    }

    #region Editor
    private void OnDrawGizmosSelected()
    {
        if (range <= 0) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, (float)range);
    }
    #endregion
}
