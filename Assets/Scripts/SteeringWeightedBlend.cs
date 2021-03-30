using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SteeringWeightedBlend : MonoBehaviour, ISteering
{
    public SteeringCore agent;

    public MonoBehaviour[] blendedBehaviors;
    public float[] blendedWeights;

    private List<ISteering> mySteerings = new List<ISteering>();
    private List<float> mySteeringWeights = new List<float>();

    void Awake()
    {
        if (agent == null) agent = GetComponent<SteeringCore>();
        if (agent == null)
        {
            this.enabled = false;
            Debug.LogError("Object " + transform.name + " has a SteeringWeightedBlend behavior, but no SteeringCore assigned.");
            return;
        }

        for (int i = 0; i < blendedBehaviors.Length; i++)
        {
            if (blendedBehaviors[i].transform == this.transform && blendedBehaviors[i] as ISteering != null)
            {
                mySteerings.Add(blendedBehaviors[i] as ISteering);
                mySteeringWeights.Add(blendedWeights.Length > i && blendedWeights[i] > 0 ? blendedWeights[i] : 0f);
            }
            else
            {
                Debug.LogWarning("Behavior " + blendedBehaviors[i].GetType().Name + " on object " + gameObject.name + " does not belong to that object, and has been removed.");
            }
        }
        blendedBehaviors = new MonoBehaviour[mySteerings.Count];
        blendedWeights = new float[mySteerings.Count];
        for (int i = 0; i < blendedBehaviors.Length; i++) blendedBehaviors[i] = mySteerings[i] as MonoBehaviour;
   
    }

    // Update is called once per frame
    void Update()
    {
        // rebalance
        float totalWeight = 0;
        for (int i = 0; i < mySteeringWeights.Count; i++) totalWeight += mySteeringWeights[i];
        if (totalWeight == 0) totalWeight = 1;
        for (int i = 0; i < mySteeringWeights.Count; i++)
        {
            mySteeringWeights[i] /= totalWeight;  // normalization
            blendedWeights[i] = mySteeringWeights[i];
        }

        SteeringOutput o = GetSteering();
        agent.SetAngularAcceleration(o.angularAcceleration);
        agent.SetLinearAcceleration(o.linearAcceleration);

    }

    public SteeringOutput GetSteering()
    {
        SteeringOutput total = SteeringOutput.None();

        for (int i = 0; i < mySteerings.Count; i++)
        {
            total += mySteerings[i].GetSteering() * mySteeringWeights[i];
        }

        return total;
    }
}
