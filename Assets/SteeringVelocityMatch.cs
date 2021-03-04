using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringVelocityMatch : MonoBehaviour, ISteering
{

    public SteeringCore agent;

    [Tooltip("The target Steering agent to match velocity on. If not set, Match Velocity will be used instead.")]
    public SteeringCore velocitySource;
    [Tooltip("The fallback target velocity to match to. Y match will be disregarded if SteeringCore.yVelocityEnabled is false.")]
    public Vector3 matchVelocity;


    private void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringVelocityMatch behavior, but no SteeringCore assigned.");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (velocitySource != null) matchVelocity = velocitySource.velocity;

        agent.SetVelocityMatch(matchVelocity);
    }

    public SteeringOutput GetSteering()
    {
        Vector3 velocityChange = matchVelocity - agent.velocity;
        return SteeringOutput.Get(velocityChange.normalized * agent.maxLinearAcceleration);
    }

    public SteeringOutput GetSteering(Vector3 mv) 
    {
        Vector3 velocityChange = mv - agent.velocity;
        return SteeringOutput.Get(velocityChange.normalized * agent.maxLinearAcceleration);
    }
}
