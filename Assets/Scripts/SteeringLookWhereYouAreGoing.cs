using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteeringAlign))]
public class SteeringLookWhereYouAreGoing : MonoBehaviour, ISteering
{

    private SteeringAlign align;

    public SteeringCore agent;

    public SteeringOutput GetSteering()
    {
        align.alignTarget = null;
        align.alignDirection = align.Agent.velocity;
        return align.GetSteering();
    }

    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();

        align = GetComponent<SteeringAlign>();

    }


    void Update()
    {
        align.alignTarget = null;
        align.alignDirection = align.Agent.velocity;

        agent.SetAngularAcceleration(align.GetSteering().angularAcceleration);

    }
}
