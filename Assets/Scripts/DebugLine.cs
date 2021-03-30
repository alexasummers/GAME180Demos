using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLine : MonoBehaviour
{
    public Transform pointA, pointB;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos() {
        Color c = Gizmos.color;

        Gizmos.color = new Color(1f, 0, 1f, 1f);
        Gizmos.DrawLine(pointA.position, pointB.position);

        Gizmos.color = c;
    }

}
