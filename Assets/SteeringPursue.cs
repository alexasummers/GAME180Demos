using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringPursue : MonoBehaviour, ISteering
{

    public SteeringCore agent;

    private SteeringSeek mySeek;

    [Tooltip("The steering target to pursue. If Steering Target and Kinematic Target are both specified, Steering Target will be used.")]
    public SteeringCore steeringTarget;
    public KinematicCore kinematicTarget;

    public float maxLookAheadSeconds = 5;   // s

    public bool showDebugGizmos = false;

    private Vector3 debugPursueLocation;

    public float activationDistance = 10f;

    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringPursue behavior, but no SteeringCore assigned.");
        }

        mySeek = GetComponent<SteeringSeek>();
        if (mySeek == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringPursue behavior, but no SteeringSeek behavior. SteeringPursue depends on SteeringSeek.");
        }
    }

    public float debugTimeToTarget = 0f;

    void Update()
    {
        mySeek.targetTransform = null;

        Vector3 targetPosition = steeringTarget == null ? kinematicTarget.transform.position : steeringTarget.transform.position;
        Vector3 targetVelocity = steeringTarget == null ? kinematicTarget.velocity : steeringTarget.velocity;

        float distanceToTarget = (targetPosition - transform.position).magnitude;
        if (distanceToTarget == 0 || agent.velocity.magnitude == 0)
        {
            mySeek.targetPosition = targetPosition;
            return;
        }

        float timeToTarget = Mathf.Clamp(distanceToTarget / agent.velocity.magnitude, 0, maxLookAheadSeconds);
        debugTimeToTarget = timeToTarget;

        targetPosition += targetVelocity * timeToTarget;

        mySeek.targetPosition = targetPosition;

        debugPursueLocation = targetPosition;

    }


    public SteeringOutput GetSteering()
    {
        mySeek.targetTransform = null;

        Vector3 targetPosition = steeringTarget == null ? kinematicTarget.transform.position : steeringTarget.transform.position;
        Vector3 targetVelocity = steeringTarget == null ? kinematicTarget.velocity : steeringTarget.velocity;

        float distanceToTarget = (targetPosition - transform.position).magnitude;


        if (distanceToTarget > activationDistance) return SteeringOutput.None();


        if (distanceToTarget == 0 || agent.velocity.magnitude == 0)
        {
            mySeek.targetPosition = targetPosition;
            return mySeek.GetSteering();
        }

        float timeToTarget = Mathf.Clamp(distanceToTarget / agent.velocity.magnitude, 0, maxLookAheadSeconds);
        debugTimeToTarget = timeToTarget;

        targetPosition += targetVelocity * timeToTarget;

        mySeek.targetPosition = targetPosition;

        debugPursueLocation = targetPosition;

        return mySeek.GetSteering();

    }


}
