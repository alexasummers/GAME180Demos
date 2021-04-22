using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParameterLineGizmos : MonoBehaviour
{
    public float lineWidth = 100;
    public float resolution = 1.0f;
    public float yStepHeight = 1.0f;

    private float xMouseLocation = 0.0f;
    private List<Vector3> evaluatedPoints = new List<Vector3>();

    public LandscapeControl landscape;

    private bool doHillClimbing = false;
    private bool useMomentum = false;
    private bool useAdaptiveResolution = false;
    private bool isDoingHillClimbing = false;
    private float lastX = 0;

    public float momentumFactor = 1.0f;
    public float adaptiveResolutionFactor = 1.3f;
    public float minResolution = 0.1f;
    private float adaptiveResolution = 0.1f;

    void Start()
    {
        
    }

    private float AddEvaluatedPoint(float x) {
        float v = landscape.Evaluate(x);
        evaluatedPoints.Add(transform.position + new Vector3(x, v, 0));
        return v;
    }

    private float leftMomentum = 0, rightMomentum = 0;

    private float bestX = 0;

    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        xMouseLocation = Mathf.Clamp(mouseWorld.x - transform.position.x, 0, lineWidth);
        
        if (Input.GetKeyDown(KeyCode.C)) evaluatedPoints.Clear();

        if (Input.GetKeyDown(KeyCode.P)) {
            // snapshot
            AddEvaluatedPoint(lastResolutionX);
        }


        if (isDoingHillClimbing) {
            // basic hill climbing: Look left and right one step,
            // go whichever direction improves the most.
            // If neither is better, we're done.

            float r = useAdaptiveResolution ? adaptiveResolution : resolution;
            float leftX = lastX - r, rightX = lastX + r;
            float leftV = -1, rightV = -1, currentV = landscape.Evaluate(lastX);

            if (landscape.Evaluate(lastX) > landscape.Evaluate(bestX)) {
                bestX = lastX;
            }

            if (leftX >= 0) leftV = AddEvaluatedPoint(leftX);
            if (rightX <= 100) rightV = AddEvaluatedPoint(rightX);

            if (useMomentum) {
                leftV += leftMomentum;
                rightV += rightMomentum;
            }

            if (useAdaptiveResolution) adaptiveResolution *= adaptiveResolutionFactor;

            if (leftV > currentV) {
                if (rightV > currentV) {
                    // which is bigger?
                    if (leftV > rightV) lastX = leftX; else lastX = rightX;
                } else {
                    lastX = leftX;
                }
            } else {
                if (rightV > currentV) {
                    // go right
                    lastX = rightX;
                } else {
                    if (useAdaptiveResolution) {
                        if (adaptiveResolution > minResolution) 
                        {
                            adaptiveResolution /= adaptiveResolutionFactor * adaptiveResolutionFactor;
                        } 
                        else 
                        {
                            Debug.Log("Done hill climbing, best parameter value is " + bestX + " with fitness value " + landscape.Evaluate(bestX));
                            isDoingHillClimbing = false;
                        }
                    } 
                    else 
                    {
                        Debug.Log("Done hill climbing, best parameter value is " + bestX + " with fitness value " + landscape.Evaluate(bestX));
                        isDoingHillClimbing = false;
                    }
                }
            }

            leftMomentum += (leftV - currentV) * momentumFactor;
            rightMomentum += (rightV - currentV) * momentumFactor;

            // bleed momentum off
            leftMomentum *= 0.9f;
            rightMomentum *= 0.9f;

        }


        if (doHillClimbing) {
            doHillClimbing = false;
            isDoingHillClimbing = true;

            if (evaluatedPoints.Count > 0) {
                Vector3 lastPoint = evaluatedPoints[evaluatedPoints.Count - 1];
                Debug.Log("lastPoint is " + lastPoint);
                evaluatedPoints.Clear();
                evaluatedPoints.Add(lastPoint);
                lastX = lastPoint.x - transform.position.x;
                Debug.Log("lastX is " + lastX);
            } 
            else 
            {
                lastX = Mathf.Floor(50 * resolution);
                AddEvaluatedPoint(lastX);
                Debug.Log("new lastX");
            }
            bestX = lastX;
        }


    }

    void OnGUI() {
        if (GUILayout.Button("Hill Climbing")) {
            doHillClimbing = true;
            useMomentum = false;
            useAdaptiveResolution = false;
        }

        if (GUILayout.Button("Hill Climbing With Momentum")) {
            doHillClimbing = true;
            useMomentum = true;
            useAdaptiveResolution = false;
        }

        if (GUILayout.Button("Hill Climbing with Adaptive Resolution")) {
            doHillClimbing = true;
            useMomentum = false;
            useAdaptiveResolution = true;
            adaptiveResolution = resolution;
        }
        
    }

    float lastResolutionX = 0;

    void OnDrawGizmos() 
    {
        Color c = Gizmos.color;

        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * lineWidth);

        Gizmos.color = Color.gray;
        for (float x = 0.0f; x <= lineWidth; x += resolution) {
            Vector3 above = transform.position + new Vector3(x, yStepHeight, 0),
                    below = transform.position + new Vector3(x, -yStepHeight, 0);

            if (xMouseLocation >= x && xMouseLocation < x + resolution) {
                lastResolutionX = x;
                Gizmos.color = Color.green;
            } else {
                Gizmos.color = Color.gray;
            }

            Gizmos.DrawLine(above, below);
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < evaluatedPoints.Count; i++) {
            Gizmos.DrawSphere(evaluatedPoints[i], Mathf.Clamp(resolution * 0.6f, 0.2f, 1f));
        }

        Gizmos.color = c;

    }
}
