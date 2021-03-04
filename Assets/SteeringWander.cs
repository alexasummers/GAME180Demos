using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWander : MonoBehaviour, ISteering
{

    public SteeringCore agent;
    public float circleDistance = 3.0f;
    public float circleRadius = 1.0f;
    public Vector3 targetDisplacement = Vector3.zero;
    public float noiseFactor = 1f;

    private SteeringSeek mySeek;

    public bool showDebugGizmos = false;

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

        targetDisplacement = transform.forward * circleRadius;
    }

    // Update is called once per frame
    void Update()
    {
        if (circleRadius < 0) circleRadius = -circleRadius;
        if (circleRadius == 0) circleRadius = 0.01f;   // circle must always have a radius

        targetDisplacement.Normalize();
        targetDisplacement *= circleRadius;

        targetDisplacement = Quaternion.AngleAxis(Random.Range(0f, noiseFactor) - Random.Range(0f, noiseFactor), Vector3.up) * targetDisplacement;

        Vector3 seekSteering = mySeek.GetSteering(transform.position + transform.forward * circleDistance + targetDisplacement).linearAcceleration;

        agent.SetLinearAcceleration(seekSteering);
    }

    public SteeringOutput GetSteering()
    {
        if (circleRadius < 0) circleRadius = -circleRadius;
        if (circleRadius == 0) circleRadius = 0.01f;   // circle must always have a radius

        targetDisplacement.Normalize();
        targetDisplacement *= circleRadius;

        targetDisplacement = Quaternion.AngleAxis(Random.Range(0f, noiseFactor) - Random.Range(0f, noiseFactor), Vector3.up) * targetDisplacement;

        Vector3 seekSteering = mySeek.GetSteering(transform.position + transform.forward * circleDistance + targetDisplacement).linearAcceleration;

        return SteeringOutput.Get(seekSteering);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        GizmoUtils.DrawGizmoCircle(transform.position + transform.forward * circleDistance, circleRadius, Vector3.up, 32, new Color(1f, 1f, 0f, 0.5f));

        Color c = Gizmos.color;

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position + transform.forward * circleDistance + targetDisplacement, 0.3f);

        Gizmos.color = c;

    }
}
