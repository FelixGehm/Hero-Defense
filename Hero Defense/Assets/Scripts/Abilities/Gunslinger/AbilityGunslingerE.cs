using UnityEngine;
using System.Collections;

public class AbilityGunslingerE : AbilityBasic
{

    public LayerMask rightClickMask;

    public float maxRange = 4.0f;

    public GameObject previewPrefab;
    private GameObject previewGameObject;

    public GameObject maxRangePrefab;
    private GameObject maxRangeGameObject;

    private Camera cam;
    private PlayerController pc;
    private CharacterEventController cec;
    private PlayerMotor motor;

    KeyCode abilityKey;


    // Use this for initialization
    void Start()
    {
        GetComponent<CharacterEventManager>().OnAbilityThree += Cast;
        cam = Camera.main;

        cec = GetComponent<CharacterEventController>();
        pc = GetComponent<PlayerController>();
        motor = GetComponent<PlayerMotor>();

        abilityKey = cec.abilityThreeKey;




    }

    bool skipFrame = false;

    protected override void Update()
    {
        base.Update();
        
        if (isLocalPlayer && previewGameObject != null && maxRangeGameObject != null)
        {
            Vector3 targetPos = GetMousePosOnWorld();

            if(GetDistanceBetweenPlayerAndTargetPos(targetPos) > maxRange)
            {
                Vector3 direction = Vector3.Normalize(targetPos - transform.position);

                targetPos = transform.position + maxRange * direction;
            }

            previewGameObject.transform.position = targetPos;

            maxRangeGameObject.transform.position = transform.position;
        }

        

        if (!skipFrame)
        {
            if (Input.GetMouseButtonDown(1) && !isAnimating)        // RightClick
            {
                CancelCast();
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(abilityKey))        // LeftClick or AbilityKey
            {
                motor.MoveToPoint(transform.position);
                //StartCoroutine(ShootProjectile(GetDirectionVectorBetweenPlayerAndMouse()));
            }
        }
        skipFrame = false;

    }

    protected override void Cast()
    {
        if (isLocalPlayer && currentCooldown <= 0)
        {
            pc.isCasting = true;
            cec.isCasting = true;
            skipFrame = true;

            SpawnPreview();
        }
    }

    private void CancelCast()
    {
        isCasting = false;
        pc.isCasting = false;
        cec.isCasting = false;

        Destroy(previewGameObject);
        Destroy(maxRangeGameObject);
    }


    private void SpawnPreview()
    {
        isCasting = true;

        previewGameObject = Instantiate(previewPrefab);

        maxRangeGameObject = Instantiate(maxRangePrefab);

        maxRangeGameObject.transform.localScale += (new Vector3(1, 0, 1)* (maxRange+4));
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

}
