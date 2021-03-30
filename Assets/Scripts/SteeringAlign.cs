using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SteeringCore))]
public class SteeringAlign : MonoBehaviour, ISteering
{
    private SteeringCore agent;
    public SteeringCore Agent
    {
        get { return agent; }
    }

    public Transform alignTarget;
    public Vector3 alignDirection;

    public float angleOfSatisfaction = 3.0f;    // degrees
    public float angleOfCloseness = 12.0f;  // degrees

    void Awake()
    {
        agent = GetComponent<SteeringCore>();
    }


    void Update()
    {

        // It works similarly to Kinematic Face,
        // but instead of *setting* the rotation,
        // we're *targeting* a rotation.

        // past the angle of closeness, we're targeting maximum rotation.
        // within the pocket, we're targeting a degree.
        // within satisfaction, we're targeting zero.


        // target rotation tr
        // current rotation cr
        // new rotation nr
        // angular rotation rate = wr
        // Time.fixedDeltaTime = dt
        // nr = cr + wr * dt
        // Solve for  nr = tr
        // tr = cr + wr * dt
        // tr - cr = wr * dt
        // wr = (tr - cr)/dt
        // but if tr and cr are expressed as parameterized...
        // r == max * scale
        // tr = max * tScale
        // cr = max * cScale

        // wr = (max * tScale - max * cScale)/dt
        // tScale is determined by parameterization.
        // cScale can be computed by current rotation and max rotation.
        // dt is known.


        

        agent.SetAngularAcceleration(GetSteering().angularAcceleration);
    }

    public bool noSteeringAtOrientation = true;
    public SteeringOutput GetSteering()
    {
        if (alignTarget != null) alignDirection = alignTarget.forward;

        float difference = Vector3.SignedAngle(transform.forward, alignDirection, Vector3.up);

        if (noSteeringAtOrientation && difference == 0)
        {
            return SteeringOutput.None();
        }

        float tScale = 0;
        if (difference < 0)
        {
            tScale = Mathf.Clamp((difference + angleOfSatisfaction) / (angleOfCloseness - angleOfSatisfaction), -1, 0);
        }
        else
        {
            tScale = Mathf.Clamp((difference - angleOfSatisfaction) / (angleOfCloseness - angleOfSatisfaction), 0, 1);
        }

        float cScale = agent.rotation / agent.maxRotation;

        float wr = agent.maxRotation * (tScale - cScale) / Time.fixedDeltaTime;

        return SteeringOutput.Get(wr);
    }
}
