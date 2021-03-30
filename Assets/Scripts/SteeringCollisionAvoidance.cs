using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringCollisionAvoidance : MonoBehaviour, ISteering
{
    public SteeringCore agent;
    public float behaviorRange = 10;

    public bool showDebugGizmos = false;

    private List<SteeringCore> collisionTargets = new List<SteeringCore>();
    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringSeparate behavior, but no SteeringCore assigned.");
            return;
        }

        SteeringCore[] steeringCores = GameObject.FindObjectsOfType<SteeringCore>();
        for (int i = 0; i < steeringCores.Length; i++)
        {
            if (steeringCores[i] != agent)
            {
                collisionTargets.Add(steeringCores[i]);
            }
        }
    }

    void Update()
    {
        
    }

    bool lastSteerHit = false;
    public SteeringOutput GetSteering() {

        lastSteerHit = false;

        int indexOfSoonest = -1;
        float soonestTime = -1;
        for (int i = 0; i < collisionTargets.Count; i++) {
            Vector3 p = collisionTargets[i].transform.position;

            if ((p - transform.position).magnitude < behaviorRange) {
                // find time of closest approach
                float t = TimeOfClosestApproach(collisionTargets[i]);
                if (t > 0 && (t < soonestTime || soonestTime < 0) ) {
                    // Will we collide at that point?

                    Vector3 cpp = transform.position + agent.velocity * t,
                            opp = collisionTargets[i].transform.position + collisionTargets[i].velocity * t;

                    if ((cpp - opp).magnitude < agent.collisionRadius + collisionTargets[i].collisionRadius) {
                        // yes, therefore...
                        indexOfSoonest = i;
                        soonestTime = t;
                    }
                }
            }
        }

        // aaand...
        if (indexOfSoonest >= 0) {

            // evade-as-if.
            // pretend you're both at that location,
            // and get the evasion steering based on that.
            // We'll need a fully delegatable evade.
            SteeringOutput delEvade = SteeringEvade.GetEvasion(
                agent.transform.position + agent.velocity * soonestTime, 
                agent.velocity, 
                collisionTargets[indexOfSoonest].transform.position + collisionTargets[indexOfSoonest].velocity * soonestTime, 
                collisionTargets[indexOfSoonest].velocity);

            lastSteerHit = true;
            myClosestApproachPos = agent.transform.position + agent.velocity * soonestTime;
            theirClosestApproachPos = collisionTargets[indexOfSoonest].transform.position + collisionTargets[indexOfSoonest].velocity * soonestTime;

            return delEvade;

        }

        return SteeringOutput.None();

    }

    private float TimeOfClosestApproach(SteeringCore other) {

        // Correction from the book:
        // We need the inverse of relative velocity.
        // I don't quite understand the math.
        Vector3 dp = other.transform.position - transform.position,
                dv = agent.velocity - other.velocity;

        // If dv is small enough, they'll effectively never hit
        // (according to the equation anyway).
        if (dv.sqrMagnitude == 0) return -1;
        
        return Vector3.Dot(dp, dv) / dv.sqrMagnitude;

    }

    private Vector3 myClosestApproachPos, theirClosestApproachPos;
    private void OnDrawGizmos() {

        if (!showDebugGizmos || agent == null) return;
        if (!lastSteerHit) return;

        // show position of both at time of closest approach
        Color c = Gizmos.color;

        Gizmos.color = new Color(0.5f, 0, 1f, 0.6f);
        Gizmos.DrawSphere(myClosestApproachPos, agent.collisionRadius);

        Gizmos.color = new Color(1.0f, 0, 0.5f, 0.6f);
        Gizmos.DrawSphere(theirClosestApproachPos, agent.collisionRadius);

        Gizmos.color = c;
    }

}
