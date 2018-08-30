using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class JungleCamp : MonoBehaviour
{
    public GameObject jungleMobPrefab;

    public float respawnTime = 30.0f;

    public float followRange = 4.0f;


    private void Awake()
    {
        if(jungleMobPrefab == null)
        {
            Debug.LogWarning("No jungleMobPrefab set on " + gameObject.name);
        }
    }

    // Draw FolowRadius in Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, followRange);
    }


    bool firstSpawnDone = false;
    private void Update()
    {
       // Debug.Log(NetworkServer.active);

        if(!firstSpawnDone && NetworkServer.active)
        {            
            Quaternion rot = transform.rotation;
            Vector3 pos = transform.position;

            GameObject enemy = Instantiate(jungleMobPrefab, pos, rot);
            enemy.GetComponent<JungleMobController>().homeTransform = this.transform;
            enemy.GetComponent<JungleMobController>().jungleCamp = this;
            enemy.GetComponent<JungleMobController>().maxDistanceToHome = followRange;
            NetworkServer.Spawn(enemy);

            firstSpawnDone = true;
        }
    }



    public void Respawn()
    {
        Debug.Log("Respawn()");
        StartCoroutine(Respawn(respawnTime, jungleMobPrefab));        
    }

    IEnumerator Respawn(float delay, GameObject prefabToSpawn)
    {
        Debug.Log("Respawning Jungle Mob with " + delay + " seconds delay");

        yield return new WaitForSeconds(delay);

        Quaternion rot = transform.rotation;
        Vector3 pos = transform.position;

        GameObject enemy = Instantiate(prefabToSpawn, pos, rot);
        enemy.GetComponent<JungleMobController>().homeTransform = this.transform;
        enemy.GetComponent<JungleMobController>().jungleCamp = this;
        enemy.GetComponent<JungleMobController>().maxDistanceToHome = followRange;
        NetworkServer.Spawn(enemy);
    }
}

