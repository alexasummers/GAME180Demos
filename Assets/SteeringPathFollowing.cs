using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringPathFollowing : MonoBehaviour, ISteering
{

    public SteeringCore agent;

    public SteeringPath path;

    private SteeringSeek mySeek;

    public bool showDebugGizmos = false;

    public float distanceOffset = 2.0f;

    // Start is called before the first frame update
    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringWander behavior, but no SteeringCore assigned.");
        }

        mySeek = GetComponent<SteeringSeek>();
        if (mySeek == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringWander behavior, but no SteeringSeek behavior. SteeringPursue depends on SteeringSeek.");
        }
    }


    void Update()
    {
        // Get me the closest point on the path
        Vector3 closestPoint = path.GetClosestPathLocation(agent.transform.position, distanceOffset);

        mySeek.targetTransform = null;
        mySeek.targetPosition = closestPoint;
    }

    public SteeringOutput GetSteering()
    {
        // Get me the closest point on the path
        Vector3 closestPoint = path.GetClosestPathLocation(agent.transform.position, distanceOffset);
        mySeek.targetTransform = null;
        mySeek.targetPosition = closestPoint;

        return mySeek.GetSteering();
    }

}
