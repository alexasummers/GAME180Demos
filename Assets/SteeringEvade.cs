using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringEvade : MonoBehaviour, ISteering
{

    public SteeringCore agent;

    [Tooltip("The steering target to evade. If Steering Target and Kinematic Target are both specified, Steering Target will be used.")]
    public SteeringCore steeringTarget;
    public KinematicCore kinematicTarget;

    public float maxLookAheadSeconds = 5;   // s

    private SteeringFlee myFlee;

    public bool showDebugGizmos = false;

    private Vector3 debugEvadeLocation;

    public float activationDistance = 10f;

    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringEvade behavior, but no SteeringCore assigned.");
        }

        myFlee = GetComponent<SteeringFlee>();
        if (myFlee == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringEvade behavior, but no SteeringFlee behavior. SteeringPursue depends on SteeringSeek.");
        }
    }

    public float debugTimeToTarget = 0f;

    void Update()
    {
        myFlee.targetTransform = null;

        Vector3 targetPosition = steeringTarget == null ? kinematicTarget.transform.position : steeringTarget.transform.position;
        Vector3 targetVelocity = steeringTarget == null ? kinematicTarget.velocity : steeringTarget.velocity;

        float distanceToTarget = (targetPosition - transform.position).magnitude;
        if (distanceToTarget == 0 || agent.velocity.magnitude == 0)
        {
            myFlee.targetPosition = targetPosition;
            return;
        }

        float timeToTarget = Mathf.Clamp(distanceToTarget / agent.velocity.magnitude, 0, maxLookAheadSeconds);
        debugTimeToTarget = timeToTarget;

        targetPosition += targetVelocity * timeToTarget;

        myFlee.targetPosition = targetPosition;

        debugEvadeLocation = targetPosition;
    }

    public SteeringOutput GetSteering()
    {
        myFlee.targetTransform = null;

        Vector3 targetPosition = steeringTarget == null ? kinematicTarget.transform.position : steeringTarget.transform.position;
        Vector3 targetVelocity = steeringTarget == null ? kinematicTarget.velocity : steeringTarget.velocity;

        float distanceToTarget = (targetPosition - transform.position).magnitude;


        if (distanceToTarget > activationDistance) return SteeringOutput.None();


        if (distanceToTarget == 0 || agent.velocity.magnitude == 0)
        {
            myFlee.targetPosition = targetPosition;
            return SteeringOutput.None();
        }

        float timeToTarget = Mathf.Clamp(distanceToTarget / agent.velocity.magnitude, 0, maxLookAheadSeconds);
        debugTimeToTarget = timeToTarget;

        targetPosition += targetVelocity * timeToTarget;

        myFlee.targetPosition = targetPosition;

        return myFlee.GetSteering();
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Color c = Gizmos.color;

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(debugEvadeLocation, 0.3f);

        Gizmos.color = c;
    }

    public static SteeringOutput GetEvasion(Vector3 actorPos, Vector3 actorVel, Vector3 toEvadePos, Vector3 toEvadeVel) {

        Vector3 targetPosition = toEvadePos;
        Vector3 targetVelocity = toEvadeVel;

        float distanceToTarget = (targetPosition - actorPos).magnitude;

        if (distanceToTarget == 0 || actorVel.magnitude == 0)
        {
            return SteeringOutput.None();
        }

        float timeToTarget = distanceToTarget / actorVel.magnitude;

        targetPosition += targetVelocity * timeToTarget;

        // flee!
        return SteeringOutput.Get(actorPos - targetPosition);

    }
}
