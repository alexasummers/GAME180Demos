using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringPriorities : MonoBehaviour
{
    [Tooltip("Each Component must implement ISteering and belong to this object.")]
    public Component[] steeringBehaviorsInOrder;

    private List<ISteering> steeringBehaviors = new List<ISteering>();
    public SteeringCore agent;

    public Vector3[] linearSteerings;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringPriorities behavior, but no SteeringCore assigned.");
            return;
        }


        for (int i = 0; i < steeringBehaviorsInOrder.Length; i++)
        {
            Component c = steeringBehaviorsInOrder[i];
            if (c.transform == this.transform && c as ISteering != null)
            {
                steeringBehaviors.Add(c as ISteering);
            }
            else
            {
                Debug.LogWarning("Component " + i.ToString() + " either does not belong to that GameObject, or does not implement ISteering.");
            }
        }

        linearSteerings = new Vector3[steeringBehaviors.Count];
    }
    
    // Update is called once per frame
    void Update()
    {
        // Check each for output until you find one
        SteeringOutput final = SteeringOutput.None();

        for (int i = 0; i < steeringBehaviors.Count && (final.linearAcceleration == Vector3.zero || final.angularAcceleration == 0); i++)
        {
            SteeringOutput steering = steeringBehaviors[i].GetSteering();

            linearSteerings[i] = steering.linearAcceleration;

            // Does it have output? We're only checking linear for now.
            if (final.linearAcceleration == Vector3.zero) final.linearAcceleration = steering.linearAcceleration;
            if (final.angularAcceleration == 0) final.angularAcceleration = steering.angularAcceleration;
        }

        debugFinal = final.linearAcceleration;

        agent.SetLinearAcceleration(final.linearAcceleration);
        agent.SetAngularAcceleration(final.angularAcceleration);
    }

    public Vector3 debugFinal;
}
