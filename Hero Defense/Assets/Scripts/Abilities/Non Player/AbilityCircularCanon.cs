using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AbilityCircularCanon : NetworkBehaviour
{
    [Header("Setup Fiels")]
    public Transform CanonsTransform;
    public Transform[] firePoints;

    [Header("General Settings")]
    public float delayToFirstShot;
    public float delayBetweenShots;
    public float delayToNextRotation;
    public float delayToNextAction;
    public int cycles;
    public float rotationSpeed;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float damage;
    public float force;
    public float projectileLifetime;

    [HideInInspector]
    public bool isInAbility = false;

    private bool isShooting = false;

    private bool isRotating = false;


    public void Execute()
    {
        isInAbility = true;
        isRotating = true;
        currentAngle = 0;
        currentCycle = 0;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isInAbility) return;

        if (isRotating) RotateAndShoot();

    }

    private float wantedAngle = 0;
    private float currentAngle = 0;
    private bool isAngleCalculated = false;
    private void RotateAndShoot()
    {
        if (!isAngleCalculated)
        {
            wantedAngle = Random.Range(15, 45);
            //Debug.Log("wanted angle: " + wantedAngle);
            isAngleCalculated = true;
        }
        //Debug.Log("current angle: " + currentAngle);
        currentAngle += Time.deltaTime * rotationSpeed;
        CanonsTransform.Rotate(0, Time.deltaTime * rotationSpeed, 0);
        if (currentAngle >= wantedAngle)
        {
            isRotating = false;
            currentAngle = 0;
            isAngleCalculated = false;
            StartCoroutine(ShootProjectilesFromAllCanons());
        }
    }

    private int currentCycle = 0;
    private IEnumerator ShootProjectilesFromAllCanons()
    {
        isShooting = true;
        yield return new WaitForSeconds(delayToFirstShot);
        foreach (Transform fp in firePoints)
        {
            ShootProjectile(fp);
            yield return new WaitForSeconds(delayBetweenShots);
        }
        yield return new WaitForSeconds(delayToNextRotation - delayBetweenShots);
        isShooting = false;
        //isRotating = true;
        currentCycle++;
        if (currentCycle == cycles)
        {
            yield return new WaitForSeconds(delayToNextAction - delayToNextRotation - delayBetweenShots);
            isInAbility = false;
        }
        else
        {
            isRotating = true;
        }
    }

    private void ShootProjectile(Transform firePoint)
    {
        if (isServer)
        {
            CmdSpawnProjectileOnServer(damage, firePoint.position, firePoint.rotation, firePoint.forward);
        }
    }

    [Command]
    void CmdSpawnProjectileOnServer(float damage, Vector3 position, Quaternion rotation, Vector3 direction)
    {

        GameObject projectileGO = Instantiate(projectilePrefab, position, rotation);
        projectileGO.GetComponent<CanonProjectile>().Init(damage, force, direction, projectileLifetime);
        NetworkServer.Spawn(projectileGO);
    }

}
