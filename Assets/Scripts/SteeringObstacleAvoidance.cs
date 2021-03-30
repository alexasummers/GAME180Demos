using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringObstacleAvoidance : MonoBehaviour, ISteering
{
    public SteeringCore agent;

    public float avoidDistance = 2f;

    [Min(0)]
    public float centerRayLength = 5.0f;
    [Min(0)]
    public float whiskerLength = 3.0f;

    [Range(0, 180)]
    public float whiskerAngle = 45;

    public LayerMask obstacleLayers;

    public bool showDebugGizmos = false;

    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null) {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringObstacleAvoidance behavior, but no SteeringCore assigned.");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetLinearAcceleration(GetSteering().linearAcceleration);
    }

    private Vector3 dbgCenterStart, dbgCenterEnd,
                    dbgLeftWStart, dbgLeftWEnd,
                    dbgRightWStart, dbgRightWEnd,
                    dbgFinalSeek;
    private List<Vector3> avoidanceTargets = new List<Vector3>();
    private List<Vector3> contactPoints = new List<Vector3>();

    public SteeringOutput GetSteering() {

        // collect obstacle-avoid targets
        avoidanceTargets.Clear();
        RaycastHit hitInfo;

        // raycast based on configuration.
        if (centerRayLength > 0) {
            dbgCenterStart = agent.transform.position;
            dbgCenterEnd = agent.transform.position + agent.transform.forward * centerRayLength;
            // raycast from center based on centerRayLength.
            if (Physics.Raycast(agent.transform.position, agent.transform.forward, out hitInfo, centerRayLength, obstacleLayers)) {
                contactPoints.Add(hitInfo.point);
                avoidanceTargets.Add(hitInfo.point + hitInfo.normal * avoidDistance);
            }
        }

        if (whiskerLength > 0) {

            // left and right by whiskerAngle
            Vector3 l = agent.transform.forward, r = agent.transform.forward;
            l = Quaternion.AngleAxis(-whiskerAngle, agent.transform.up) * l;
            r = Quaternion.AngleAxis(whiskerAngle, agent.transform.up) * r;

            Vector3 ls = agent.transform.position + agent.transform.right * -1 * agent.selfRadius;
            Vector3 rs = agent.transform.position + agent.transform.right * agent.selfRadius;

            dbgLeftWStart = ls;
            dbgLeftWEnd = l * whiskerLength + ls;
            dbgRightWStart = rs;
            dbgRightWEnd = r * whiskerLength + rs;

            if (Physics.Raycast(ls, l, out hitInfo, whiskerLength, obstacleLayers)) {
                contactPoints.Add(hitInfo.point);
                avoidanceTargets.Add(hitInfo.point + hitInfo.normal * avoidDistance);
            }
            if (Physics.Raycast(rs, r, out hitInfo, whiskerLength, obstacleLayers)) {
                contactPoints.Add(hitInfo.point);
                avoidanceTargets.Add(hitInfo.point + hitInfo.normal * avoidDistance);
            }
        }

        if (avoidanceTargets.Count > 0) {
            // average, normally
            Vector3 avgPosition = Vector3.zero;
            for (int i = 0; i < avoidanceTargets.Count; i++) avgPosition += avoidanceTargets[i];
            avgPosition /= avoidanceTargets.Count;
            dbgFinalSeek = avgPosition;
            return agent.GetMaxAcceleration(avgPosition - transform.position);
        } else {
            return SteeringOutput.None();
        }

    }

    private void OnDrawGizmos() 
    {
        if (!showDebugGizmos) return;


        Color c = Gizmos.color;

        Gizmos.color = Color.cyan;
        if (centerRayLength > 0) Gizmos.DrawLine(dbgCenterStart, dbgCenterEnd);

        if (whiskerLength > 0) {
            Gizmos.DrawLine(dbgLeftWStart, dbgLeftWEnd);
            Gizmos.DrawLine(dbgRightWStart, dbgRightWEnd);
        }


        if (avoidanceTargets.Count > 0) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(dbgFinalSeek, 0.3f);
        }

        Gizmos.color = c;

    }
}
