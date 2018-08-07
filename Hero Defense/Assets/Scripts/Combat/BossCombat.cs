using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BossCombat : CharacterCombat
{

    [HideInInspector]
    public bool isFiringMortar = false;
    private AbilityMortar mortar;

    private AbilityBodySlam bodySlam;

    private AbilityCircularCanon canons;

    [Header("Mortar Settings")]
    public Transform firePointMortar;
    public int nrOfShots = 5;
    public float mortarDelay = 0.3f;
    public float delayToNextShot = 0.15f;
    public float mortarDelayToNextAA = 2;
    public float mortarCooldown = 5;
    private float currentMortarCooldown = 0;

    [Header("Body Slam Settings")]
    public float bodySlamCooldown = 10;
    private float currentBodySlamCooldown = 0;

    [Header("Circular Canon Settings")]
    public float canonsCooldown = 10;
    private float currentCanonsCooldown = 0;

    private EnemyAnimator anim;

    public override void Start()
    {
        base.Start();
        mortar = GetComponent<AbilityMortar>();
        bodySlam = GetComponent<AbilityBodySlam>();
        canons = GetComponent<AbilityCircularCanon>();
        anim = GetComponent<EnemyAnimator>();
    }

    protected override void Update()
    {
        base.Update();
        currentMortarCooldown -= Time.deltaTime;
        currentBodySlamCooldown -= Time.deltaTime;
        currentCanonsCooldown -= Time.deltaTime;
        //Debug.Log(currentBodySlamCooldown);
    }

    public void MortarAttack(Vector3 targetPosition)
    {
        if (currentMortarCooldown <= 0)
        {
            StartCoroutine(ShootMortar(targetPosition, mortarDelay, delayToNextShot, nrOfShots));
            currentMortarCooldown = mortarCooldown;
        }
    }

    protected IEnumerator ShootMortar(Vector3 targetPosition, float delay, float delayToNextShot, int nrOfShots)
    {
        mortar.SpawnPreview(targetPosition);
        //anim.StartMortarAnimation();
        isFiringMortar = true;
        yield return new WaitForSeconds(delay);
        for (int i = 1; i <= nrOfShots; i++)
        {
            mortar.Fire(firePointMortar.position, targetPosition);
            if (i < nrOfShots) yield return new WaitForSeconds(delayToNextShot);
        }

        yield return new WaitForSeconds(mortarDelayToNextAA);
        mortar.DestroyPreview();
        isFiringMortar = false;
    }

    public bool IsMortarReady
    {
        get
        {
            return currentMortarCooldown <= 0;
        }
    }

    public void BodySlam(Transform target)
    {
        if (currentBodySlamCooldown <= 0)
        {
            bodySlam.Execute(target);
            currentBodySlamCooldown = bodySlamCooldown;
        }
    }

    public bool IsBodySlamReady
    {
        get
        {
            return currentBodySlamCooldown <= 0;
        }
    }

    public bool IsBodySlamming
    {
        get
        {
            return bodySlam.isInAbility;
        }
    }

    public void FireCanons()
    {
        if (currentCanonsCooldown <= 0)
        {
            canons.Execute();
            currentCanonsCooldown = canonsCooldown;
        }
    }

    public bool IsCanonsReady
    {
        get
        {
            return currentCanonsCooldown <= 0;
        }
    }

    public bool IsFiringCanons
    {
        get
        {
            return canons.isInAbility;
        }
    }














    /*
    public void MortarAttack(GameObject target)
    {
        if (currentMortarCooldown <= 0)
        {
            StartCoroutine(ShootMortar(target, mortarDelay, delayToNextShot, nrOfShots));
            currentMortarCooldown = mortarCooldown;
        }
    }
    */

    /*
    protected IEnumerator ShootMortar(GameObject target, float delay, float delayToNextShot, int nrOfShots)
    {
        mortar.SpawnPreview(target.transform.position);
        //anim.StartMortarAnimation();
        isFiringMortar = true;
        yield return new WaitForSeconds(delay);
        for (int i = 1; i <= nrOfShots; i++)
        {
            mortar.Fire(firePointMortar.position, target.transform.position);
            if (i < nrOfShots) yield return new WaitForSeconds(delayToNextShot);
        }

        yield return new WaitForSeconds(mortarDelayToNextAA);
        mortar.DestroyPreview();
        isFiringMortar = false;
    }
    */
}
