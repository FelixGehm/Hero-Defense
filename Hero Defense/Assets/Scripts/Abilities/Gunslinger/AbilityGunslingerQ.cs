using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterEventController))]
public class AbilityGunslingerQ : AbilityBasic
{
    [Space]
    public GameObject previewPrefab;
    private GameObject previewGameObject;

    [Space]
    public LayerMask rightClickMask;

    [Header("Projectile settings")]
    public GameObject projectilePrefab;

    public float projectileSpeed = 10.0f;

    public float projectilePhysicalDamageStart = 10.0f;
    public float projectilePhysicalDamageMax = 100.0f;

    public float chargeTime = 2.0f;
  
    KeyCode abilityKey;

    float timeAtCastStart;
    float timeAtShooting;
    

    protected override void Start()
    {
        base.Start();

        GetComponent<CharacterEventManager>().OnAbilityOne += Cast;
     

        abilityKey = characterEventController.abilityOneKey;



        if (previewPrefab == null)
        {
            Debug.LogWarning("No Preview-Prefab on AbilityGunslingerQ!");
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("No Projectile-Prefab on AbilityGunslingerQ!");
        }
    }

    bool skipFrame = false;

    protected override void Update()
    {
        base.Update();

        if (isLocalPlayer)
        {
            if (isCasting)
            {
                if (previewGameObject != null)
                {
                    // Position und Rotation der Preview anpassen
                    previewGameObject.transform.position = transform.position;
                    previewGameObject.transform.rotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
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
                        StartCoroutine(ShootProjectile(GetDirectionVectorBetweenPlayerAndMouse()));
                        
                        timeAtShooting = Time.time;
                        
                    }
                }
                skipFrame = false;
            }
        }
    }

    protected override void Cast()
    {

        if (isLocalPlayer && currentCooldown <= 0)
        {
            playerController.IsCasting = true;
            characterEventController.isCasting = true;

            ShowPreview();
            skipFrame = true;

            timeAtCastStart = Time.time;
        }
    }

    public void CancelCast()
    {
        isCasting = false;
        playerController.IsCasting = false;
        characterEventController.isCasting = false;

        Destroy(previewGameObject);
    }


    private void ShowPreview()
    {
        isCasting = true;

        previewGameObject = Instantiate(previewPrefab);
    }

    private IEnumerator ShootProjectile(Vector3 direction)
    {
        TriggerAnimation();

        Vector3 firePoint = previewGameObject.GetComponent<CalcDistanceFromStartToEnd>().GetStartPos();
        Quaternion rotation = previewGameObject.transform.rotation;
        transform.rotation = rotation;      //erstmal so... vielleicht lieber lerpen?
        float maxDistance = previewGameObject.GetComponent<CalcDistanceFromStartToEnd>().GetDistance();

        Destroy(previewGameObject);

        //Debug.Log("ShootProjectile(): AnimationTime!");
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);
        isAnimating = false;

        //Debug.Log("ShootProjectile(): AnimationTime over!");

        isCasting = false;
        playerController.IsCasting = false;
        characterEventController.isCasting = false;

        currentCooldown = abilityCooldown;

        // Calculate damage from projectile
        float deltaT = timeAtShooting - timeAtCastStart;
        float percentDamage;

        if (deltaT >= chargeTime)
        {
            percentDamage = 1.0f;
        } else
        {
            percentDamage = deltaT / chargeTime;
        }

        float projectilePhysicalDamage = Mathf.Lerp(projectilePhysicalDamageStart,projectilePhysicalDamageMax, percentDamage);
        //Debug.Log("Damage done by Projectile ="+ projectilePhysicalDamage);

        if (isServer)
        {
            CmdSpawnProjectileOnServer(projectilePhysicalDamage, direction, firePoint, rotation, maxDistance);
        }
        else
        {
            TellServerToSpawnProjectile(projectilePhysicalDamage, direction, firePoint, rotation, maxDistance);
        }

    }

    [Command]
    void CmdSpawnProjectileOnServer(float damage, Vector3 direction, Vector3 firePoint, Quaternion rotation, float maxDistance)
    {
        //Debug.Log("CmdSpawnProjectileOnServer()");

        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint, rotation);
        GunslingerQProjectile projectile = projectileGO.GetComponent<GunslingerQProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.SetMaxDistance(maxDistance);
            projectile.speed = projectileSpeed;
            projectile.damage = damage;
        }

        //Debug.Log(projectileGO);
        NetworkServer.Spawn(projectileGO);
    }

    [ClientCallback]
    void TellServerToSpawnProjectile(float damage, Vector3 direction, Vector3 firePoint, Quaternion rotation, float maxDistance)
    {
        //Debug.Log("TellServerToSpawnProjectile(float damage, Vector3 direction)");

        if (!isServer)
        {
            CmdSpawnProjectileOnServer(damage, direction, firePoint, rotation, maxDistance);
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
