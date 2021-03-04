using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringSeek : MonoBehaviour, ISteering
{

    public SteeringCore agent;

    [Tooltip("The target transform to seek to. If not set, Target Position will be used instead.")]
    public Transform targetTransform;
    [Tooltip("The fallback target position to seek to. Y match will be disregarded if SteeringCore.yVelocityEnabled is false.")]
    public Vector3 targetPosition;

    public bool showDebugGizmos = false;

    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringSeek behavior, but no SteeringCore assigned.");
        }
    }


    void Update()
    {
        if (targetTransform != null) targetPosition = targetTransform.position;

        agent.SetMaxLinearAcceleration(targetPosition - agent.transform.position);
    }

    public SteeringOutput GetSteering(Vector3 target)
    {
        Vector3 direction = target - agent.transform.position;
        return SteeringOutput.Get(direction.normalized * agent.maxLinearAcceleration);
    }

    public SteeringOutput GetSteering()
    {
        return GetSteering(targetTransform == null ? targetPosition : targetTransform.position);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        Color c = Gizmos.color;

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(targetTransform != null ? targetTransform.position : targetPosition, 0.3f);

        Gizmos.color = c;
    }
}
