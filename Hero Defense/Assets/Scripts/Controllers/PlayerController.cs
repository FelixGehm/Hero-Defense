using UnityEngine.EventSystems;
using UnityEngine;


[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    public Interactable focus;

    public LayerMask rightClickMask;

    Camera cam;
    PlayerMotor motor;

    // Use this for initialization
    void Start()
    {
        cam = Camera.main;
        motor = GetComponent<PlayerMotor>();
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


            }
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
    }
}
