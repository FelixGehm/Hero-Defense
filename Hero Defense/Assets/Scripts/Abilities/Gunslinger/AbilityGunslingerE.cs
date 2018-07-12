using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class AbilityGunslingerE : AbilityBasic
{

    public LayerMask rightClickMask;

    public float maxThrowRange = 4.0f;

    public GameObject previewPrefab;
    private GameObject previewGameObject;

    public GameObject maxRangePrefab;
    private GameObject maxRangeGameObject;

    public GameObject grenadePrefab;
    public Transform firePoint;

    
    KeyCode abilityKey;


    [Space]
    [Header("Grenade settings")]
    public float peakHeight;
    public float explosionRadius;
    public float explosionDamage;
    public float stunDuration;


    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        GetComponent<CharacterEventManager>().OnAbilityThree += Cast;


        abilityKey = characterEventController.abilityThreeKey;        

    }

    bool skipFrame = false;

    protected override void Update()
    {
        base.Update();

        if (isLocalPlayer && previewGameObject != null && maxRangeGameObject != null)
        {
            Vector3 landingPos = GetMousePosOnWorld();

            if (GetDistanceBetweenPlayerAndTargetPos(landingPos) > maxThrowRange)
            {
                Vector3 direction = Vector3.Normalize(landingPos - transform.position);

                landingPos = transform.position + maxThrowRange * direction;
                landingPos.y = 0.1f;
            }

            //previewGameObject.transform.position = landingPos;

            previewGameObject.transform.position = new Vector3(landingPos.x, 0.1f, landingPos.z);

            maxRangeGameObject.transform.position = transform.position;



            if (!skipFrame && isCasting)
            {
                if (Input.GetMouseButtonDown(1) && !isAnimating)        // RightClick
                {
                    CancelCast();
                }

                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(abilityKey))        // LeftClick or AbilityKey
                {
                    playerMotor.MoveToPoint(transform.position);
                    StartCoroutine(ThrowGrenade(landingPos));
                }
            }
            skipFrame = false;
        }
    }

    protected override void Cast()
    {
        if (isLocalPlayer && currentCooldown <= 0)
        {
            
            playerController.IsCasting = true;
            characterEventController.isCasting = true;
            skipFrame = true;

            SpawnPreview();
        }
    }

    private void CancelCast()
    {
        IsCasting(false);

        Destroy(previewGameObject);
        Destroy(maxRangeGameObject);
    }


    private void SpawnPreview()
    {
        isCasting = true;


        maxRangeGameObject = Instantiate(maxRangePrefab);
        previewGameObject = Instantiate(previewPrefab);        

        //maxRangeGameObject.transform.localScale += (new Vector3(1,1,0) * maxThrowRange);
    }

    private IEnumerator ThrowGrenade(Vector3 landingPoint)
    {   
        TriggerAnimation();

        transform.LookAt(previewGameObject.transform);   // rotate Player in correct direction

        Destroy(maxRangeGameObject);
        Destroy(previewGameObject);

        //Debug.Log("AnimationTime!");
        isAnimating = true;

        yield return new WaitForSeconds(abilityCastTime);
        isAnimating = false;

        //Debug.Log("AnimationTime over!");

        IsCasting(false);

        currentCooldown = abilityCooldown;

        if (isServer)
        {
            CmdSpawnGrenadeOnServer(firePoint.position, landingPoint, peakHeight, explosionRadius, explosionDamage, stunDuration);
        }
        else
        {
            TellServerToSpawnGrenade(firePoint.position, landingPoint, peakHeight, explosionRadius, explosionDamage, stunDuration);
        }
    }

    [Command]
    void CmdSpawnGrenadeOnServer(Vector3 start, Vector3 end, float height, float range, float damage, float stunTime)
    {
        GameObject grenadeGO = Instantiate(grenadePrefab, firePoint.position, transform.rotation);
        NetworkServer.Spawn(grenadeGO);

        GunslingerEGrenade grenadeScript = grenadeGO.GetComponent<GunslingerEGrenade>();
        if (grenadeScript != null)
        {
            grenadeScript.Init(start, end, height, range, damage, stunTime, this.transform);
        }       

        
    }

    [ClientCallback]
    void TellServerToSpawnGrenade(Vector3 start, Vector3 end, float height, float range, float damage, float stunTime)
    {
        if (!isServer)
        {
            CmdSpawnGrenadeOnServer(start, end, height, range, damage, stunTime);
        }
    }

    protected Vector3 GetMousePosOnWorld()
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
        return angle;
    }

    private float GetDistanceBetweenPlayerAndTargetPos(Vector3 targetPos)
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        return distance;
    }

    #region Editor
    private void OnDrawGizmosSelected()
    {
        if (maxThrowRange <= 0) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, (float)maxThrowRange);
    }
    #endregion

}
