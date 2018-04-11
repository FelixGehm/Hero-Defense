using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent),typeof(PlayerController))]
public class AbilityDashStun : MonoBehaviour
{

    private NavMeshAgent agent;
    private PlayerController pc;
    private Camera cam;

    public float abilityCooldown = 4.0f;
    float currentCooldown = 0.0f;

    public float dashDistance = 6.0f;
    public float dashTime = 0.5f;

    private float dashSpeed; // gets calculated

    public float stunRange = 10.0f;
    public float stunDuration = 2.0f;

    // Use this for initialization
    void Start()
    {
        GetComponent<CharacterEventManager>().OnAbilityTwo += Cast;

        agent = GetComponent<NavMeshAgent>();

        pc = GetComponent<PlayerController>();

        cam = Camera.main;
    }

    void Update()
    {
        currentCooldown -= Time.deltaTime;

        // Debug.Log(GetDirectionVectorBetweenPlayerAndMouse());
    }

    public void Cast()
    {
        Debug.Log("Cast(): DashStun");
        if (currentCooldown <= 0)
        {
            currentCooldown = abilityCooldown;

            //Dash();
            //Stun();

            PlayerController pc = GetComponent<PlayerController>();
            StartCoroutine(pc.GetStunned(stunDuration));
        }
    }

    private void Dash()
    {
        // calc Speed
        float dashSpeed = dashDistance / dashTime;
        Vector3 endPos = transform.position + (GetDirectionVectorBetweenPlayerAndMouse() * dashDistance);

        agent.SetDestination(endPos);
        StartCoroutine(ResetSpeedAfterTime(agent.speed, dashTime));
        agent.speed = dashSpeed;

        pc.isWaiting = true;        
    }


    private IEnumerator ResetSpeedAfterTime(float oldSpeed, float delay)
    {
        yield return new WaitForSeconds(delay);

        agent.speed = oldSpeed;
        pc.isWaiting = false;
    }

    private void Stun()
    {

        List<GameObject> closeEnemys = FindEnemysInRange(stunRange);
        foreach (GameObject enemy in closeEnemys)
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();

            //StartCoroutine(ec.GetStunned(stunDuration));

            //StartCoroutine(ec.GetBleedingWound(3, 0.05f));

            
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

    /// <summary>
    /// Sucht nach Gameobjekten in der angegebenen Reichweite mit dem Tag "Enemy" und gibt eine List mit den Gameobjekten zurück.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    private List<GameObject> FindEnemysInRange(float range)
    {
        List<GameObject> closeEnemys = new List<GameObject>();

        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log("enemys with tag = " + enemys.Length);

        // check distance and add to List if in range
        foreach (GameObject enemy in enemys)
        {
            float distance = Vector3.Distance(enemy.transform.position, transform.position);

            Debug.Log("enemy distance = " + distance);
            if (distance <= range)
            {
                closeEnemys.Add(enemy);
                Debug.Log("enemy added to list");
            }
        }
        Debug.Log("FindEnemysInRange(): ");
        Debug.Log("closeEnemys.Count = " + closeEnemys.Count);

        return closeEnemys;
    }

    /// <summary>
    /// Draw Dash-Distance
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashDistance);
    }
}
