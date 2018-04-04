using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AbilityDashStun : MonoBehaviour
{

    private NavMeshAgent agent;
    private Camera cam;

    public float abilityCooldown = 4.0f;
    float currentCooldown = 0.0f;

    public float dashDistance = 6.0f;
    public float dashTime = 0.5f;

    private float dashSpeed; // gets calculated

    // Use this for initialization
    void Start()
    {
        GetComponent<CharacterEventManager>().OnAbilityTwo += Cast;

        agent = GetComponent<NavMeshAgent>();

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

            Dash();
            Stun();
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

        //transform.position += dashSpeed * GetDirectionVectorBetweenPlayerAndMouse();
    }


    private IEnumerator ResetSpeedAfterTime(float oldSpeed, float delay)
    {
        yield return new WaitForSeconds(delay);

        agent.speed = oldSpeed;
    }

    private void Stun()
    {

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
}
