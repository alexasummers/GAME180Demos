using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringSeparate : MonoBehaviour, ISteering
{
    private const float EPSILON = 0.02f;
    public SteeringCore agent;

    private SteeringVelocityMatch myVelocityMatch;

    private List<Transform> separationTargets = new List<Transform>();

    public bool useInverseSquare = true;

    public float decayCoefficient = 5;

    public float linearThreshold = 5;

    [Range(0, 360)]
    public float coneArcDegrees = 360;
    public float coneLength = 8;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringSeparate behavior, but no SteeringCore assigned.");
            return;
        }

        myVelocityMatch = GetComponent<SteeringVelocityMatch>();
        if (myVelocityMatch == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringSeparate behavior, but no SteeringVelocityMatch behavior. SteeringSeparate depends on SteeringVelocityMatch.");
            return;
        }

        // find things to separate from - anything with a SteeringCore or KinematicCore
        // This method of keeping track of what to separate from is highly error-prone
        // and not recommended to emulate.
        // It's much better to occasionally refresh by "looking around" for active 
        // entities to separate from, within a limited radius.
        // This computation can be distributed over several frames with some intermediate hackery,
        // and then done regularly enough that separation will be performant and correct
        // with minimal visible errors.
        Dictionary<Transform, bool> acc = new Dictionary<Transform, bool>();

        SteeringCore[] steeringCores = GameObject.FindObjectsOfType<SteeringCore>();
        for (int i = 0; i < steeringCores.Length; i++)
        {
            if (steeringCores[i] != agent && !acc.ContainsKey(steeringCores[i].transform))
            {
                acc.Add(steeringCores[i].transform, true);
                separationTargets.Add(steeringCores[i].transform);
            }
        }

        KinematicCore[] kinematicCores = GameObject.FindObjectsOfType<KinematicCore>();
        for (int i = 0; i < kinematicCores.Length; i++)
        {
            if (kinematicCores[i].transform != agent && !acc.ContainsKey(kinematicCores[i].transform))
            {
                acc.Add(kinematicCores[i].transform, true);
                separationTargets.Add(kinematicCores[i].transform);
            }
        }

//        print(separationTargets.Count.ToString() + " separation targets for " + transform.name);

    }

    // Update is called once per frame
    void Update()
    {
        // Accumulate all separations
        Vector3 total = Vector3.zero;
        for (int i = 0; i < separationTargets.Count; i++)
        {
            total += GetSeparation(separationTargets[i].position);
        }

        // total represents a suggested steering.
        // If we use acceleration, the agent will never stop moving.
        // Really what we want is a velocity match.

        // We'll often have small values, which we can ignore.
        if (total.sqrMagnitude <= EPSILON * EPSILON) total = Vector3.zero;

        myVelocityMatch.velocitySource = null;
        myVelocityMatch.matchVelocity = total;
    }

    Vector3 GetSeparation(Vector3 other)
    {
        // Does it qualify?
        if (Vector3.Angle(other - transform.position, transform.forward) > coneArcDegrees / 2) return Vector3.zero;
        if (Vector3.Distance(other, transform.position) > coneLength) return Vector3.zero;

        // move in the direction of displacement
        Vector3 displacement = (agent.transform.position - other);
        float distance = displacement.magnitude;
        if (useInverseSquare)
        {
            float sc = decayCoefficient / distance / distance;
            if (agent.maxLinearAcceleration < sc) sc = agent.maxLinearAcceleration;

            return displacement.normalized * sc;
        }
        else
        {
            if (distance > linearThreshold) return Vector3.zero;
            return displacement.normalized * agent.maxLinearAcceleration * (linearThreshold - distance) / linearThreshold;
        }
    }

    public bool noSteeringAtTarget = true;
    public SteeringOutput GetSteering()
    {
        // Accumulate all separations
        Vector3 total = Vector3.zero;
        for (int i = 0; i < separationTargets.Count; i++)
        {
            total += GetSeparation(separationTargets[i].position);
        }

        // total represents a suggested steering.
        // If we use acceleration, the agent will never stop moving.
        // Really what we want is a velocity match.

        // We'll often have small values, which we can ignore.
        if (total.sqrMagnitude <= EPSILON * EPSILON) total = Vector3.zero;

        if (noSteeringAtTarget && total == Vector3.zero)
        {
            return SteeringOutput.None();
        }

        return SteeringOutput.Get(total);
    }

    public void SetSeparationTargets(List<Transform> targets)
    {
        separationTargets = targets;
    }

    //public Vector3 GetSteering(List<Vector3> targets)
    //{
    //    Vector3 total = Vector3.zero;
    //    for (int i = 0; i < targets.Count; i++)
    //    {
    //        total += GetSeparation(targets[i]);
    //    }
    //    if (total.sqrMagnitude <= EPSILON * EPSILON) total = Vector3.zero;
    //    myVelocityMatch.matchVelocity = total;
    //    return myVelocityMatch.GetSteering();
    //}
}
