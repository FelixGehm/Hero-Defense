using UnityEngine.EventSystems;
using UnityEngine;



[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    public Interactable focus;
    private Vector3? destination;       //nullable Vector3

    public event System.Action OnFocusNull;

    public LayerMask rightClickMask;

    Camera cam;
    PlayerMotor motor;
    PlayerStats stats;
    CharacterCombat combat;

    CharacterStats enemyStats;

    public bool isWaiting = false;

    // Use this for initialization
    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<CharacterCombat>();
        cam = Camera.main;
    }

    public void SetupCam()
    {
        cam = Camera.main;
        cam.GetComponent<CameraController>().SetLookAt(transform);
    }

    // Update is called once per frame
    void Update()
    {
        if (focus == null && OnFocusNull != null)
        {
            OnFocusNull();
        }

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
                    if (!isWaiting)
                    {
                        motor.MoveToPoint(hit.point);
                        RemoveFocus();
                    }
                    else
                    {
                        destination = hit.point;
                    }
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
                    //Debug.Log("Enemy in Range");
                    isWaiting = true;

                    motor.PauseFollowTarget();

                    combat.Attack(enemyStats);


                    if (focus != null)
                    {
                        StartCoroutine(ResumeFollow(1.0f / stats.attackSpeed.GetValue()));
                    }
                }
            }
        }
    }




    System.Collections.IEnumerator ResumeFollow(float delay)
    {
        //Debug.Log("Coroutine: resumeFollow()");
        yield return new WaitForSeconds(delay);

        if (destination != null)
        {
            RemoveFocus();
            motor.MoveToPoint(destination);
            destination = null;
        }
        else
        {
            motor.ContinueFollowTarget();
        }
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
