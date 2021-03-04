using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringFlee : MonoBehaviour, ISteering
{

    public SteeringCore agent;

    [Tooltip("The target transform to seek to. If not set, Target Position will be used instead.")]
    public Transform targetTransform;
    [Tooltip("The fallback target position to seek to. Y match will be disregarded if SteeringCore.yVelocityEnabled is false.")]
    public Vector3 targetPosition;

    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringFlee behavior, but no SteeringCore assigned.");
        }
    }


    void Update()
    {
        agent.SetMaxLinearAcceleration(GetSteering().linearAcceleration);
    }

    public SteeringOutput GetSteering()
    {
        if (targetTransform != null) targetPosition = targetTransform.position;

        return SteeringOutput.Get((agent.transform.position - targetPosition).normalized * agent.maxLinearAcceleration);
    }
}
