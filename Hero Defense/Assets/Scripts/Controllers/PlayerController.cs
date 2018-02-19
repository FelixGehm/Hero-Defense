using UnityEngine.EventSystems;
using UnityEngine;


[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    public Interactable focus;

    public LayerMask rightClickMask;

    Camera cam;
    PlayerMotor motor;
    PlayerStats stats;
    CharacterCombat combat;

    CharacterStats enemyStats;

    bool isWaiting = false;

    // Use this for initialization
    void Start()
    {
        cam = Camera.main;
        motor = GetComponent<PlayerMotor>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<CharacterCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        //no controlls if pointer is over ui
        if (EventSystem.current.IsPointerOverGameObject())
            return;


        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, rightClickMask))
            {
                if (hit.collider.tag == "Walkable")
                {
                    motor.MoveToPoint(hit.point);
                    RemoveFocus();
                }
                else
                {
                    Interactable interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable != null) SetFocus(interactable);
                }

                if (focus != null && focus.GetType() == typeof(Enemy))
                {
                    enemyStats = focus.GetComponent<CharacterStats>();   
                }


            }            
        }

        if (focus != null && focus.GetType() == typeof(Enemy))
        {

            if (!isWaiting)
            {
                // Entfernung Player und Gegner
                float distance = Vector3.Distance(focus.transform.position, transform.position);

                if (distance <= stats.attackRange.GetValue())
                {

                    Debug.Log("Enemy in Range");
                    isWaiting = true;

                    motor.PauseFollowTarget();
                    combat.Attack(enemyStats);

                    if (focus != null)
                        //Debug.Log("Start Coroutine");
                        StartCoroutine(ResumeFollow(stats.attackSpeed.GetValue() / 100));
                }
            }
        }


    }

    System.Collections.IEnumerator ResumeFollow(float delay)
    {
        Debug.Log("Coroutine: resumeFollow()");
        yield return new WaitForSeconds(delay);

        motor.ContinueFollowTarget();
        isWaiting = false;
    }

    void SetFocus(Interactable newFocus)
    {
        if (newFocus != focus)
        {
            if (focus != null)
                focus.OnDefocused();

            focus = newFocus;
            motor.FollowTarget(newFocus);
        }


        newFocus.OnFocused(transform);

    }

    void RemoveFocus()
    {
        if (focus != null)
            focus.OnDefocused();

        focus = null;
        motor.StopFollowingTarget();
    }
}
