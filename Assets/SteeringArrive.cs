using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringArrive : MonoBehaviour, ISteering
{

    public SteeringCore agent;

    [Tooltip("The target transform to seek to. If not set, Target Position will be used instead.")]
    public Transform targetTransform;
    [Tooltip("The fallback target position to seek to. Y match will be disregarded if SteeringCore.yVelocityEnabled is false.")]
    public Vector3 targetPosition;

    public float radiusOfSatisfaction = 1.0f;
    public float radiusOfApproach = 3.0f;

    public bool showDebugGizmos = false;
    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringArrive behavior, but no SteeringCore assigned.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // distance to target?
        if (targetTransform != null) targetPosition = targetTransform.position;

        agent.SetLinearAcceleration(GetSteering().linearAcceleration);

    }

    private void OnDrawGizmos()
    {
        if (enabled && showDebugGizmos)
        {
            for (int i = 0; i < 10; i++)
            {
                float radius = Mathf.Lerp(radiusOfSatisfaction, radiusOfApproach, i / 10f);
                Color c = Color.Lerp(Color.black, Color.white, i / 10f);
                GizmoUtils.DrawGizmoCircle(targetPosition, radius, Vector3.up, 32, c);
            }
        }
    }

    public bool noSteeringAtTarget = true;
    public SteeringOutput GetSteering()
    {
        if (targetTransform != null) targetPosition = targetTransform.position;

        Vector3 toTarget = targetPosition - agent.transform.position;
        if (!agent.yVelocityEnabled) toTarget.y = 0;
        float distanceToTarget = toTarget.magnitude;

        if (noSteeringAtTarget && distanceToTarget == 0) return SteeringOutput.None();

        Vector3 normalVelocityToTarget = toTarget.normalized;

        Vector3 targetVelocity = normalVelocityToTarget * Mathf.Clamp((distanceToTarget - radiusOfSatisfaction) / (radiusOfApproach - radiusOfSatisfaction), 0, agent.maxSpeed);

        Vector3 velocityChange = targetVelocity - agent.velocity / agent.maxSpeed;

        return SteeringOutput.Get(velocityChange.normalized * agent.maxLinearAcceleration);
    }

}

public class GizmoUtils
{
    public static void DrawGizmoCircle(Vector3 center, float radius, Vector3 up, int segments, Color color)
    {
        Color oc = Gizmos.color;
        Gizmos.color = color;

        Queue<Vector3> points = new Queue<Vector3>();
        Vector3 localPointer = Vector3.right;
        if (up == Vector3.zero) up = Vector3.up;
        if (up != Vector3.up) localPointer = Vector3.Cross(up, Vector3.up).normalized;

        localPointer *= radius;
        Quaternion rotator = Quaternion.AngleAxis(360f / segments, up);
        for (; points.Count <= segments; localPointer = rotator * localPointer) points.Enqueue(center + localPointer);

        Vector3 p = points.Dequeue();
        while (points.Count > 0)
        {
            Vector3 t = points.Dequeue();
            Gizmos.DrawLine(p, t);
            p = t;
        }

        Gizmos.color = oc;

    }
}