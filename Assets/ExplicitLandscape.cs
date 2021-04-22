using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ILandscape {

    public float Evaluate(float x);

}

public class ExplicitLandscape : MonoBehaviour, ILandscape
{

    public float[] fitnessValues;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float Evaluate(float x) {

        // fitnessValues shows the value from 0 to 100
        // first value is 0, last value is 100,
        // all else is interpolation

        float graphStepSize = 100f / (fitnessValues.Length - 1 );
        int lStep = Mathf.FloorToInt(x / graphStepSize),
              bStep = lStep + 1;

        if (lStep < 0 || bStep >= fitnessValues.Length) return 0;

        // interpolate
        float lVal = fitnessValues[lStep], bVal = fitnessValues[bStep];
        return Mathf.Lerp(lVal, bVal, (x - lStep * graphStepSize) / graphStepSize);
    }

}
