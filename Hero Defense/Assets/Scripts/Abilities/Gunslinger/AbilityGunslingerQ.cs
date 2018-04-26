using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterEventController))]
public class AbilityGunslingerQ : NetworkBehaviour
{
    private Camera cam;

    public float abilityCooldown = 4.0f;
    private float currentCooldown = 0.0f;

    [Space]
    public GameObject previewPrefab;
    private GameObject previewGameObject;
    
    [Header("Projectile settings")]
    public GameObject projectilePrefab;

    public float projectileSpeed = 10.0f;   

    public float projectilePhysicalDamage = 100.0f;

    

    private bool isCasting = false;

    PlayerController pc;
    CharacterEventController cec;

    KeyCode abilityKey;


    // Use this for initialization
    void Start()
    {
        GetComponent<CharacterEventManager>().OnAbilityOne += Cast;        
        cam = Camera.main;

        cec = GetComponent<CharacterEventController>();
        pc = GetComponent<PlayerController>();

        abilityKey = cec.abilityOneKey;



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

    void Update()
    {
        if(!isCasting)
        {
            currentCooldown -= Time.deltaTime;
        }
        else
        {
            if (previewGameObject != null)
            {
                // Position und Rotation der Preview anpassen
                previewGameObject.transform.position = transform.position;
                previewGameObject.transform.rotation = Quaternion.AngleAxis(GetAngleFromDirection(), Vector3.up);
            }

            if(!skipFrame)
            {
                if (Input.GetMouseButtonDown(1))        // RightClick
                {
                    CancelCast();
                }

                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(abilityKey))        // LeftClick or AbilityKey
                {
                    ShootProjectile(GetDirectionVectorBetweenPlayerAndMouse());
                    isCasting = false;
                    pc.isCasting = false;
                    cec.isCasting = false;
                    Destroy(previewGameObject);
                }
            }
            skipFrame = false;            
        }
    }

    public void Cast()
    {
        
        if (currentCooldown <= 0)
        {
            //Debug.Log("Cast(): GunslingerQ");
            pc.isCasting = true;
            cec.isCasting = true;
            ShowPreview();
            skipFrame = true;
        }
    }

    public void CancelCast()
    {
        isCasting = false;
        pc.isCasting = false;
        cec.isCasting = false;

        Destroy(previewGameObject);
    }


    private void ShowPreview()
    {
        isCasting = true;

        previewGameObject = Instantiate(previewPrefab);
    }

    private void ShootProjectile(Vector3 direction)
    {
     //   Debug.Log("ShootProjectile():");

        currentCooldown = abilityCooldown;
        
        if(isServer)
        {
            CmdSpawnProjectileOnServer(projectilePhysicalDamage,direction);
        } else
        {
            TellServerToSpawnProjectile(projectilePhysicalDamage, direction);
        }
        
    }

    private Vector3 GetDirectionVectorBetweenPlayerAndMouse()
    {
        Vector3 playerPos = transform.position;

        Vector3 mousePos = new Vector3();
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000))
        {
            mousePos = hit.point;
        }

        Vector3 direction = Vector3.Normalize(mousePos - playerPos);
        
        return direction;
    }

    private float GetAngleFromDirection()
    {
        float angle = 0.0f;
        Vector3 direction = GetDirectionVectorBetweenPlayerAndMouse();
        angle = Vector3.Angle(Vector3.forward, direction);

        if (direction.x < 0.0f)
        {
            angle = 360.0f-angle;
        }
        //Debug.Log("angle = " + angle);
        return angle;
    }

    [Command]
    void CmdSpawnProjectileOnServer(float damage, Vector3 direction)
    {

        Vector3 firePoint = previewGameObject.GetComponent<CalcDistanceFromStartToEnd>().GetStartPos();

        GameObject projectileGO = (GameObject)Instantiate(projectilePrefab, firePoint, previewGameObject.transform.rotation);
        GunslingerQProjectile projectile = projectileGO.GetComponent<GunslingerQProjectile>();

        if (projectile != null)
        {             
            projectile.SetDirection(direction);

            float maxDistance = previewGameObject.GetComponent<CalcDistanceFromStartToEnd>().GetDistance();
            
            projectile.SetMaxDistance(maxDistance);
            projectile.speed = projectileSpeed;
            projectile.damage = damage;
        }

        //Debug.Log(projectileGO);
        NetworkServer.Spawn(projectileGO);
    }

    [ClientCallback]
    void TellServerToSpawnProjectile(float damage, Vector3 direction)
    {
        if (!isServer)
        {
            CmdSpawnProjectileOnServer(damage, direction);
        }
    }
}
