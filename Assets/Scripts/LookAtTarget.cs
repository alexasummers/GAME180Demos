using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{

    public Transform lookTarget;
    private Vector3 startPositionDelta;
    private SteeringArrive arrive;

    void Awake()
    {
        startPositionDelta = lookTarget.position - transform.position;
        arrive = GetComponent<SteeringArrive>();
    }

    // Update is called once per frame
    void Update()
    {
        arrive.targetPosition = lookTarget.position - startPositionDelta;
        transform.LookAt(lookTarget, Vector3.up);
    }
}
