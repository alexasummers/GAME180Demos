using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringConstrainedWander : MonoBehaviour, ISteering
{
    public SteeringCore agent;

    private SteeringSeek mySeek;
    private SteeringWander myWander;

    public float outerRange = 8.0f;

    public Transform constraintCenter;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringConstrainedWander behavior, but no SteeringCore assigned.");
        }

        mySeek = GetComponent<SteeringSeek>();
        if (mySeek == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringConstrainedWander behavior, but no SteeringSeek behavior. SteeringConstrainedWander depends on SteeringSeek.");
        }

        myWander = GetComponent<SteeringWander>();
        if (myWander == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringConstrainedWander behavior, but no SteeringWander behavior. SteeringConstrainedWander depends on SteeringSeek.");
        }

    }


    // Update is called once per frame
    void Update()
    {
        Vector3 wanderAcc = myWander.GetSteering().linearAcceleration;
        Vector3 seekAcc = mySeek.GetSteering(constraintCenter.position).linearAcceleration;

        // wander rate is 1; center rate is distance from center * centerRate
        Vector3 centerLocation = constraintCenter.position;
        float centerDistance = (centerLocation - transform.position).magnitude;

        // Past the outer range, our Seek acceleration should be full.
        float normalizedDistance = centerDistance / outerRange / 2;

        // But we do want Wander to take the priority as much as possible.
        // The further out we are, the more we want Seek to take over.
        // We want it to ramp up at the edge.
        float wanderFactor = (1 - normalizedDistance) * (1 - normalizedDistance);
        float centerFactor = 1 - wanderFactor;


        Vector3 totalAcc = wanderAcc * wanderFactor + seekAcc * centerFactor;

        agent.SetLinearAcceleration(totalAcc);
    }

    public SteeringOutput GetSteering()
    {
        Vector3 wanderAcc = myWander.GetSteering().linearAcceleration;
        Vector3 seekAcc = mySeek.GetSteering(constraintCenter.position).linearAcceleration;

        // wander rate is 1; center rate is distance from center * centerRate
        Vector3 centerLocation = mySeek.targetTransform == null ? mySeek.targetPosition : mySeek.targetTransform.position;
        float centerDistance = (centerLocation - transform.position).magnitude;

        // Past the outer range, our Seek acceleration should be full.
        float normalizedDistance = centerDistance / outerRange / 2;

        // But we do want Wander to take the priority as much as possible.
        // The further out we are, the more we want Seek to take over.
        // We want it to ramp up at the edge.
        float wanderFactor = (1 - normalizedDistance) * (1 - normalizedDistance);
        float centerFactor = 1 - wanderFactor;


        Vector3 totalAcc = wanderAcc * wanderFactor + seekAcc * centerFactor;

        return SteeringOutput.Get(totalAcc);

    }
}
