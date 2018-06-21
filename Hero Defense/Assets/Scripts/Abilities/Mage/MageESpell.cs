using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MageESpell : NetworkBehaviour
{
    private float speed = 2;
    private float maxRange = 5;
    private float damage = 10;
    private float healAmount = 10;
    private float maxBounces = 6;
    private float currentBounces = 0;

    private GameObject target;
    private List<GameObject> usedTargets;

    Vector3 direction;

    MageESpellDummy dummy;

    public void Init(GameObject firstTarget, float speed, float maxRange, float damage, float healAmount, float maxBounces)
    {
        target = firstTarget;
        this.speed = speed;
        this.maxRange = maxRange;
        this.damage = damage;
        this.healAmount = healAmount;
        this.maxBounces = maxBounces;

        dummy = GetComponent<MageESpellDummy>();
        dummy.Init(speed, target);
    }

    //[Server]
    void Start()
    {
        //SearchForNextTarget();
        dummy = GetComponent<MageESpellDummy>();
        usedTargets = new List<GameObject>();
    }

    private bool reachedTarget = false;
    //[Server]
    void Update()
    {
        if (!isServer)
        {
            this.enabled = false;
            return;
        }

        if (isServer)
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }


            direction = (target.transform.position - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime, Space.World);


            //Debug.Log((transform.position - target.transform.position).magnitude);


            if (!reachedTarget && (transform.position - target.transform.position).magnitude <= 0.05f) //transform nearly reached target
            {
                //init hit particle effect

                if (target.CompareTag("Player"))
                {
                    target.GetComponent<CharacterStats>().TakeHeal(healAmount);
                }
                else if (target.CompareTag("Enemy"))
                {
                    target.GetComponent<CharacterStats>().TakeMagicDamage(damage);
                }

                reachedTarget = true;
                usedTargets.Add(target);
                currentBounces++;
                if (currentBounces < maxBounces)
                {
                    target = SearchForNextTarget();
                    dummy.SetTarget(target);
                }
                else
                {
                    target = null;
                }

            }
            else
            {
                reachedTarget = false;
            }
        }
    }

    private GameObject SearchForNextTarget()    //searches for the closest target that was not used allready
    {
        #region SetupList
        List<GameObject> units = new List<GameObject>();
        foreach (GameObject player in PlayerManager.instance.players) //Eine Liste für die Spieler wäre sehr viel praktischer
        {
            if (player != null)
            {
                units.Add(player);
            }
        }
        units.AddRange(PlayerManager.instance.enemies);
        #endregion
        GameObject _target = null;
        float dist = 0;
        float lastDist = float.MaxValue;

        foreach (GameObject unit in units)
        {
            if (unit != target && IsInRange(unit) && !WasTargetUsed(unit))
            {
                dist = Vector3.Distance(unit.transform.position, transform.position);

                if (dist < lastDist)
                {
                    _target = unit;
                }
                lastDist = dist;
            }
        }
        return _target;
    }



    private GameObject SearchForClosestTarget()
    {
        float dist = 0;
        float lastDist = float.MaxValue;
        GameObject _target = null;
        foreach (GameObject enemy in PlayerManager.instance.enemies)
        {
            if (enemy != target)
            {
                dist = Vector3.Distance(enemy.transform.position, transform.position);

                if (dist < lastDist)
                {
                    _target = enemy;
                }
                lastDist = dist;
            }
        }
        return _target;
    }

    private bool WasTargetUsed(GameObject _target)
    {
        foreach (GameObject t in usedTargets)
        {
            if (_target == t)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsInRange(GameObject _target)
    {
        if (Vector3.Distance(_target.transform.position, this.transform.position) <= maxRange)
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }
}
