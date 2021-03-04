using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SteeringPath : MonoBehaviour
{
    private List<Vector3> waypointLocations = new List<Vector3>();

    private List<float> stretchDistances = new List<float>();
    
    // Start is called before the first frame update
    void Start()
    {
        RefreshPath();
    }

    void RefreshPath()
    {
        // Find my children in order and make a path of lines
        waypointLocations.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            waypointLocations.Add(transform.GetChild(i).position);
        }

        float runningDistance = 0;
        for (int i = 0; i < waypointLocations.Count - 1; i++)
        {
            float stretchDistance = (waypointLocations[i + 1] - waypointLocations[i]).magnitude;
            stretchDistances.Add(runningDistance + stretchDistance);
            runningDistance += stretchDistance;
        }

    }

    // Update is called once per frame
    void Update()
    {
        RefreshPath();
    }

    private void OnDrawGizmos()
    {
        Color c = Gizmos.color;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypointLocations.Count - 1; i++)
        {
            Gizmos.DrawLine(waypointLocations[i], waypointLocations[i + 1]);
        }
    }

    public Vector3 GetClosestPathLocation(Vector3 outsider, float distanceOffset)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        if (waypointLocations.Count == 0) return outsider;

        Vector3 candidate = waypointLocations[0];
        float totalDistanceFromStart = 0;

        for (int i = 0; i < waypointLocations.Count - 1; i++)
        {
            Vector3 nc = ClosestPointOnSegment(waypointLocations[i], waypointLocations[i + 1], outsider);
            if ((candidate - outsider).sqrMagnitude > (nc - outsider).sqrMagnitude)
            {
                // nc is closer
                candidate = nc;

                if (i > 0)
                {
                    totalDistanceFromStart = stretchDistances[i - 1] + (candidate - waypointLocations[i]).magnitude;
                }
                else
                {
                    totalDistanceFromStart = (candidate - waypointLocations[i]).magnitude;
                }
            }
        }

        float targetDistance = totalDistanceFromStart + distanceOffset;
        float totalLength = stretchDistances[stretchDistances.Count - 1];
        targetDistance = Mathf.Clamp(targetDistance, 0, totalLength);

        if (targetDistance == 0) return waypointLocations[0];
        if (targetDistance == totalLength) return waypointLocations[waypointLocations.Count - 1];

        // Start from stretchDistances[0] and find the first time stretchDistances[i] is larger than targetDistance
        int stretchIndex = 0;
        while (stretchIndex < stretchDistances.Count && stretchDistances[stretchIndex] < targetDistance)
            stretchIndex++;

        float previousLength = 0;
        if (stretchIndex > 0) previousLength = stretchDistances[stretchIndex - 1];

        float remainingLength = targetDistance - previousLength;

        return waypointLocations[stretchIndex] + (waypointLocations[stretchIndex + 1] - waypointLocations[stretchIndex]).normalized * remainingLength;

    }

    private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 o)
    {
        // parameterized dot product = 0

        Vector3 p = b - a, c = o;
        float d = (p.x * p.x + p.y * p.y + p.z * p.z);
        if (d == 0) return a;   // length 0
        float t = (p.z * c.z + p.y * c.y + p.x * c.x - p.x * a.x - p.y * a.y - p.z * a.z) / d;

        if (t < 0) return a;
        if (t > 1) return b;
        return a + (b - a) * t;
    }
}
