using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Non Player Ability
public class AbilityMortar : NetworkBehaviour
{
    [Header("Setup Fields")]
    public GameObject spawnIndicatorPrefab;
    private GameObject spawnIndicatorGO;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    //private GameObject projectileGO;
    public float damage;
    public float peakHeight;
    public float explosionRadius;

    public void SpawnPreview(Vector3 landingPoint)
    {
        if (isServer)
        {
            CmdSpawnIndicatorOnServer(landingPoint);
        }
        else
        {
            TellServerToSpawnIndicator(landingPoint);
        }
    }

    public void DestroyPreview()
    {
        if (isServer)
        {
            CmdDestroyIndicatorOnServer();
        }
        else
        {
            TellServerToDestroyIndicator();
        }
    }

    public void Fire(Vector3 startingPoint, Vector3 landingPoint)
    {
        if (isServer)
        {
            CmdSpawnProjectileOnServer(startingPoint, landingPoint, peakHeight, explosionRadius, damage);
        }
        else
        {
            TellServerToSpawnProjectile(startingPoint, landingPoint, peakHeight, explosionRadius, damage);
        }
    }
    #region Network Projectile Functions
    [Command]
    void CmdSpawnProjectileOnServer(Vector3 start, Vector3 end, float height, float range, float damage)
    {
        GameObject projectileGO = Instantiate(projectilePrefab, start, transform.rotation);
        NetworkServer.Spawn(projectileGO);

        projectileGO.GetComponent<MortarProjectile>().Init(start, end, height, range, damage);
    }

    [ClientCallback]
    void TellServerToSpawnProjectile(Vector3 start, Vector3 end, float height, float range, float damage)
    {
        if (!isServer)
        {
            CmdSpawnProjectileOnServer(start, end, height, range, damage);
        }
    }
    #endregion


    [Command]
    void CmdSpawnIndicatorOnServer(Vector3 spawnPosition)
    {
        spawnIndicatorGO = Instantiate(spawnIndicatorPrefab, spawnPosition, spawnIndicatorPrefab.transform.rotation);
        NetworkServer.Spawn(spawnIndicatorGO);
    }

    [ClientCallback]
    void TellServerToSpawnIndicator(Vector3 spawnPosition)
    {
        if (!isServer)
        {
            CmdSpawnIndicatorOnServer(spawnPosition);
        }
    }

    [Command]
    void CmdDestroyIndicatorOnServer()
    {
        NetworkServer.Destroy(spawnIndicatorGO);
    }

    [ClientCallback]
    void TellServerToDestroyIndicator()
    {
        CmdDestroyIndicatorOnServer();
    }
}
