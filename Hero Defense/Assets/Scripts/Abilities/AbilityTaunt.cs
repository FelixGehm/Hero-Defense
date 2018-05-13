using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityTaunt : MonoBehaviour
{
    static float abilityCooldown = 3.0f;
    float currentCooldown = abilityCooldown;

    public float range = 10.0f;
    float duration = 2.0f;

    // Use this for initialization
    void Start()
    {
        GetComponent<CharacterEventManager>().OnAbilityOne += Cast;

    }

    void Update()
    {
        currentCooldown -= Time.deltaTime;
    }

    public void Cast()
    {
        Debug.Log("Cast(): Taunt");
        if (currentCooldown <= 0)
        {
            currentCooldown = abilityCooldown;
            TauntNearbyEnemys();
        }
    }

    private void TauntNearbyEnemys()
    {
        Debug.Log("TauntNearbyEnemys(): ");
        List<GameObject> closeEnemys = FindEnemysInRange(range);
        foreach (GameObject enemy in closeEnemys)
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            ec.GetTaunted(this.transform, duration);            
        }
    }
    

    /// <summary>
    /// Sucht nach Gameobjekten in der angegebenen Reichweite mit dem Tag "Enemy" und gibt eine List mit den Gameobjekten zurück.
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    private List<GameObject> FindEnemysInRange(float range)
    {
        List<GameObject> closeEnemys = new List<GameObject>();

        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log("enemys with tag = " + enemys.Length);

        // check distance and add to List if in range
        foreach (GameObject enemy in enemys)
        {
            float distance = Vector3.Distance(enemy.transform.position, transform.position);

            Debug.Log("enemy distance = " + distance);
            if (distance <= range)
            {
                closeEnemys.Add(enemy);
                Debug.Log("enemy added to list");
            }
        }
        Debug.Log("FindEnemysInRange(): ");
        Debug.Log("closeEnemys.Count = " + closeEnemys.Count);

        return closeEnemys;
    }


    // Draw Range in Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

}
