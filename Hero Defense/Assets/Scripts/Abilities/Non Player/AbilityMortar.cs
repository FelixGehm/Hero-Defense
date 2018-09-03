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
    }

    public void DestroyPreview()
    {
        if (isServer)
        {
            CmdDestroyIndicatorOnServer();
        }
    }

    public void Fire(Vector3 startingPoint, Vector3 landingPoint)
    {
        if (isServer)
        {
            CmdSpawnProjectileOnServer(startingPoint, landingPoint, peakHeight, explosionRadius, damage);
        }
    }
    #region Network
    [Command]
    void CmdSpawnProjectileOnServer(Vector3 start, Vector3 end, float height, float range, float damage)
    {
        GameObject projectileGO = Instantiate(projectilePrefab, start, transform.rotation);
        NetworkServer.Spawn(projectileGO);

        projectileGO.GetComponent<MortarProjectile>().Init(start, end, height, range, damage);
    }



    [Command]
    void CmdSpawnIndicatorOnServer(Vector3 spawnPosition)
    {
        spawnIndicatorGO = Instantiate(spawnIndicatorPrefab, spawnPosition, spawnIndicatorPrefab.transform.rotation);
        NetworkServer.Spawn(spawnIndicatorGO);
    }

    [Command]
    void CmdDestroyIndicatorOnServer()
    {
        NetworkServer.Destroy(spawnIndicatorGO);
    }

    #endregion

    private void OnDestroy()
    {
        DestroyPreview();
        Destroy(spawnIndicatorGO);
    }
}
