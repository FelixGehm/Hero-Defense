




// used ranged auto attacks as well as the mortar ability
public class SpecialMantisController : EnemyController
{
    private float attackRange;
    public float mortarRange;
    private MantisCombat mCombat;
    public override void Awake()
    {
        base.Awake();
        mCombat = GetComponent<MantisCombat>();
        attackRange = GetComponent<EnemyStats>().attackRange.GetValue();
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
            target = GetTarget();
        }

        //Moving to Player and attack
        if (!mCombat.isAttacking && !mCombat.isFiringMortar)
            agent.SetDestination(target.position);

        if (distanceToTarget <= attackRange)
        {
            agent.ResetPath();
            if (!mCombat.isFiringMortar)
            {
                CharacterStats targetStats = target.GetComponent<CharacterStats>();

                if (targetStats != null)
                {
                    mCombat.Attack(targetStats);
                    //combat.MortarAttack(target.position);
                }

                FaceTarget(target);
            }

        }

        if (distanceToTarget <= mortarRange && !mCombat.isAttacking && mCombat.IsMortarReady)
        {
            agent.ResetPath();
            CharacterStats targetStats = target.GetComponent<CharacterStats>();

            if (targetStats != null)
            {
                mCombat.DoubleMortarAttack(target.position);
            }

            FaceTarget(target);
        }

        CheckIfStillInCombat();
    }
}
