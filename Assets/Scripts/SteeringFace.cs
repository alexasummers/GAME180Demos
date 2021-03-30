using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteeringAlign))]
public class SteeringFace : MonoBehaviour, ISteering
{
     public SteeringCore agent;

    public Transform faceTarget;

    private SteeringAlign align;

    void Awake()
    {
        align = GetComponent<SteeringAlign>();

        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringSeparate behavior, but no SteeringCore assigned.");
            return;
        }
    }


    void Update()
    {
        align.alignTarget = null;
        align.alignDirection = faceTarget.position - transform.position;

        agent.SetAngularAcceleration(align.GetSteering().angularAcceleration);
        
    }

    public SteeringOutput GetSteering() {
        align.alignTarget = null;
        align.alignDirection = faceTarget.position - transform.position;
        return align.GetSteering();
    }
}
