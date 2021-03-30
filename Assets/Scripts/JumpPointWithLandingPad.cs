using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPointWithLandingPad : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform landingPad;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 ComputeRunUpVelocity(SteeringCore agent) {
        // we know the start point:
        Vector3 startPoint = transform.position;

        // We know the end point:
        Vector3 endPoint = landingPad.position;

        // We know the jump velocity:
        float jumpVelocity = agent.jumpPower;

        float sq = Mathf.Sqrt(2 * -9.8f * (endPoint.y - startPoint.y) + jumpVelocity * jumpVelocity);
        float t1 = (-jumpVelocity + sq) / -9.8f,
              t2 = (-jumpVelocity - sq) / -9.8f;

        if (t1 < 0) t1 = t2;
        if (t2 < 0) t2 = t1;

        if (t1 != t2) {
            Vector3 v1 = new Vector3((endPoint.x - startPoint.x) / t1, 0, (endPoint.z - startPoint.z) / t1);
            Vector3 v2 = new Vector3((endPoint.x - startPoint.x) / t2, 0, (endPoint.z - startPoint.z) / t2);

            if (v1.magnitude <= agent.maxSpeed) return v1;
            return v2;
        } else {
            Vector3 v2 = new Vector3((endPoint.x - startPoint.x) / t2, 0, (endPoint.z - startPoint.z) / t2);
            return v2;
        }


    }
}
