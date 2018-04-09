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
    CharacterAnimator animator;
    CharacterStats enemyStats;

    public bool isWaiting = false;

    // Use this for initialization
    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<CharacterCombat>();
        animator = GetComponent<CharacterAnimator>();
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
        //Debug.Log(isWaiting);
        if (OnFocusNull != null)
            OnFocusNull();


        //Das muss hier oben stehen, da sonst vorher returned werden könnte
        KeepTrackOfTarget();


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
                    if (!combat.isAttacking)
                    {
                        motor.MoveToPoint(hit.point);
                        RemoveFocus();
                    }
                    else //cancel Auto attack
                    {
                        combat.CancelAttack();
                        combat.isAttacking = false;
                        motor.MoveToPoint(hit.point);
                        RemoveFocus();
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
    }
    bool wasAttacking = false;
    private void KeepTrackOfTarget()
    {
        
        if (focus != null && focus.GetType() == typeof(Enemy))
        {
            // Entfernung Player und Gegner
            float distance = Vector3.Distance(focus.transform.position, transform.position);

            if (distance <= stats.attackRange.GetValue())
            {
                combat.Attack(enemyStats);
                wasAttacking = true;
                motor.PauseFollowTarget();
            }
            else if (!animator.IsInAttackAnimation() && focus != null)
            {
                motor.ContinueFollowTarget();
            }

        }
        //setzt die destination des agent zurück sobald der Gegner tot ist.
        if(wasAttacking && focus == null)
        {
            motor.MoveToPoint(transform.position);
            wasAttacking = false;
        }
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
        wasAttacking = false;
    }
}
