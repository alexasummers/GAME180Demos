﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicArrive : MonoBehaviour
{
    public KinematicCore agent;

    [Tooltip("The target transform to seek to. If not set, Target Position will be used instead.")]
    public Transform targetTransform;
    [Tooltip("The fallback target position to seek to. Y match will be disregarded if KinematicCore.yVelocityEnabled is false.")]
    public Vector3 targetPosition;
    public float radiusOfSatisfaction = 0.1f;   // 10cm
    public float radiusOfApproach = 3.0f;

    public bool showDebugGizmos = false;

    void Awake()
    {
        // Standard dependency pattern; disable the component if it doesn't have what it needs, and show an error.
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a KinematicSeek behavior, but no KinematicCore assigned.");
        }
    }

    void Update()
    {
        // Get the target position from the target transform, if any.
        if (targetTransform != null) targetPosition = targetTransform.position;

        // Check for satisfaction; account for y position, which may be different!
        Vector3 positionDifference = targetPosition - transform.position;
        if (!agent.yVelocityEnabled) positionDifference.y = 0;

        float distance = Vector3.Magnitude(positionDifference);

        float scale = Mathf.Clamp((distance - radiusOfSatisfaction) / (radiusOfApproach - radiusOfSatisfaction), 0, 1);

        agent.SetVelocity(positionDifference, scale);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1);
    }


}
