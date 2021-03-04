using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicFlee : MonoBehaviour
{
    public KinematicCore agent;

    [Tooltip("The target transform to seek to. If not set, Target Position will be used instead.")]
    public Transform targetTransform;
    [Tooltip("The fallback target position to seek to. Y match will be disregarded if KinematicCore.yVelocityEnabled is false.")]
    public Vector3 targetPosition;

    void Start()
    {
        // Standard dependency pattern; disable the component if it doesn't have what it needs, and show an error.
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a KinematicFlee behavior, but no KinematicCore assigned.");
        }
    }

    void Update()
    {
        // Get the target position from the target transform, if any.
        if (targetTransform != null) targetPosition = targetTransform.position;

        Vector3 positionDifference = targetPosition - transform.position;

        agent.SetVelocity(-positionDifference, 1);
    }
}
