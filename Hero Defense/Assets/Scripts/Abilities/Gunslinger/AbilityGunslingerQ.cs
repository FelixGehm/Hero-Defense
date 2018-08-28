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

    //bool isTimeStartSet = false;
    float timeAtShootStart = 0;

    bool isTimeAtShootingSet = false;
    float timeAtShooting;

    private bool hasCasted = false;

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
        if (skipFrame)
        {
            skipFrame = false;
            return;
        }

        if (isLocalPlayer)
        {
            base.Update();

            if (isCasting)
            {
                if (previewGameObject != null)
                {
                    // Rotation der Preview & des Modells  anpassen                  
                    Quaternion rot = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);

                    previewGameObject.transform.rotation = rot;
                    this.gameObject.transform.rotation = rot;
                }

                if (Input.GetMouseButtonDown(1) && !isAnimating)        // RightClick down
                {
                    CancelCast();
                    return;
                }

                /*
                if (!isTimeStartSet && (Input.GetMouseButton(0) || Input.GetKey(abilityKey)))
                {
                    isTimeStartSet = true;
                    //timeAtShootStart = Time.time;
                }*/


                if (!isTimeAtShootingSet && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(abilityKey)))        // LeftClick or AbilityKey released
                {
                    isTimeAtShootingSet = true;
                    timeAtShooting = Time.time;
                    StartCoroutine(ShootProjectile(GetDirectionVectorBetweenPlayerAndMouse()));
                }
            }
        }

        if (isLocalPlayer && !isCasting && !isAnimating && hasCasted && Input.GetMouseButtonDown(1))
        {
            CancelSecondAnimation();
            hasCasted = false;
        }
    }

    protected override void Cast()
    {

        if (isLocalPlayer && currentCooldown <= 0)
        {
            IsCasting(true);
            TriggerAnimation();
            playerMotor.MoveToPoint(transform.position);
            ShowPreview();
            skipFrame = true;

            timeAtShootStart = Time.time;
        }
    }

    public void CancelCast()
    {
        IsCasting(false);
        CancelAnimation();
        Destroy(previewGameObject);
    }


    private void ShowPreview()
    {
        isCasting = true;

        previewGameObject = Instantiate(previewPrefab);
        previewGameObject.transform.position = transform.position;
    }

    private IEnumerator ShootProjectile(Vector3 direction)
    {
        hasCasted = false;
        //TriggerAnimation();
        TriggerSecondAnimation();

        Vector3 firePoint = previewGameObject.GetComponent<CalcDistanceFromStartToEnd>().GetStartPos();
        Quaternion rotation = previewGameObject.transform.rotation;

        transform.LookAt(previewGameObject.transform);      //roatate Player to TaRGEt
        float maxDistance = previewGameObject.GetComponent<CalcDistanceFromStartToEnd>().GetDistance();

        Destroy(previewGameObject);

        //Debug.Log("ShootProjectile(): AnimationTime!");
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);
        isAnimating = false;
        hasCasted = true;
        //Debug.Log("ShootProjectile(): AnimationTime over!");

        IsCasting(false);

        currentCooldown = abilityCooldown;

        // Calculate damage from projectile
        float deltaT = timeAtShooting - timeAtShootStart;
        float percentDamage;
        //isTimeStartSet = false;
        isTimeAtShootingSet = false;


        if (deltaT >= chargeTime)
        {
            percentDamage = 1.0f;
        }
        else
        {
            percentDamage = deltaT / chargeTime;
        }

        float projectilePhysicalDamage = Mathf.Lerp(projectilePhysicalDamageStart, projectilePhysicalDamageMax, percentDamage);

        Debug.Log("Damage: " + projectilePhysicalDamage);

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

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint, rotation);
        GunslingerQProjectile projectile = projectileGO.GetComponent<GunslingerQProjectile>();

        if (projectile != null)
        {
            projectile.Init(direction, maxDistance, projectileSpeed, damage, this.transform);
        }

        //Debug.Log(projectileGO);
        NetworkServer.Spawn(projectileGO);
    }

    [ClientCallback]
    void TellServerToSpawnProjectile(float damage, Vector3 direction, Vector3 firePoint, Quaternion rotation, float maxDistance)
    {
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
