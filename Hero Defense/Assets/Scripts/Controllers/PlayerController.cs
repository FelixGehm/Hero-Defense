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
    PlayerCombat combat;
    EnemyStats enemyStats;
    CharacterEventManager playerEventManager;

    //public bool isDead;

    public bool IsDead { get; private set; }


    public override void Awake()
    {
        base.Awake();

        motor = GetComponent<PlayerMotor>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        cam = Camera.main;

        playerEventManager = GetComponent<CharacterEventManager>();
    }

    public void SetupCam()
    {
        cam = Camera.main;
        cam.GetComponent<CameraController>().SetLookAt(transform);
    }

   

    private bool isCasting = false;
    public bool IsCasting
    {
        get
        {
            return isCasting;
        }
        set
        {
            //Debug.Log("IsCasting() value =" + value);
            if (value == true)
            {
                combat.CancelAttack();
                RemoveFocus();
            }

            isCasting = value;
        }
    }

    void Update()
    {


        //test delete after
        if (Input.GetKeyDown(KeyCode.L))
        {
            stats.TakeTrueDamage(1000);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            combat.TestKill();
        }
        //test end



        OnFocusNull?.Invoke();

        if (myStatuses.Contains(Status.stunned) || IsDead)
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
                    enemyStats = focus.GetComponent<EnemyStats>();
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

    public event System.Action OnPlayerKilled;
    public event System.Action OnPlayerRevived;

    public void KillPlayer()
    {
        if (combat.isAttacking)
            combat.CancelAttack();

        RemoveFocus();
        motor.MoveToPoint(transform.position);

        IsDead = true;

        if (OnPlayerKilled != null)
            OnPlayerKilled();
    }

    public void RevivePlayer()
    {
        IsDead = false;

        stats.SyncedCurrentHealth = stats.maxHealth.GetValue();

        if (OnPlayerRevived != null)
            OnPlayerRevived();
    }



    #region CrowdControllable
    protected override IEnumerator GetTauntedCo(Transform tauntTarget, float duration)
    {
        Debug.Log("NOT WORKING RIGHT NOW");

        focus = tauntTarget.GetComponent<Interactable>();

        myStatuses.Add(Status.taunted);
        yield return new WaitForSeconds(duration);
        //throw new System.NotImplementedException();

        myStatuses.Remove(Status.taunted);
    }

    protected override IEnumerator GetStunnedCo(float duration)
    {
        if (combat.isAttacking)
            combat.CancelAttack();

        //cancel all abilities
        RemoveFocus();     //ja oder nein?


        while (myStatuses.Contains(Status.stunned))
        {
            myStatuses.Remove(Status.stunned);
        }

        myStatuses.Add(Status.stunned);

        motor.MoveToPoint(transform.position);

        yield return new WaitForSeconds(duration);
        myStatuses.Remove(Status.stunned);
    }

    protected override IEnumerator GetSilencedCo(float duration)
    {
        playerEventManager.IsSilenced = true;
        yield return new WaitForSeconds(duration);
        playerEventManager.IsSilenced = false;
    }

    protected override IEnumerator GetBlindedCo(float duration)
    {
        combat.isBlinded = true;
        yield return new WaitForSeconds(duration);
        combat.isBlinded = false;
    }

    protected override IEnumerator GetCrippledCo(float duration, float percent)
    {
        float oldSpeed = motor.GetAgentSpeed();

        motor.SetAgentSpeed(percent * oldSpeed);

        yield return new WaitForSeconds(duration);

        motor.SetAgentSpeed(oldSpeed);
    }

    protected override IEnumerator GetBleedingWoundCo(int ticks, float percentPerTick)
    {
        yield return new WaitForSeconds(1.0f);

        float damageDealed = stats.SyncedCurrentHealth * percentPerTick;
        stats.TakeTrueDamage(damageDealed);

        ticks -= 1;
        if (ticks > 0)
        {
            StartCoroutine(GetBleedingWoundCo(ticks, percentPerTick));
        }
    }

    #endregion
}
