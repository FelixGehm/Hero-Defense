
// only uses a different Combat Component than the EnemyController
public class MantisController : EnemyController
{

    public override void Awake()
    {
        base.Awake();
        combat = GetComponent<MantisCombat>();
        agent.stoppingDistance = GetComponent<EnemyStats>().attackRange.GetValue();
    }

    protected override void Update()
    {
        if (myStatuses.Contains(Status.stunned))
        {
            // Tue nichts, solange bis der Stun vorbei ist.
            return;
        }

        if (!myStatuses.Contains(Status.taunted))
        {
            targetTransform = GetTarget();
        }

        //Moving to Player and attack
        if (!combat.isAttacking)
            agent.SetDestination(targetTransform.position);

        if (distanceToTarget <= agent.stoppingDistance)
        {
            CharacterStats targetStats = targetTransform.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                combat.Attack(targetStats);
            }

            FaceTarget(targetTransform);
        }

        CheckIfStillInCombat();
    }
}
