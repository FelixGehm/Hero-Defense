using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMotor : MonoBehaviour {
    NavMeshAgent agent;

    Transform target;

    private bool faceOnly = false;
    
	// Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
    }
	
	void Update () {


		if(target != null)
        {
            if(!faceOnly)
            {
                agent.SetDestination(target.position);
            }
            FaceTarget();
        }
	}

    public void MoveToPoint(Vector3 point)
    {
        agent.SetDestination(point);
    }

    public void MoveToPoint(Vector3? point)
    {
        agent.SetDestination((Vector3)point);
    }

    public void FollowTarget (Interactable newTarget)
    {
        agent.stoppingDistance = newTarget.interactionRadius;
        agent.updateRotation = false;

        target = newTarget.interactionTransform;

        faceOnly = false;
    }

    public void StopFollowingTarget()
    {
        agent.stoppingDistance = 0;
        agent.updateRotation = true;

        target = null;
    }

    public void PauseFollowTarget()
    {
        //Debug.Log("PauseFollowTarget():");
        agent.SetDestination(transform.position);

        faceOnly = true;
    }

    public void ContinueFollowTarget()
    {
        //Debug.Log("ContinueFollowTarget():");

        if(target != null)
        {
            agent.SetDestination(target.position);
            faceOnly = false;
        } else
        {
            //Debug.Log("Couldn't continue following target. Might have disapeared or focus changed");
        }
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }
}
