using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringAvoid : MonoBehaviour, ISteering
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
            Debug.LogError("Object " + transform.name + " has a SteeringAvoid behavior, but no SteeringCore assigned.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // distance to target?
        if (targetTransform != null) targetPosition = targetTransform.position;

        Vector3 toTarget = targetPosition - agent.transform.position;
        if (!agent.yVelocityEnabled) toTarget.y = 0;
        float distanceToTarget = toTarget.magnitude;

        Vector3 normalVelocityToTarget = -toTarget.normalized;

        agent.SetVelocityMatch(normalVelocityToTarget, Mathf.Clamp((radiusOfApproach - distanceToTarget) / (radiusOfApproach - radiusOfSatisfaction), 0, 1));
    }

    public SteeringOutput GetSteering()
    {
        throw new System.NotImplementedException("TODO: Implement SteeringAvoid.GetSteering");
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

}
