using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandscapeControl : MonoBehaviour
{
    public Component landscape;
    private Component lastLandscape;
    private ILandscape trueLandscape;

    public ParameterLineGizmos parameterLine;

    public float renderResolution = 0.1f;

    public bool showGraphGizmos = true;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (landscape == null) return;
        if (landscape != lastLandscape) {
            trueLandscape = landscape as ILandscape;
        }
    }

    public float Evaluate(float x) {
        return trueLandscape.Evaluate(x);
    }

    void OnDrawGizmos() {

        if (!showGraphGizmos) return;

        if (landscape == null) return;
        if (landscape != lastLandscape) {
            trueLandscape = landscape as ILandscape;
        }
        
        Color c = Gizmos.color;
        Gizmos.color = Color.cyan;

        float pv = trueLandscape.Evaluate(0);

        for (float x = renderResolution; x <= 100; x += renderResolution) {
            // previous
            Gizmos.DrawLine(transform.position + new Vector3(x - renderResolution, pv, 0), 
                            transform.position + new Vector3(x, pv = trueLandscape.Evaluate(x)));
        }



        Gizmos.color = c;
    }


}
