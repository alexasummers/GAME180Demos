using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KinematicCore))]
public class KinematicFace : MonoBehaviour
{
    private KinematicCore agent;

    public Transform faceTarget;
    public Vector3 faceDirection;


    public float angleOfSatisfaction = 3.0f;    // degrees
    public float angleOfCloseness = 12.0f;  // degrees

    void Start()
    {
        agent = GetComponent<KinematicCore>();
    }


    void Update()
    {
        // Remember - face is a continuous operation.
        // Like Seek, it has a "radius" of satisfaction,
        // except in kinematics it's an angle of satisfaction.
        // We use an arrive-like behavior here as well;
        // stop turning so fast once you're inside it,
        // and stop turning at all once you're inside the angle of satisfaction.
        // The key is to make sure they can't cross over the entire angle of satisfaction in one frame.
        //
        // Our base rate is 720 degrees per second,
        // which at 60 FPS means 12 degrees per frame.
        // So really we want a window that's larger than 12 degrees.

        // first we need to know the target forward.
        if (faceTarget != null) faceDirection = faceTarget.position - transform.position;
        if (!agent.yVelocityEnabled) faceDirection.y = 0;

        // Almost makes it too easy!
        float difference = Vector3.SignedAngle(transform.forward, faceDirection, Vector3.up);

        if (difference < 0)
        {
            agent.SetRotation(Mathf.Clamp((difference + angleOfSatisfaction) / (angleOfCloseness - angleOfSatisfaction), -1, 0));
        }
        else
        {
            agent.SetRotation(Mathf.Clamp((difference - angleOfSatisfaction) / (angleOfCloseness - angleOfSatisfaction), 0, 1));
        }

        


    }
}
