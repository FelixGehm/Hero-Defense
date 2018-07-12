



// only uses a different Combat Component than the EnemyController
public class MantisController : EnemyController
{
    public override void Awake()
    {
        base.Awake();
        combat = GetComponent<MantisCombat>();
    }
}
