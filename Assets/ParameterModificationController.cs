using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterModificationController : MonoBehaviour
{
    public SteeringObstacleAvoidance obstacleAvoidance;
    public ContactTiming contactTiming;

    public float timeoutPerTest = 20;
    private float timer = 0;

    private Vector3 startPosition;
    private Quaternion startOrientation;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = obstacleAvoidance.transform.position;
        startOrientation = obstacleAvoidance.transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer >= timeoutPerTest) {
            Debug.Log("Timeout");
            Reset();
        } 
        else 
        {
            if (contactTiming.Done) {
                Debug.Log("Contact rate \t" + contactTiming.ContactRate + "(" + contactTiming.ContactFrames + "/" + contactTiming.TotalFrames + ")");
                Reset();
            }
        }
    }

    void Reset() {
        obstacleAvoidance.GetComponent<SteeringCore>().DebugStop();
        obstacleAvoidance.transform.position = startPosition;
        obstacleAvoidance.transform.rotation = startOrientation;
        timer = 0;
        contactTiming.Reset();
    }
}
