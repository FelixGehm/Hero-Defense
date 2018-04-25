using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : CrowdControllable
{
    public Interactable focus;
    private Interactable oldFocus;

    public event System.Action OnFocusNull;

    public LayerMask rightClickMask;

    Camera cam;
    PlayerMotor motor;
    PlayerStats stats;
    CharacterCombat combat;
    CharacterStats enemyStats;
    CharacterEventManager playerEventManager;

    public override void Awake()
    {
        base.Awake();

        motor = GetComponent<PlayerMotor>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<CharacterCombat>();
        cam = Camera.main;

        playerEventManager = GetComponent<CharacterEventManager>();
    }

    public void SetupCam()
    {
        cam = Camera.main;
        cam.GetComponent<CameraController>().SetLookAt(transform);
    }


    public bool isCasting = false;
    
    void Update()
    {


        if (OnFocusNull != null)
            OnFocusNull();

        if (myStatuses.Contains(Status.stunned))
        {
            return;
        }

        //Das muss hier oben stehen, da sonst vorher returned werden könnte
        KeepTrackOfTarget();


        //no controlls if pointer is over ui
        if (EventSystem.current.IsPointerOverGameObject())
            return;
        /*
        //no controlls if player is taunted
        if (myStatuses.Contains(Status.taunted))
            return;
        */

        if (Input.GetMouseButtonDown(1) && !isCasting)
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
                        //combat.isAttacking = false;
                        motor.MoveToPoint(hit.point);
                        RemoveFocus();
                    }
                }
                else
                {
                    Interactable interactable = hit.collider.GetComponent<Interactable>();
                    if (interactable != null) SetFocus(interactable);

                    if (combat.isAttacking && focus != oldFocus) combat.CancelAttack();

                    oldFocus = focus;
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
        //Debug.Log("KeepTrackOfTarget()");
        if (focus != null && focus.GetType() == typeof(Enemy))
        {
            float distance = Vector3.Distance(focus.transform.position, transform.position);    // Entfernung Player und Gegner

            if (distance <= stats.attackRange.GetValue() && combat.GetAttackCooldown() <= 0)
            {
                combat.Attack(enemyStats);
                wasAttacking = true;
                motor.PauseFollowTarget();
            }
            else if (distance <= stats.attackRange.GetValue() && combat.GetAttackCooldown() > 0)
            {
                motor.PauseFollowTarget();
            }
            else if (combat.GetAttackCooldown() <= 0)
            {
                motor.ContinueFollowTarget();
            }
        }
        //setzt die destination des agent zurück sobald der Gegner tot ist.
        if (wasAttacking && focus == null)
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

    #region CrowdControllable
    public override IEnumerator GetTaunted(Transform tauntTarget, float duration)
    {
        Debug.Log("NOT WORKING RIGHT NOW");

        focus = tauntTarget.GetComponent<Interactable>();

        myStatuses.Add(Status.taunted);
        yield return new WaitForSeconds(duration);
        //throw new System.NotImplementedException();

        myStatuses.Remove(Status.taunted);
    }

    public override IEnumerator GetStunned(float duration)
    {
        if (combat.isAttacking)
            combat.CancelAttack();

        //cancel all abilities
        RemoveFocus();     //ja oder nein?

        myStatuses.Add(Status.stunned);

        motor.MoveToPoint(transform.position);

        yield return new WaitForSeconds(duration);
        myStatuses.Remove(Status.stunned);
    }

    public override IEnumerator GetSilenced(float duration)
    {
        playerEventManager.IsSilenced = true;
        yield return new WaitForSeconds(duration);
        playerEventManager.IsSilenced = false;
    }

    public override IEnumerator GetBlinded(float duration)
    {
        combat.isBlinded = true;
        yield return new WaitForSeconds(duration);
        combat.isBlinded = false;
    }

    public override IEnumerator GetCrippled(float duration, float percent)
    {
        float oldSpeed = motor.GetAgentSpeed();

        motor.SetAgentSpeed(percent * oldSpeed);

        yield return new WaitForSeconds(duration);

        motor.SetAgentSpeed(oldSpeed);
    }

    public override IEnumerator GetBleedingWound(int ticks, float percentPerTick)
    {
        yield return new WaitForSeconds(1.0f);

        float damageDealed = stats.CurrentHealth * percentPerTick;
        stats.TakeTrueDamage(damageDealed);

        ticks -= 1;
        if (ticks > 0)
        {
            StartCoroutine(GetBleedingWound(ticks, percentPerTick));
        }
    }

    #endregion
}
