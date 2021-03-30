using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringFlocking : MonoBehaviour, ISteering
{
    public SteeringCore agent;

    public string flockGroupName = string.Empty;

    [Range(0f, 1f)]
    public float velocityMatchWeight = 0.6f;

    [Range(0f, 1f)]
    public float separationWeight = 0.2f;

    [Range(0f, 1f)]
    public float cohesionWeight = 0.2f;

    public float neighborhoodRadius = 3;

    public bool showComponentAccelerations = false;

    private SteeringArrive arrive;
    private SteeringSeparate separate;
    private SteeringVelocityMatch velocityMatch;
    private SteeringAlign align;


    static Dictionary<string, List<SteeringFlocking>> flockByName = new Dictionary<string, List<SteeringFlocking>>();

    // Start is called before the first frame update
    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringSeek behavior, but no SteeringCore assigned.");
            return;
        }

        arrive = GetComponent<SteeringArrive>();
        separate = GetComponent<SteeringSeparate>();
        velocityMatch = GetComponent<SteeringVelocityMatch>();
        align = GetComponent<SteeringAlign>();


        if (!flockByName.ContainsKey(flockGroupName)) flockByName.Add(flockGroupName, new List<SteeringFlocking>());
        flockByName[flockGroupName].Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        float sum = velocityMatchWeight + separationWeight + cohesionWeight;
        if (sum == 0) sum = 1;

        float norm = 1 / sum;
        velocityMatchWeight *= norm;
        separationWeight *= norm;
        cohesionWeight *= norm;


        // Find everything in the flock that's within my radius
        Vector3 centerOfMass = transform.position;
        Vector3 forward = transform.forward;
        Vector3 averageVelocity = agent.velocity;
        List<Transform> separationTargets = new List<Transform>();
        for (int i = 0; i < flockByName[flockGroupName].Count; i++)
        {
            SteeringFlocking boid = flockByName[flockGroupName][i];
            if (boid != this)
            {
                float nSquared = neighborhoodRadius * neighborhoodRadius;
                if (Vector3.SqrMagnitude(boid.transform.position - transform.position) < nSquared)
                {
                    centerOfMass += (boid.transform.position - transform.position);
                    forward += boid.transform.forward;
                    separationTargets.Add(boid.transform);
                    averageVelocity += boid.agent.velocity;
                }
            }
        }

        // arrive at center of mass
        arrive.targetPosition = centerOfMass;
        Vector3 arriveAccel = arrive.GetSteering().linearAcceleration;

        // and separation targets
        separate.SetSeparationTargets(separationTargets);
        Vector3 separationAccel = separate.GetSteering().linearAcceleration;

        // and velocity match
        averageVelocity /= (1 + separationTargets.Count);
        velocityMatch.matchVelocity = averageVelocity;
        Vector3 velocityMatchAccel = velocityMatch.GetSteering().linearAcceleration;

        debugTotal = arriveAccel * cohesionWeight + separationAccel * separationWeight + velocityMatchAccel * velocityMatchWeight;

        agent.SetLinearAcceleration(debugTotal);

        align.alignDirection = forward;

        debugArrive = arriveAccel * cohesionWeight;
        debugSeparate = separationAccel * separationWeight;
        debugVelocityMatch = velocityMatchAccel * velocityMatchWeight;

    }

    public SteeringOutput GetSteering()
    {
        float sum = velocityMatchWeight + separationWeight + cohesionWeight;
        if (sum == 0) sum = 1;

        float norm = 1 / sum;
        velocityMatchWeight *= norm;
        separationWeight *= norm;
        cohesionWeight *= norm;


        // Find everything in the flock that's within my radius
        Vector3 centerOfMass = transform.position;
        Vector3 forward = transform.forward;
        Vector3 averageVelocity = agent.velocity;
        List<Transform> separationTargets = new List<Transform>();
        for (int i = 0; i < flockByName[flockGroupName].Count; i++)
        {
            SteeringFlocking boid = flockByName[flockGroupName][i];
            if (boid != this)
            {
                float nSquared = neighborhoodRadius * neighborhoodRadius;
                if (Vector3.SqrMagnitude(boid.transform.position - transform.position) < nSquared)
                {
                    centerOfMass += (boid.transform.position - transform.position);
                    forward += boid.transform.forward;
                    separationTargets.Add(boid.transform);
                    averageVelocity += boid.agent.velocity;
                }
            }
        }

        // arrive at center of mass
        arrive.targetPosition = centerOfMass;
        Vector3 arriveAccel = arrive.GetSteering().linearAcceleration;

        // and separation targets
        separate.SetSeparationTargets(separationTargets);
        Vector3 separationAccel = separate.GetSteering().linearAcceleration;

        // and velocity match
        averageVelocity /= (1 + separationTargets.Count);
        velocityMatch.matchVelocity = averageVelocity;
        Vector3 velocityMatchAccel = velocityMatch.GetSteering().linearAcceleration;

        debugTotal = arriveAccel * cohesionWeight + separationAccel * separationWeight + velocityMatchAccel * velocityMatchWeight;


        align.alignDirection = forward;

        debugArrive = arriveAccel * cohesionWeight;
        debugSeparate = separationAccel * separationWeight;
        debugVelocityMatch = velocityMatchAccel * velocityMatchWeight;

        SteeringOutput alignOutput = align.GetSteering();

        return SteeringOutput.Get(debugTotal, alignOutput.angularAcceleration);



    }

    public Vector3 debugArrive, debugSeparate, debugVelocityMatch;
    public Vector3 debugTotal;

    private void OnDrawGizmos()
    {
        Color c = Gizmos.color;

        if (showComponentAccelerations)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + debugArrive);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + debugSeparate);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + debugVelocityMatch);
        }



        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + debugTotal);


        Gizmos.color = c;
    }

}
